#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OkCancelButtons.cs
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
using System.Drawing;
using System.Windows.Forms;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Provides a user control with OK, Cancel and optionally Apply buttons.
    /// </summary>
    public sealed partial class OkCancelButtons : BaseUserControl
    {
        #region Fields

        private bool isOkButtonVisible = true;
        private bool isCancelButtonVisible = true;
        private bool isApplyVisible;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets whether the OK button is visible.
        /// Default value: <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        [Category("OkCancelButtons")]
        [Description("Gets or sets whether the OK button is visible.")]
        public bool OKButtonVisible
        {
            get => isOkButtonVisible;
            set
            {
                if (isOkButtonVisible == value)
                    return;
                OKButton.Visible = isOkButtonVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the Cancel button is visible.
        /// Default value: <see langword="true"/>.
        /// </summary>
        [DefaultValue(true)]
        [Category("OkCancelButtons")]
        [Description("Gets or sets whether the Cancel button is visible.")]
        public bool CancelButtonVisible
        {
            get => isCancelButtonVisible;
            set
            {
                if (isCancelButtonVisible == value)
                    return;
                CancelButton.Visible = isCancelButtonVisible = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the Apply button is visible.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        [DefaultValue(false)]
        [Category("OkCancelButtons")]
        [Description("Gets or sets whether the Apply button is visible.")]
        public bool ApplyButtonVisible
        {
            get => isApplyVisible;
            set
            {
                if (isApplyVisible == value)
                    return;
                ApplyButton.Visible = isApplyVisible = value;
            }
        }

        /// <summary>
        /// Gets the OK button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button OKButton => btnOK;

        /// <summary>
        /// Gets the Cancel button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button CancelButton => btnCancel;

        /// <summary>
        /// Gets the Apply button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Button ApplyButton => btnApply;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OkCancelButtons"/> class.
        /// </summary>
        public OkCancelButtons() => InitializeComponent();

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            // Fixing high DPI appearance on Mono
            PointF scale;
            if (OSUtils.IsMono && (scale = this.GetScale()) != ScaleHelper.DefaultScale)
            {
                Height = (int)(35 * scale.Y);
                var referenceButtonSize = new Size(75, 23);
                OKButton.Size = referenceButtonSize.Scale(scale);
                CancelButton.Size = referenceButtonSize.Scale(scale);
                ApplyButton.Size = referenceButtonSize.Scale(scale);
            }

            base.OnLoad(e);
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        #endregion
    }
}