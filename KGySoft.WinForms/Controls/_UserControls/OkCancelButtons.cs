#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OkCancelButtons.cs
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
    public sealed partial class OkCancelButtons : BaseUserControl, IPerMonitorDpiAware
    {
        #region Fields

        #region Static Fields

        private static readonly Size buttonReferenceSize = new Size(75, 23);
        private static readonly Padding buttonReferenceMargin = new Padding(3);
        private static readonly Padding panelReferencePadding = new Padding(3);

        #endregion

        #region Instance Fields

        private bool isOkButtonVisible = true;
        private bool isCancelButtonVisible = true;
        private bool isApplyVisible;
        private bool autoScale = true;

        #endregion
        
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
        /// Gets or sets whether the buttons and their panel should be automatically scaled depending on the current DPI settings,
        /// regardless of the auto-scaling of the current framework or the <see cref="ContainerControl.AutoScaleMode"/> of the form.
        /// Default value: <see langword="true"/>.
        /// </summary>
        /// <remarks>
        /// <para>Even if this property is set to <see langword="false"/>, the current executing platform still may scale the buttons with more or less success.</para>
        /// <para>This property does not affect the font of the buttons, which are auto-scaled regardless of this property.
        /// To turn off auto-scaling the font, set the <see cref="AdvancedButton.AutoScaleFont"/> property of the <see cref="OKButton"/>, <see cref="CancelButton"/>
        /// and <see cref="ApplyButton"/> properties.</para>
        /// </remarks>
        [DefaultValue(true)]
        [Category("OkCancelButtons")]
        [Description("Gets or sets whether the buttons and their panel should be automatically scaled depending on the current DPI settings, "
            + "regardless of the auto-scaling of the current framework or the AutoScaleMode of the form")]
        public bool AutoScale
        {
            get => autoScale;
            set
            {
                if (autoScale == value)
                    return;
                autoScale = value;
                if (autoScale)
                    ResetSizes();
            }
        }

        /// <summary>
        /// Gets the OK button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AdvancedButton OKButton => btnOK;

        /// <summary>
        /// Gets the Cancel button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AdvancedButton CancelButton => btnCancel;

        /// <summary>
        /// Gets the Apply button.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AdvancedButton ApplyButton => btnApply;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OkCancelButtons"/> class.
        /// </summary>
        public OkCancelButtons()
        {
            InitializeComponent();
            this.RegisterPerMonitorAwarenessNotifications();
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnLoad(EventArgs e)
        {
            if (!IsLoaded && autoScale)
                ResetSizes();
            base.OnLoad(e);
        }

        /// <summary>
        /// Applies the string resources of the <see cref="OkCancelButtons"/> control. By default, if <see cref="BaseUserControl.DynamicStringLocalization"/>
        /// is <see cref="DynamicStringLocalization.Disabled"/>, it applies the English string resources from the <c>KGySoft.WinForms</c> assembly, whose localization
        /// is turned off by default and can be opt-in by setting the <a href="https://docs.kgysoft.net/corelibraries/html/P_KGySoft_LanguageSettings_DynamicResourceManagersSource.htm">LanguageSettings.DynamicResourceManagersSource</a>
        /// property to <a href="https://docs.kgysoft.net/corelibraries/html/T_KGySoft_Resources_ResourceManagerSources.htm">CompiledAndResX</a>.
        /// Otherwise, it applies the dynamic string localization as it is described in the <see cref="BaseUserControl.ApplyStringResources">BaseUserControl.ApplyStringResources</see> method.
        /// </summary>
        protected override void ApplyStringResources()
        {
            btnOK.Text = Res.DialogsOKButtonText;
            btnCancel.Text = Res.DialogsCancelButtonText;
            btnApply.Text = Res.DialogsApplyButtonText;
            base.ApplyStringResources();
        }

        /// <inheritdoc />
        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                // Doing it in WM_DPICHANGED_BEFOREPARENT could cause double scaling on .NET Framework 4.7+ and .NET Core
                case Constants.WM_DPICHANGED_AFTERPARENT when autoScale:
                    base.WndProc(ref m);
                    ResetSizes();
                    break;

                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                components?.Dispose();
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void ResetSizes()
        {
            PointF scale = this.GetScale();
            pnlButtons.SuspendLayout();
            try
            {
                Size minSize = buttonReferenceSize.Scale(scale);
                Padding margin = buttonReferenceMargin.Scale(scale);
                foreach (Control control in pnlButtons.Controls)
                {
                    if (control is not Button button)
                        continue;

                    button.MinimumSize = minSize;
                    button.Size = button.GetPreferredSize(new Size(0, minSize.Height));
                    button.Margin = margin;
                }

                pnlButtons.Padding = panelReferencePadding.Scale(scale);
                Height = minSize.Height + pnlButtons.Padding.Vertical + margin.Vertical;
            }
            finally
            {
                pnlButtons.ResumeLayout();
            }
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        void IPerMonitorDpiAware.ParentFormDpiChanging() { }
        void IPerMonitorDpiAware.ParentFormDpiChanged() => ResetSizes();

        #endregion

        #endregion
    }
}