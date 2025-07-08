#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: frmImageViewer.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2025 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

#region Usings

using System;
using System.Drawing;
using System.Drawing.Imaging;

using KGySoft.Drawing;
using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms.Example.Forms
{
    internal partial class frmImageViewer : ControlsTestBaseForm
    {
        #region Fields

        private Bitmap? smallBitmap;
        private Bitmap? largeBitmap;
        private Metafile? metafile;

        #endregion

        #region Constructors

        public frmImageViewer()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        #region Static Methods

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

        #endregion

        #region Instance Methods

        #region Protected Methods

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

        #endregion

        #region Event handlers

        private void AdvancedRadioButton_CheckedChanged(object sender, EventArgs e)
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
            if (!imageViewer.AutoZoom)
                imageViewer.Zoom = 1.0f;
        }

        #endregion

        #endregion

        #endregion
    }
}
