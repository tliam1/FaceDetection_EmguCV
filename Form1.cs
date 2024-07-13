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
using Emgu.CV;
using Emgu.Util;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using Emgu.CV.Dai;

namespace FaceDectection
{
    public partial class FaceScan : Form
    {
        private VideoCapture cap;
        private CascadeClassifier cascadeClassifier;
        private bool camIsActive = false;
        public FaceScan()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cap = new VideoCapture(0); // 0th camera (so built in webcam for me :) )
            cap.ImageGrabbed += ProcessFrame;
            cascadeClassifier =
                new CascadeClassifier(Application.StartupPath +
                                      "/haarcascade_frontalface_default.xml");
            Console.WriteLine($"DOES CASCADE CLASSIFIER EXIST: {cascadeClassifier != null}");
        }

        private void ProcessFrame(object sender, EventArgs e)
        {
            if (!camIsActive) return;

            if (InvokeRequired)
            {
                BeginInvoke(new Action(() => ProcessFrame(sender, e)));
                return;
            }

            Mat frame = new Mat();
            cap.Retrieve(frame);

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
                        Console.WriteLine("DRAWING BOX AROUND FACE!");
                    }

                    Photo_PB.Image = nextFrame.ToBitmap();
                }
            }

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            /*using (Image<Bgr, byte> nextFrame = cap.QueryFrame())
            {

            }*/
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (cap != null)
            {
                cap.Dispose();
            }
        }

        private void Scan_BTN_Click(object sender, EventArgs e)
        {
            if(!camIsActive)
                cap.Start();
            else
                cap.Pause();
            camIsActive = !camIsActive;
        }
    }
}
