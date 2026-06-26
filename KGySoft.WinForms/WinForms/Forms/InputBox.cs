#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: InputBox.cs
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

namespace KGySoft.WinForms.Forms
{
    internal sealed partial class InputBox : DialogBaseForm
    {
        #region Constants

        // NOTE: LSB flags are in the base BaseForm class, so starting with bit 16
        private const int isResettingHeight = 1 << 16;

        #endregion

        #region Constructors

        public InputBox()
        {
            InitializeComponent();
            if (OSHelper.IsWindowsMono)
                StartPosition = FormStartPosition.CenterParent;
            RightToLeft = LanguageSettings.DisplayLanguage.TextInfo.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            Font = ScaleHelper.MessageBoxFont;
        }

        #endregion

        #region Methods

        #region Static Methods

        internal static bool Show(IWin32Window? owner, string caption, string prompt, ref string value, Point? location = null)
        {
            using var inputBox = new InputBox();
            inputBox.Text = caption;
            inputBox.lblPrompt.Text = prompt;
            inputBox.edtValue.Text = value;
            if (location.HasValue)
            {
                inputBox.StartPosition = FormStartPosition.Manual;
                inputBox.Location = location.Value;
            }

            if (inputBox.ShowDialog(owner) == DialogResult.OK)
            {
                value = inputBox.edtValue.Text;
                return true;
            }

            return false;
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        protected override void OnLoad(EventArgs e)
        {
            if (!IsLoaded)
                CheckHeight();
            base.OnLoad(e);
        }

        protected override void OnDeviceScaleAutoResized(EventArgs e)
        {
            base.OnDeviceScaleAutoResized(e);

            // this triggers an auto resize of the buttons panel, which otherwise would occur later, in WM_DPICHANGED_AFTERPARENT (with per-monitor DPI awareness V2)
            pnlButtons.AutoScale = false;
            pnlButtons.AutoScale = true;
            CheckHeight();
        }

        protected override void Dispose(bool disposing)
        {
            edtValue.KeyPress -= edtValue_KeyPress;

            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void CheckHeight()
        {
            if (flags[isResettingHeight] || !IsHandleCreated)
                return;

            int desiredHeight = lblPrompt.GetPreferredSize(new Size(lblPrompt.Width, 0)).Height;
            if (lblPrompt.Height != desiredHeight)
            {
                flags[isResettingHeight] = true;
                var bounds = Bounds;
                int newHeight = bounds.Height + (desiredHeight - lblPrompt.Height);
                Bounds = new Rectangle(bounds.X, bounds.Y, bounds.Width, newHeight).EnsureScreen(Screen.FromRectangle(bounds), false);
                flags[isResettingHeight] = false;
            }
        }

        #endregion

        #region Event Handlers
#pragma warning disable IDE1006 // Naming Styles

        private void edtValue_KeyPress(object? sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Enter:
                    DialogResult = DialogResult.OK;
                    e.Handled = true;
                    break;
                case (char)Keys.Escape:
                    DialogResult = DialogResult.Cancel;
                    e.Handled = true;
                    break;
            }
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion

        #endregion
    }
}
