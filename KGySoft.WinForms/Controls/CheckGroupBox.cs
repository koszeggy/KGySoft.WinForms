#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CheckGroupBox.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.CoreLibraries;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type. - Controls items are never null
#pragma warning disable CS8604 // Possible null reference argument. - Controls items are never null
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Represents a <see cref="GroupBox"/> control with a <see cref="CheckBox"/> that can be checked or unchecked to enable or disable the content of the group box.
    /// </summary>
    public partial class CheckGroupBox : GroupBox, ICustomLocalizable, IToolTipTargetProvider
    {
        #region Fields

        private bool isInitialized;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="CheckBox"/> is checked or unchecked.
        /// </summary>
        internal event EventHandler CheckedChanged
        {
            add => Events.AddHandler(nameof(CheckedChanged), value);
            remove => Events.RemoveHandler(nameof(CheckedChanged), value);
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the text of the <see cref="CheckGroupBox"/>. That is, the text of the <see cref="CheckBox"/> control.
        /// </summary>
        [Localizable(true)]
        [AllowNull]
        public override string Text
        {
            get => checkBox.Text;
            set => checkBox.Text = value;
        }

        /// <summary>
        /// Gets or sets whether the <see cref="CheckBox"/> of the <see cref="CheckGroupBox"/> control is checked.
        /// When checked, the content of the group box is enabled, otherwise it is disabled.
        /// <br/>Default value: <see langword="true"/>.
        /// </summary>
        [Category("CheckGroupBox")]
        [DefaultValue(true)]
        public bool Checked
        {
            get => checkBox.Checked;
            set => checkBox.Checked = value;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets the <see cref="AdvancedCheckBox"/> control, which serves as the checkbox of the <see cref="CheckGroupBox"/> control.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AdvancedCheckBox CheckBox => checkBox;

        #endregion

        #region Protected Properties

        /// <inheritdoc />
        protected override Padding DefaultPadding => new Padding(3, 5, 3, 3);

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckGroupBox"/> class.
        /// </summary>
        [SuppressMessage("ReSharper", "LocalizableElement", Justification = "Whitespace")]
        public CheckGroupBox()
        {
            InitializeComponent();
            Controls.Add(checkBox);
            checkBox.SizeChanged += CheckBox_SizeChanged;
            checkBox.CheckedChanged += CheckBox_CheckedChanged;

            // making sure there is enough space before the CheckBox at every DPI
            base.Text = "   ";
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <inheritdoc />
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);
            if (DesignMode || e.Control.In(checkBox, contentPanel))
                return;

            // Linux/Mono workaround: prevent disabling ErrorProvider's user control when the content is disabled
            if ((!OSHelper.IsWindows || OSHelper.IsMono) && e.Control.GetType().DeclaringType == typeof(ErrorProvider))
                return;

            // When not in design mode, adding custom controls to a panel so we can toggle its Enabled with preserving their original state.
            // Also, translating the control's location so it appears in the same place as in the designer. Doing it only after initialization is complete,
            // because in the designer the child controls' location may be set after adding them to the group box.
            contentPanel.Parent ??= this;
            e.Control.Parent = contentPanel;
            if (isInitialized)
                e.Control.Location = new Point(e.Control.Left - contentPanel.Left, e.Control.Top - contentPanel.Top);
        }

        /// <inheritdoc />
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (isInitialized)
                return;
            isInitialized = true;
            ResetCheckBoxLocation();
            foreach (Control control in contentPanel.Controls)
                control.Location = new Point(control.Left - contentPanel.Left, control.Top - contentPanel.Top);
        }

        /// <summary>
        /// Raises the <see cref="CheckedChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected virtual void OnCheckedChanged(EventArgs e) => (Events[nameof(CheckedChanged)] as EventHandler)?.Invoke(this, e);

        /// <inheritdoc />
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (RightToLeft == RightToLeft.Yes)
                ResetCheckBoxLocation();
        }

        /// <inheritdoc />
        protected override void OnRightToLeftChanged(EventArgs e)
        {
            base.OnRightToLeftChanged(e);
            ResetCheckBoxLocation();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                if (contentPanel.Parent == null)
                    contentPanel.Dispose();
                Events.Dispose();
            }

            checkBox.CheckedChanged -= CheckBox_CheckedChanged;
            checkBox.SizeChanged -= CheckBox_SizeChanged;
            base.Dispose(disposing);
        }

        #endregion

        #region Private Methods

        private void ResetCheckBoxLocation()
        {
            // Skipping if the handle has never been created yet (GetScale uses the Handle).
            // Without this, the focus rectangle may not be rendered when pressing TAB, and not even the ShowFocusCues is called.
            // Can happen if the CheckBox is unchecked and the GroupBox is inside a user control.
            if (!isInitialized)
                return;

            checkBox.Left = RightToLeft == RightToLeft.No
                ? (int)(10 * this.GetScale().X)
                : Width - checkBox.Width - (int)(10 * this.GetScale().X);
        }

        #endregion

        #region Event handlers

        private void CheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            // Toggling the Enabled state of the content. This method preserves the original Enabled state of the controls.
            contentPanel.Enabled = checkBox.Checked;
            OnCheckedChanged(EventArgs.Empty);
        }

        private void CheckBox_SizeChanged(object? sender, EventArgs e) => ResetCheckBoxLocation();

        #endregion

        #region Explicitly Implemented Interface Methods

        bool ICustomLocalizable.ApplyStringResources(LocalizationContext context)
        {
            // Self properties
            LocalizationHelper.LocalizeStringProperties(this, Name, context);

            // children: only contentPanel controls so checkBox is skipped (otherwise, could be overwritten by checkbox.Name)
            foreach (Control child in contentPanel.Controls)
                LocalizationHelper.ApplyStringResources(child, context);

            return true;
        }

        Control IToolTipTargetProvider.GetToolTipTarget() => checkBox;

        #endregion

        #endregion
    }
}
