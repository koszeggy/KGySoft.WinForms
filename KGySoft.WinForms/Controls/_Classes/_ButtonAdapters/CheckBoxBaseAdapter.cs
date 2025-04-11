#region Used namespaces

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class CheckBoxBaseAdapter: CheckableControlBaseAdapter
    {
        #region Constants

        protected const int FlatCheckSize = 11;

        #endregion

        #region Fields

        [ThreadStatic]
        private static Bitmap checkImageChecked;

        [ThreadStatic]
        private static Color checkImageCheckedBackColor;

        [ThreadStatic]
        private static Bitmap checkImageIndeterminate;

        [ThreadStatic]
        private static Color checkImageIndeterminateBackColor;

        #endregion

        #region Properties

        private CheckBox CheckBoxInstance
        {
            get
            {
                return (CheckBox)ButtonInstance;
            }
        }

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
            {
                checkColor = colors.buttonShadow;
            }
            else if (state.CheckState == CheckState.Indeterminate && disabledColors)
            {
                checkColor = SystemInformation.HighContrast ? colors.highlight : colors.buttonShadow;
            }
            Rectangle checkBounds = layout.checkBounds;
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
            if (layout.options.dotNetOneButtonCompat)
                checkBounds.Y--;
            else
                checkBounds.Y -= 2;

            g.DrawImageColorized(image, checkBounds, checkColor);
        }

        private static Bitmap GetCheckBoxImage(Color checkColor, Rectangle fullSize, ref Color cacheCheckColor, ref Bitmap cacheCheckImage)
        {
            if (((cacheCheckImage == null) || !cacheCheckColor.Equals(checkColor)) || ((cacheCheckImage.Width != fullSize.Width) || (cacheCheckImage.Height != fullSize.Height)))
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
            options.checkAlign = CheckBoxInstance.CheckAlign;
            options.textOffset = false;
            options.shadowedText = !state.Enabled;
            options.layoutRTL = RightToLeft.Yes == ButtonInstance.RightToLeft;
            return options;
        }

        #endregion

        #region Protected Methods

        protected void DrawCheckOnly(PaintEventArgs e, LayoutData layout, ColorData colors, Color checkColor, bool disabledColors, ControlAppearanceState state)
        {
            DrawCheckOnly(11, e.Graphics, layout, colors, checkColor, disabledColors, state);
        }

        #endregion

        #endregion

        #endregion
    }
}
