#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DialogBaseForm.cs
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
using System.ComponentModel;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Base form for OK/Cancel(/Apply) dialogs.
    /// </summary>
    public partial class DialogBaseForm : BaseForm
    {
        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets whether the OK button is visible.
        /// Default value: <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        [Category("DialogBaseForm")]
        [Description("Gets or sets whether the OK button is visible.")]
        public bool ShowOKButton
        {
            get => pnlButtons.OKButtonVisible;
            set
            {
                pnlButtons.OKButtonVisible = value;
                AcceptButton = value ? pnlButtons.OKButton : null;
            }
        }

        /// <summary>
        /// Gets or sets whether the Cancel button is visible.
        /// Default value: <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        [Category("DialogBaseForm")]
        [Description("Gets or sets whether the Cancel button is visible.")]
        public bool ShowCancelButton
        {
            get => pnlButtons.CancelButtonVisible;
            set
            {
                pnlButtons.CancelButtonVisible = value;
                base.CancelButton = value ? pnlButtons.CancelButton : null;
            }
        }

        /// <summary>
        /// Gets or sets whether the Apply button is visible.
        /// Default value: <see langword="false"/>.
        /// </summary>
        [DefaultValue(false)]
        [Category("DialogBaseForm")]
        [Description("Gets or sets whether the Apply button is visible.")]
        public bool ShowApplyButton
        {
            get => pnlButtons.ApplyButtonVisible;
            set => pnlButtons.ApplyButtonVisible = value;
        }

        #endregion

        #region Protected Properties

        /// <summary>Gets the OK button.</summary>
        [Browsable(false)]
        protected Button OKButton => pnlButtons.OKButton;

        /// <summary>Gets the Cancel button.</summary>
        [Browsable(false)]
        protected new Button CancelButton => pnlButtons.CancelButton;

        /// <summary>Gets the Apply button.</summary>
        [Browsable(false)]
        protected Button ApplyButton => pnlButtons.ApplyButton;

#pragma warning disable IDE1006 // Naming Styles
        // ReSharper disable InconsistentNaming - Justification: These were protected field names, they are kept for backward compatibility.

        /// <summary>Gets the OK button.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected Button btnOK => OKButton;

        /// <summary>Gets the Cancel button.</summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected Button btnCancel => CancelButton;

        // ReSharper restore InconsistentNaming
#pragma warning restore IDE1006 // Naming Styles

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DialogBaseForm"/>.
        /// </summary>
        public DialogBaseForm()
        {
            InitializeComponent();
            AcceptButton = pnlButtons!.OKButton;
            base.CancelButton = pnlButtons.CancelButton;
            pnlButtons.OKButton.Click += btnOK_Click;
            pnlButtons.CancelButton.Click += btnCancel_Click;
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Executes the dialog window.
        /// </summary>
        /// <returns>Returns true, when the OK button was pressed, otherwise, false.</returns>
        public virtual bool Execute() => ShowDialog() == DialogResult.OK;

        #endregion

        #region Protected Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            pnlButtons.OKButton.Click -= btnOK_Click;
            pnlButtons.CancelButton.Click -= btnCancel_Click;
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        /// <summary>
        /// Override this method when anything needs to be performed when the OK button is pressed.
        /// </summary>
        protected virtual void OKPressed()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Override this method when anything needs to be performed when the Cancel button is pressed.
        /// </summary>
        protected virtual void CancelPressed()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Event handlers
#pragma warning disable IDE1006 // Naming Styles

        private void btnOK_Click(object? sender, EventArgs e) => OKPressed();

        private void btnCancel_Click(object? sender, EventArgs e) => CancelPressed();

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion
    }
}
