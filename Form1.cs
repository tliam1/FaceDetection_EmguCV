using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
/*using DlibDotNet; I can use emgu ddn */
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Dai;
using Emgu.CV.Dnn;
using Emgu.CV.Util;
using System.Runtime.InteropServices;

namespace FaceDectection
{
    public partial class FaceScan : Form
    {
        private VideoCapture cap;
        private CascadeClassifier cascadeClassifier;
        private bool camIsActive = false;
        private Net ageNet; 
        private Net genderNet; 
        public FaceScan()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cap = new VideoCapture(0); // 0th camera (so built in webcam for me :) )
            // cap.ImageGrabbed += ProcessFrame;
            cascadeClassifier =
                new CascadeClassifier(Application.StartupPath +
                                      "/haarcascade_frontalface_default.xml");
            /*SRC: https://github.com/spmallick/learnopencv/tree/master/AgeGender */
            ageNet = LoadNet("age_deploy.prototxt", "age_net.caffemodel");
            genderNet = LoadNet("gender_deploy.prototxt", "gender_net.caffemodel");
            // Console.WriteLine($"DOES CASCADE CLASSIFIER EXIST: {cascadeClassifier != null}");
            /*Console.WriteLine($"{ageNet}");
            Console.WriteLine($"{genderNet}");*/
        }

