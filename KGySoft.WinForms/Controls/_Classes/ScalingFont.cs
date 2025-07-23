#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ScalingFont.cs
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
using System.Diagnostics;
using System.Drawing;

#endregion

namespace KGySoft.WinForms.Controls
{
    [DebuggerDisplay("{Font} - Scale: {scale}")]
    internal sealed class ScalingFont : IDisposable
    {
        #region Fields

        private PointF scale;
        private Font systemScaleFont;
        private Font scaledFont;
        private bool disposeSystemScaleFont;
        private bool disposeScaledFont;

        #endregion

        #region Properties

        internal Font Font
        {
            get
            {
                EnsureValid();
                return scaledFont;
            }
        }

        internal PointF CurrentScale => scale;

        #endregion

        #region Constructors

        internal ScalingFont(Font font, PointF scale)
        {
            this.scale = scale;
            bool dispose = false;
            if (font.Unit is not GraphicsUnit.Point)
            {
                dispose = true;
                font = CloneWithPoints(font);
            }

            // initializing with system scale
            if (scale == ScaleHelper.SystemScale)
            {
                disposeSystemScaleFont = dispose;
                scaledFont = systemScaleFont = font;
                return;
            }

            // initializing with a custom scale
            scaledFont = font;
            disposeScaledFont = dispose;
            systemScaleFont = ScaleFrom(font, scale);
            disposeSystemScaleFont = true;
        }

        #endregion

        #region Methods

        #region Static Methods

        private static Font CloneWithPoints(Font font)
        {
            try
            {
                return new Font(font.FontFamily, font.SizeInPoints, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
            }
            catch (ArgumentException)
            {
                // Font.SizeInPoints (and Font.Height) may throw an exception if the font is already disposed.
                // Font.Size does not throw, though this way we reinterpret the font size in Points without an actual conversion.
                return new Font(font.FontFamily, font.Size, font.Style);
            }
        }

        private static Font ScaleTo(Font font, PointF scale)
        {
            if (scale == ScaleHelper.SystemScale)
                return font;

            float ratio = scale.Y / ScaleHelper.SystemScale.Y;
            return new Font(font.FontFamily, font.SizeInPoints * ratio, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
        }

        private static Font ScaleFrom(Font font, PointF scale)
        {
            if (scale == ScaleHelper.SystemScale)
                return font;

            float ratio = ScaleHelper.SystemScale.Y / scale.Y;
            return new Font(font.FontFamily, font.SizeInPoints * ratio, font.Style, GraphicsUnit.Point, font.GdiCharSet, font.GdiVerticalFont);
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public void Dispose()
        {
            if (disposeSystemScaleFont)
                systemScaleFont.Dispose();
            if (disposeScaledFont)
                scaledFont.Dispose();
        }

        #endregion

        #region Internal Methods

        internal void Scale(PointF newScale)
        {
            if (newScale == scale)
                return;

            if (disposeScaledFont)
                scaledFont.Dispose();
            scale = newScale;

            if (scale == ScaleHelper.SystemScale)
            {
                scaledFont = systemScaleFont;
                disposeScaledFont = false;
                return;
            }

            scaledFont = ScaleTo(systemScaleFont, scale);
            disposeScaledFont = true;
        }

        internal Font GetScaled(PointF newScale)
        {
            Scale(newScale);
            disposeScaledFont = false; // Do not dispose the scaled font when returning it, because it is used by the caller.
            return scaledFont;
        }

        internal void Reset()
        {
            bool areSame = ReferenceEquals(systemScaleFont, scaledFont);
            var newSystemFont = CloneWithPoints(systemScaleFont);
            var newScaledFont = areSame ? newSystemFont : CloneWithPoints(scaledFont);
            if (disposeSystemScaleFont)
                systemScaleFont.Dispose();
            if (disposeScaledFont && !areSame)
                scaledFont.Dispose();

            systemScaleFont = newSystemFont;
            scaledFont = newScaledFont;
        }

        internal void ResetFrom(Font newFont, PointF newScale)
        {
            if (ReferenceEquals(newFont, scaledFont) && scale == newScale)
                return;

            if (disposeScaledFont)
                scaledFont.Dispose();
            if (newFont.Unit is not GraphicsUnit.Point)
            {
                scaledFont = CloneWithPoints(newFont);
                disposeScaledFont = true;
            }
            else
            {
                scaledFont = newFont;
                disposeScaledFont = false;
            }

            scale = newScale;

            // reset with system scale
            if (scale == ScaleHelper.SystemScale)
            {
                if (ReferenceEquals(systemScaleFont, scaledFont))
                    return;
                if (disposeSystemScaleFont)
                    systemScaleFont.Dispose();
                disposeSystemScaleFont = disposeScaledFont;
                systemScaleFont = scaledFont;
                return;
            }

            // reset with a custom scale
            if (disposeSystemScaleFont)
                systemScaleFont.Dispose();
            systemScaleFont = ScaleFrom(scaledFont, scale);
            disposeSystemScaleFont = true;
        }

        #endregion

        #region Private Methods

        private void EnsureValid()
        {
            // Controls may dispose the even the explicitly set font when performing a scaling operation.
            // Happens often in older frameworks, especially with per-monitor DPI awareness level V1.
            if (!scaledFont.IsDisposed())
                return;

            bool areSame = ReferenceEquals(systemScaleFont, scaledFont);
            scaledFont = CloneWithPoints(scaledFont);
            disposeScaledFont = true;

            if (!areSame)
                return;

            disposeSystemScaleFont = false;
            systemScaleFont = scaledFont;
        }

        #endregion

        #endregion

        #endregion
    }
}
