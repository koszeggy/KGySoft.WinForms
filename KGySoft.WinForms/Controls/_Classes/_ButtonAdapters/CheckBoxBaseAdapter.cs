#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckBoxBaseAdapter.cs
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
using System.Runtime.InteropServices;
using System.Windows.Forms;

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

        private static void DrawCheckOnly(int checkSize, Graphics g, LayoutData layout, ColorData colors, Color checkColor, bool disabledColors, ControlAppearanceState state)
        {
            if (state.CheckState == CheckState.Unchecked)
                return;

            if (!state.Enabled && disabledColors)
                checkColor = colors.ButtonShadow;
            else if (state.CheckState == CheckState.Indeterminate && disabledColors)
                checkColor = VisualStyleHelper.HighContrast ? colors.Highlight : colors.ButtonShadow;
            
            Rectangle checkBounds = layout.CheckBounds;
            if (checkBounds.Width == checkSize)
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

        private static Bitmap GetCheckBoxImage(Color checkColor, Rectangle fullSize, ref Color cacheCheckColor, ref Bitmap? cacheCheckImage)
        {
            if (cacheCheckImage == null || !cacheCheckColor.Equals(checkColor) || cacheCheckImage.Width != fullSize.Width || cacheCheckImage.Height != fullSize.Height)
            {
                if (cacheCheckImage != null)
                {
                    cacheCheckImage.Dispose();
                    cacheCheckImage = null;
                }

                RECT rect = RECT.FromXYWH(0, 0, fullSize.Width, fullSize.Height);
                Bitmap image = new Bitmap(fullSize.Width, fullSize.Height);
                Graphics wrapper = Graphics.FromImage(image);
                wrapper.Clear(Color.Transparent);
                IntPtr hdc = wrapper.GetHdc();
                try
                {
                    User32.DrawFrameControl(new HandleRef(wrapper, hdc), ref rect, 2, 1);
                }
                finally
                {
                    wrapper.ReleaseHdcInternal(hdc);
                    wrapper.Dispose();
                }

                image.MakeTransparent();
                cacheCheckImage = image;
                cacheCheckColor = checkColor;
            }
            return cacheCheckImage;
        }

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

        protected void DrawCheckOnly(PaintEventArgs e, LayoutData layout, ColorData colors, Color checkColor, bool disabledColors, ControlAppearanceState state)
            => DrawCheckOnly(11, e.Graphics, layout, colors, checkColor, disabledColors, state);

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
