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
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Base form for OK/Cancel dialogs.
    /// </summary>
    public partial class DialogBaseForm : BaseForm
    {
        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="DialogBaseForm"/>.
        /// </summary>
        public DialogBaseForm()
        {
            InitializeComponent();
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
        }

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Executes the dialog window.
        /// </summary>
        /// <returns>Returns true, when the OK button was pressed, otherwise, false.</returns>
        public virtual bool Execute()
        {
            return ShowDialog() == DialogResult.OK;
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            btnOK.Click -= btnOK_Click;
            btnCancel.Click -= btnCancel_Click;
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Override this method when anything needs to be performed when the OK button is pressed.
        /// Call base method to close the window with positive result.
        /// </summary>
        protected virtual void OKPressed()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Override this method when anything needs to be performed when the Cancel button is pressed.
        /// Call base method to close the window with negative result.
        /// </summary>
        protected virtual void CancelPressed()
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        #endregion

        #region Event handlers

        private void btnOK_Click(object sender, EventArgs e)
        {
            OKPressed();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            CancelPressed();
        }

        #endregion

        #endregion
    }
}
