#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckableControlBaseAdapter.cs
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

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class CheckableControlBaseAdapter : ButtonBaseAdapter
    {
        #region Constants

        private const int standardCheckSize = 13;

        #endregion

        #region Fields

        private ButtonBaseAdapter? buttonAdapter;

        #endregion

        #region Properties

        protected bool IsButton
        {
            get
            {
                if (ButtonInstance is CheckBox checkBox)
                    return checkBox.Appearance == Appearance.Button;
                if (ButtonInstance is RadioButton radioButton)
                    return radioButton.Appearance == Appearance.Button;
                return false;
            }
        }

        protected ButtonBaseAdapter ButtonAdapter => buttonAdapter ??= CreateButtonAdapter();

        #endregion

        #region Constructors

        internal CheckableControlBaseAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Static Methods

        protected static ButtonState GetButtonState(ControlAppearanceState state)
        {
            ButtonState result = ButtonState.Normal;
            if (state.CheckState != CheckState.Unchecked)
                result |= ButtonState.Checked;
            if (!state.Enabled)
                result |= ButtonState.Inactive;
            if (state.Pressed)
                result |= ButtonState.Pushed;
            return result;
        }

        #endregion

        #region Instance Methods

        #region Internal Methods
        
        internal override LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions options = base.CommonLayout(state);
            options.GrowBorderBy1PxWhenDefault = false;
            options.BorderSize = 0;
            options.PaddingSize = 0;
            options.MaxFocus = false;
            options.FocusOddEvenFixup = true;
            options.CheckSize = standardCheckSize.Scale(options.Scale.X);
            return options;
        }

        internal override Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
        {
            if (IsButton)
                return ButtonAdapter.GetPreferredSizeCore(g, proposedSize, state);

            Size preferredSizeCore = Layout(g, state).GetPreferredSizeCore(g, proposedSize);
            return preferredSizeCore;
        }

        #endregion

        #region Protected Methods

        protected override bool IsHighContrastHighlighted(ControlAppearanceState state) => false;

        protected abstract ButtonBaseAdapter CreateButtonAdapter();

        #endregion

        #endregion

        #endregion
    }
}
