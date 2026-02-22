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

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class CheckBoxBaseAdapter : CheckableControlBaseAdapter
    {
        #region Constants

        #region Internal Constants

        internal const int FlatCheckSize = 11;

        #endregion

        #region Private Constants

        private const int minCheckSize = 13;
        
        #endregion

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

        protected static void DrawCheckOnly(Graphics g, Rectangle checkBounds, ColorData colors, Color checkColor, bool disabledColors, ControlAppearanceState state)
        {
            if (state.CheckState == CheckState.Unchecked)
                return;

            if (!state.Enabled && disabledColors)
                checkColor = colors.ButtonShadow;
            else if (state.CheckState == CheckState.Indeterminate && disabledColors)
                checkColor = VisualStyleHelper.HighContrast ? colors.Highlight : colors.ButtonShadow;

            checkBounds = Rectangle.Union(checkBounds, new Rectangle(checkBounds.X, checkBounds.Y, minCheckSize, minCheckSize));
            Bitmap checkImage = ControlPaintHelper.GetCheckImage(checkBounds.Size);
            g.DrawImageColorized(checkImage, checkBounds, Color.Black, checkColor);
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
