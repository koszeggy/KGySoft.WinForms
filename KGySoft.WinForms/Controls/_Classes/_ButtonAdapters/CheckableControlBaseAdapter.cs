#region Used namespaces

using System.Drawing;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Controls
{
    internal abstract class CheckableControlBaseAdapter: ButtonBaseAdapter
    {
        #region Fields

        private ButtonBaseAdapter buttonAdapter;

        #endregion

        #region Properties

        protected bool IsButton
        {
            get
            {
                CheckBox checkBox = ButtonInstance as CheckBox;
                if (checkBox != null)
                {
                    return checkBox.Appearance == Appearance.Button;
                }

                RadioButton radioButton = ButtonInstance as RadioButton;
                if (radioButton != null)
                {
                    return radioButton.Appearance == Appearance.Button;
                }

                return false;
            }
        }

        protected ButtonBaseAdapter ButtonAdapter
        {
            get { return buttonAdapter ?? (buttonAdapter = CreateButtonAdapter()); }
        }

        #endregion

        #region Constructors

        internal CheckableControlBaseAdapter(ButtonBase control)
            : base(control)
        {
        }

        #endregion

        #region Methods

        #region Internal Methods

        internal override LayoutOptions CommonLayout(ControlAppearanceState state)
        {
            LayoutOptions options = base.CommonLayout(state);
            options.growBorderBy1PxWhenDefault = false;
            options.borderSize = 0;
            options.paddingSize = 0;
            options.maxFocus = false;
            options.focusOddEvenFixup = true;
            options.checkSize = 13;
            return options;
        }

        internal override Size GetPreferredSizeCore(Graphics g, Size proposedSize, ControlAppearanceState state)
        {
            if (IsButton)
            {
                return ButtonAdapter.GetPreferredSizeCore(g, proposedSize, state);
            }

            Size preferredSizeCore = Layout(g, state).GetPreferredSizeCore(g, proposedSize);
            return preferredSizeCore;
        }

        #endregion

        #region Protected Methods

        //protected ControlAppearanceState ToPushButtonAppearance(ControlAppearanceState state)
        //{
        //    ControlAppearanceState result = (ControlAppearanceState)state.Clone();
        //    result.SystemPartId = (int)BUTTONPARTS.BP_PUSHBUTTON;
        //    PUSHBUTTONSTATES state = PUSHBUTTONSTATES.PBS_NORMAL;
        //    if (result.Hovered)
        //    {
        //        state = PUSHBUTTONSTATES.PBS_HOT;
        //    }
        //    else if (!result.Enabled)
        //    {
        //        state = PUSHBUTTONSTATES.PBS_DISABLED;
        //    }
        //    else if (ButtonInstance.Focused || result.IsDefault)
        //    {
        //        state = PUSHBUTTONSTATES.PBS_DEFAULTED;
        //    }

        //    result.SystemStateId = (int)state;
        //    return result;
        //}

        protected ButtonState GetButtonState(ControlAppearanceState state)
        {
            ButtonState result = ButtonState.Normal;
            if (state.CheckState != CheckState.Unchecked)
            {
                result |= ButtonState.Checked;
            }
            if (!state.Enabled)
            {
                result |= ButtonState.Inactive;
            }
            if (state.Pressed)
            {
                result |= ButtonState.Pushed;
            }
            return result;
        }

        protected abstract ButtonBaseAdapter CreateButtonAdapter();

        #endregion

        #endregion
    }
}
