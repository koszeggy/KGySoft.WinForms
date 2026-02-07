#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCaptionedContainer.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.WinForms.Controls.Design;

#endregion

namespace KGySoft.WinForms.Controls
{

    /// <summary>
    /// User control with caption (on a groupbox or label) that can be used in design time to drop another controls into it.
    /// </summary>
    [Designer(typeof(ucCaptionedContainerDesigner))]
    [DefaultBindingProperty("ControlValue")]
    //[DesignerSerializer(typeof(ucCaptionedContainerSerializer), typeof(CodeDomSerializer))]
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore. "
        + "If you would use it for the check box, it is recommended to use the CheckGroupBox control instead.")]
    public partial class ucCaptionedContainer: ucCaptionedBase
    {
        #region Fields

        private readonly ContentPanel contentPanel;

        private object? value;
        private bool readOnly;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the <see cref="PanelContent"/> as main control.
        /// </summary>
        protected override Control MainControl => PanelContent;

        /// <summary>
        /// Gets or sets the object value associated with the control.
        /// <see cref="ucCaptionedContainer"/> has no primary inner control so value of this
        /// property is not set automatically but can be used for data binding if you need.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public override object? ControlValue
        {
            get => value;
            set => this.value = value;
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// </summary>
        [Category("ucCaptionedContainer")]
        [Description("Gets or sets the ReadOnly state of the inner content.")]
        [DefaultValue(false)]
        public override bool ReadOnly
        {
            get => readOnly;
            set
            {
                if (readOnly != value)
                {
                    ControlExtensions.SetControlReadonly(PanelContent, value);
                    readOnly = value;
                    base.ReadOnly = value;
                }
            }
        }

        /// <summary>
        /// Gets BackColor regardless of enabled state.
        /// Setter does not work in <see cref="ucCaptionedContainer"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ColorEnabled
        {
            get => BackColor;
            set { }
        }

        /// <summary>
        /// Gets BackColor regardless of enabled state.
        /// Setter does not work in <see cref="ucCaptionedContainer"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ColorDisabled
        {
            get => BackColor;
            set { }
        }

        /// <summary>
        /// This property has no meaning for this instance.
        /// Setter does not work in <see cref="ucCaptionedContainer"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ColorModified
        {
            get => base.ColorModified;
            set { }
        }

        /// <summary>
        /// Gets constantly the gray text color.
        /// Setter does not work in <see cref="ucCaptionedContainer"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ColorControlTextDisabled
        {
            // TODO: AdvancedLabel: IDisabledColorCapable. Then a few of these properties will have meaning.
            get => SystemColors.GrayText;
            set { }
        }

        /// <summary>
        /// Gets ForeColor regardless of enabled state.
        /// Setter does not work in <see cref="ucCaptionedContainer"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color ColorControlTextEnabled
        {
            get => ForeColor;
            set { }
        }

        /// <summary>
        /// Gets the content panel.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ContentPanel PanelContent => contentPanel;

        internal override Panel ContentPanel => contentPanel;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new instance of <see cref="ucCaptionedContainer"/>.
        /// </summary>
        public ucCaptionedContainer()
        {
            InitializeComponent();
            ColorEnabled = SystemColors.Control;

            // Replacing pnlContent to a ContentPanel to prevent saving panel properties when using ucCaptionedContainer
            pnlContent = new ContentPanel(pnlContent);
            contentPanel = (ContentPanel)pnlContent;
        }

        #endregion

        #region Methods

        /// <summary>
        /// This method does nothing in this class.
        /// </summary>
        protected override void ResetColor()
        {
            // suppressing color resetting for the main panel
        }

        #endregion
    }
}

