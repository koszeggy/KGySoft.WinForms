#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckBoxBaseAdapter.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
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
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.Drawing.Imaging;
using KGySoft.Drawing.Shapes;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class CheckBoxBaseAdapter : CheckableControlBaseAdapter
    {
        #region Constants

        protected const int FlatCheckSize = 11;

        #endregion

        #region Fields

        [ThreadStatic]private static Bitmap? checkImageChecked;
        [ThreadStatic]private static Color checkImageCheckedBackColor;
        [ThreadStatic]private static Bitmap? checkImageIndeterminate;
        [ThreadStatic]private static Color checkImageIndeterminateBackColor;

        #endregion

        #region Properties

        protected AdvancedCheckBox CheckBoxInstance => (AdvancedCheckBox)ButtonInstance;

        #endregion

        #region Constructors

        internal CheckBoxBaseAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods
        
        #region Protected Methods

        protected static void DrawCheckOnly(Graphics g, LayoutData layout, ColorData colors, Color checkColor, bool disabledColors, ControlAppearanceState state)
        {
            if (state.CheckState == CheckState.Unchecked)
                return;

            if (!state.Enabled && disabledColors)
                checkColor = colors.ButtonShadow;
            else if (state.CheckState == CheckState.Indeterminate && disabledColors)
                checkColor = VisualStyleHelper.HighContrast ? colors.Highlight : colors.ButtonShadow;
            
            Rectangle checkBounds = layout.CheckBounds;
            if (checkBounds.Width == FlatCheckSize)
            {
                checkBounds.Width++;
                checkBounds.Height++;
            }

            checkBounds.Width++;
            checkBounds.Height++;
            Bitmap image = state.CheckState == CheckState.Checked
                ? GetCheckBoxImage(checkColor, checkBounds, ref checkImageCheckedBackColor, ref checkImageChecked)
                : GetCheckBoxImage(checkColor, checkBounds, ref checkImageIndeterminateBackColor, ref checkImageIndeterminate);
            
            if (layout.Options.DotNetOneButtonCompat)
                checkBounds.Y--;
            else
                checkBounds.Y -= 2;

            g.DrawImageColorized(image, checkBounds, checkColor);
        }

        #endregion

        #region Private Methods

        private static Bitmap GetCheckBoxImage(Color checkColor, Rectangle fullSize, ref Color cacheCheckColor, ref Bitmap? cacheCheckImage)
        {
            if (cacheCheckImage != null && cacheCheckColor.ToArgb() == checkColor.ToArgb() && cacheCheckImage.Size == fullSize.Size)
                return cacheCheckImage;

            cacheCheckImage?.Dispose();

            var result = new Bitmap(fullSize.Width, fullSize.Height);
            if (OSHelper.IsWindows)
            {
                RECT rect = RECT.FromXYWH(0, 0, fullSize.Width, fullSize.Height);
                using (Graphics g = Graphics.FromImage(result))
                {
                    IntPtr hdc = g.GetHdc();
                    try
                    {
                        User32.DrawFrameControl(hdc, ref rect, 2, 1);
                    }
                    finally
                    {
                        g.ReleaseHdcInternal(hdc);
                    }
                }

                result.MakeTransparent();
            }
            else
            {
                using IReadWriteBitmapData bitmapData = result.GetReadWriteBitmapData();
                int checkHeight = fullSize.Height / 5;
                Color32 c = Color.Black;
                int start = (int)(fullSize.Width * 0.25f);
                int mid = (int)(fullSize.Width * 0.4f);
                int end = (int)(fullSize.Width * 0.7f);
                int y = (int)(fullSize.Height * 0.4f);
                for (int x = start; x < end; x++)
                {
                    bitmapData.DrawLine(c, x, y, x, y + checkHeight);
                    y += x < mid ? +1 : -1;
                }
            }

            cacheCheckImage = result;
            cacheCheckColor = checkColor;
            return cacheCheckImage;
        }

        #endregion

        #endregion

        #region Instance Methods

        #region Internal Methods

        internal override LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions options = base.CommonLayout(state);
            options.CheckAlign = CheckBoxInstance.CheckAlign;
            options.TextOffset = false;
            options.ShadowedText = !state.Enabled;
            options.LayoutRtl = RightToLeft.Yes == ButtonInstance.RightToLeft;
            return options;
        }

        #endregion

        #region Protected Methods

        protected void AdjustFocusRectangle(ControlAppearanceState state, LayoutData layout)
        {
            if (String.IsNullOrEmpty(state.Text))
            {
                // When a CheckBox has no text, AutoSize sets the size to zero and thus there's no place around which
                // to draw the focus rectangle. So, when AutoSize == true we want the focus rectangle to be rendered
                // inside the box. Otherwise, it should encircle all the available space next to the box (like it's
                // done in WPF and ComCtl32).
                layout.Focus = ButtonInstance.AutoSize ? Rectangle.Inflate(layout.CheckBounds, -2, -2) : layout.Field;
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
