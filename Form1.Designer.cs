namespace FaceDectection
{
    partial class FaceScan
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.Scan_BTN = new System.Windows.Forms.Button();
            this.Photo_PB = new System.Windows.Forms.PictureBox();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.Photo_PB)).BeginInit();
            this.SuspendLayout();
            // 
            // Scan_BTN
            // 
            this.Scan_BTN.Location = new System.Drawing.Point(782, 28);
            this.Scan_BTN.Name = "Scan_BTN";
            this.Scan_BTN.Size = new System.Drawing.Size(88, 23);
            this.Scan_BTN.TabIndex = 0;
            this.Scan_BTN.Text = "Toggle Scan";
            this.Scan_BTN.UseVisualStyleBackColor = true;
            this.Scan_BTN.Click += new System.EventHandler(this.Scan_BTN_Click);
            // 
            // Photo_PB
            // 
            this.Photo_PB.Location = new System.Drawing.Point(22, 28);
            this.Photo_PB.Name = "Photo_PB";
            this.Photo_PB.Size = new System.Drawing.Size(746, 404);
            this.Photo_PB.TabIndex = 1;
            this.Photo_PB.TabStop = false;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // FaceScan
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 483);
            this.Controls.Add(this.Photo_PB);
            this.Controls.Add(this.Scan_BTN);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "FaceScan";
            this.Text = "FaceScan";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Photo_PB)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Scan_BTN;
        private System.Windows.Forms.PictureBox Photo_PB;
        private System.Windows.Forms.Timer timer1;
    }
}