        private void ProcessFrame(/*object sender, EventArgs e*/)
        {

            /*
            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ProcessFrame(sender, e)));
                return;
            }*/
            if (!camIsActive) return;

            Mat frame = new Mat();
            cap.Retrieve(frame);

            if (Photo_PB.InvokeRequired)
            {
                Photo_PB.Invoke(new Action(() => ProcessFrame()));
                return;
            }

            if (frame.IsEmpty)
            {
                Console.WriteLine("Retrieved frame is empty.");
                return;
            }

            if (Photo_PB.Image != null)
            {
                Photo_PB.Image.Dispose();
            }
            /*
             * A Bitmap file displays a small dots in a pattern that,
             * when viewed from afar, creates an overall image and that
             * Bitmap image is a grid made of rows and columns where a
             * specific cell is given a value that fills it in or leaves
             * it blank thus creating an image out of the data.
             */
            
            // use nextFrame to process the image
            // bgr = blue green red values
            using (Image<Bgr, byte> nextFrame = frame.ToImage<Bgr, byte>())
            {
                if (nextFrame != null)
                {
                    var grayframe = nextFrame.Convert<Gray, byte>();
                    var faces = cascadeClassifier.DetectMultiScale(
                        grayframe,                          // Input image
                        1.2,                      // Scale factor
                        4                       // Minimum neighbor (two low and it detects multiple facial expressions on a single person)
                    ); 
                    Console.WriteLine(faces);
                    //var faces = grayframe.DetectHaarCascade (DEPRECIATED) 
                    foreach (var face in faces)
                    {
                        nextFrame.Draw(face, new Bgr(Color.BurlyWood), 3); //the detected face(s) is highlighted here using a box that is drawn around it/them

                        // HA if this works im stupid
                        // Extract the face region
                        var faceRegion = new Mat(frame, face);
                        var faceImage = faceRegion.ToImage<Bgr, byte>();

                        // Convert the face region to HSV (HUE (0 = red, 120 = green, 240 = blue), SATURATION (0-100%), VALUE (brightness))
                        var faceImageHsv = faceImage.Convert<Hsv, byte>();

                        // Define skin color range in HSV
                        var lowerSkin = new Hsv(0, 30, 60);
                        var upperSkin = new Hsv(20, 150, 255);

                        // Threshold the HSV image to get only skin colors
                        var skinMask = faceImageHsv.InRange(lowerSkin, upperSkin);

                        // Calculate the DOMINANT skin color in the face region
                        Hsv dominantSkinColor = GetDominantColor(faceImageHsv, skinMask.Mat);
                        //string categorizedSkinColor = CategorizeSkinColor(dominantSkinColor); OLD AND BAD

                        // Convert the dominant HSV color to BGR
                        Image<Hsv, byte> singleColorImage = new Image<Hsv, byte>(1, 1, dominantSkinColor);
                        Image<Bgr, byte> singleColorImageBgr = singleColorImage.Convert<Bgr, byte>();
                        Bgr dominantBgrColor = singleColorImageBgr[0, 0];

                        // Draws a box with the dominant skin color
                        var colorBoxLocation = new System.Drawing.Rectangle(face.Right + 10, face.Top, face.Width / 10, face.Height);
                        nextFrame.Draw(colorBoxLocation, dominantBgrColor, -1); 

                        // Uses neural network to predict age and gender
                        var predictedAge = PredictAge(ageNet, faceRegion);
                        var predictedGender = PredictGender(genderNet, faceRegion);

                        // Draw age and gender predictions
                        CvInvoke.PutText(nextFrame, $"Age: {predictedAge}", new System.Drawing.Point(face.Left, face.Bottom + 20), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.8, new MCvScalar(0, 0, 255), 2);
                        CvInvoke.PutText(nextFrame, $"Gender: {predictedGender}", new System.Drawing.Point(face.Left, face.Bottom + 50), Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.8, new MCvScalar(255, 0, 0), 2);
                        /*Console.WriteLine($"PREDICTED AGE: {predictedAge}... PREDICTED GENDER: {predictedGender}");
                        // Output the average skin color (Hue, Saturation, Value)
                        Console.WriteLine($"Average skin color (HSV): {dominantSkinColor.Hue}, {dominantSkinColor.Satuation}, {dominantSkinColor.Value}.");*/
                    }
                    Photo_PB.Image = nextFrame.ToBitmap();
                }
            }
        }
        private Hsv GetDominantColor(Image<Hsv, byte> hsvImage, Mat mask)
        {
            // split the HSV image into separate channels
            VectorOfMat hsvPlanes = new VectorOfMat();
            CvInvoke.Split(hsvImage, hsvPlanes);

            // parameters
            int[] histSize = { 256 };
            float[] range = { 0, 256 };
            int[] channels = { 0 };

            // histograms for H, S, and V channels
            Mat histH = new Mat();
            Mat histS = new Mat();
            Mat histV = new Mat();

            CvInvoke.CalcHist(new VectorOfMat(hsvPlanes[0]), channels, mask, histH, histSize, range, false);
            CvInvoke.CalcHist(new VectorOfMat(hsvPlanes[1]), channels, mask, histS, histSize, range, false);
            CvInvoke.CalcHist(new VectorOfMat(hsvPlanes[2]), channels, mask, histV, histSize, range, false);

            // Find the dominants in each histogram
            double minVal = 0, maxVal = 0;
            System.Drawing.Point minLoc = new System.Drawing.Point(), maxLoc = new System.Drawing.Point();

            CvInvoke.MinMaxLoc(histH, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            int dominantH = maxLoc.Y;

            CvInvoke.MinMaxLoc(histS, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            int dominantS = maxLoc.Y;

            CvInvoke.MinMaxLoc(histV, ref minVal, ref maxVal, ref minLoc, ref maxLoc);
            int dominantV = maxLoc.Y;

            // Return the dominant HSV color
            return new Hsv(dominantH, dominantS, dominantV);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (!camIsActive) return;

            // Run ProcessFrame on a different thread
            Task.Run(() => ProcessFrame());
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cap != null)
            {
                cap.Dispose();
            }
        }

        // Toggles the camera on & pauses. Look up how to turn it off later
        private void Scan_BTN_Click(object sender, EventArgs e)
        {
            if(!camIsActive)
                cap.Start();
            else
                cap.Pause();
            camIsActive = !camIsActive;
        }

        /* See https://github.com/spmallick/learnopencv/tree/master/AgeGender for some examples in c++ and python */
        private string PredictAge(Net ageNet, Mat faceImage)
        {
            // 
            var blob = DnnInvoke.BlobFromImage(faceImage, 1.0, new Size(227, 227), new MCvScalar(78.4263377603, 87.7689143744, 114.895847746), false, false);
            ageNet.SetInput(blob);
            try
            {
                var preds = ageNet.Forward();
                var ageList = new string[] { "0-2", "4-6", "8-12", "15-20", "25-32", "38-43", "48-53", "60-100" };

                // Convert predictions to float array
                float[] data = new float[preds.Total.ToInt32()];
                Marshal.Copy(preds.DataPointer, data, 0, data.Length);

                var maxIndex = ArgMax(data);
                return ageList[maxIndex];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during model inference: {ex.Message}");
                throw;
            }
        }

        private static string PredictGender(Net genderNet, Mat faceImage)
        {
            var blob = DnnInvoke.BlobFromImage(faceImage, 1.0, new Size(227, 227), new MCvScalar(78.4263377603, 87.7689143744, 114.895847746), false, false);
            genderNet.SetInput(blob);
            try
            {
                var preds = genderNet.Forward();
                var genderList = new string[] { "Male", "Female" };

                // Convert predictions to float array
                float[] data = new float[preds.Total.ToInt32()];
                Marshal.Copy(preds.DataPointer, data, 0, data.Length);

                var maxIndex = ArgMax(data);
                return genderList[maxIndex];
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private static Net LoadNet(string protocalPath, string modelPath)
        {
            return DnnInvoke.ReadNetFromCaffe(protocalPath, modelPath);
        }
        // the most simple max function known to man kind :)
        public static int ArgMax(float[] array)
        {
            float max = array[0];
            int maxIndex = 0;
            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] > max)
                {
                    max = array[i];
                    maxIndex = i;
                }
            }
            return maxIndex;
        }
    }
}
