using System;
using System.Drawing;
using System.Drawing.Imaging;
using KGySoft.Drawing;

using KGySoft.WinForms.Controls;

#nullable enable

namespace KGySoft.WinForms.Test.Forms
{
    internal partial class frmImageViewer : ControlsTestBaseForm
    {
        private Bitmap? smallBitmap;
        private Bitmap? largeBitmap;
        private Metafile? metafile;

        public frmImageViewer()
        {
            InitializeComponent();
        }

        private static Metafile GenerateMetafile()
        {
            //Set up reference Graphic
            Graphics refGraph = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr hdc = refGraph.GetHdc();
            Metafile result = new Metafile(hdc, new Rectangle(0, 0, 100, 100), MetafileFrameUnit.Pixel, EmfType.EmfOnly, "Test");

            //Draw some silly drawing
            using (var g = Graphics.FromImage(result))
            {
                var r = new Rectangle(0, 0, 100, 100);
                var leftEye = new Rectangle(20, 20, 20, 30);
                var rightEye = new Rectangle(60, 20, 20, 30);
                g.FillEllipse(Brushes.Yellow, r);
                g.FillEllipse(Brushes.White, leftEye);
                g.FillEllipse(Brushes.White, rightEye);
                g.DrawEllipse(Pens.Black, leftEye);
                g.DrawEllipse(Pens.Black, rightEye);
                g.DrawBezier(Pens.Red, new Point(10, 50), new Point(10, 100), new Point(90, 100), new Point(90, 50));
            }

            refGraph.ReleaseHdc(hdc); //cleanup
            refGraph.Dispose();
            return result;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                components?.Dispose();
                smallBitmap?.Dispose();
                largeBitmap?.Dispose();
                metafile?.Dispose();
            }
        }

        private void AdvancedRadioButton_CheckedChanged(object sender, System.EventArgs e)
        {
            var radioButton = (AdvancedRadioButton)sender;
            if (!radioButton.Checked)
                return;

            Image? image = radioButton.Name switch
            {
                nameof(rbSmallBitmap) => smallBitmap ??= Icons.SystemError.ExtractNearestBitmap(new Size(16, 16), PixelFormat.Format32bppArgb),
                nameof(rbLargeBitmap) => largeBitmap ??= (Bitmap)Screenshot.CaptureScreenshot(),
                nameof(rbMetafile) => metafile ??= GenerateMetafile(),
                _ => null
            };

            imageViewer.Image = pictureBox.Image = image;

            //Image? prevPictureBoxImage = pictureBox.Image;
            //Image? prevImageViewerImage = imageViewer.Image;

            //// cloning the image is necessary to avoid "bitmap region is already locked"
            //// because the ImageViewer accesses the image in a separate thread and PictureBox does not lock the image
            ////pictureBox.Image = image;
            //prevPictureBoxImage?.Dispose();

            //imageViewer.Image = image is Bitmap bmp ? bmp.CloneCurrentFrame() : (Image?)image?.Clone();
            //prevImageViewerImage?.Dispose();
        }
    }
}
