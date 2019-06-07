using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using KGySoft.Controls.Design;

namespace KGySoft.Controls
{

    /// <summary>
    /// User control with caption (on a groupbox or label) that can be used in design time to drop another controls into it.
    /// </summary>
    [Designer(typeof(ucCaptionedContainerDesigner))]
    [DefaultBindingProperty("ControlValue")]
    //[DesignerSerializer(typeof(ucCaptionedContainerSerializer), typeof(CodeDomSerializer))]
    [ToolboxItem(true)]
    public partial class ucCaptionedContainer: ucCaptionedBase
    {
        #region Fields

        private object value;
        private bool readOnly = false;
        private readonly ContentPanel contentPanel;

        #endregion

        #region Properties

        /// <summary>
        /// Returns the <see cref="PanelContent"/> as main control.
        /// </summary>
        protected override Control MainControl
        {
            get { return PanelContent; }
        }

        /// <summary>
        /// Gets or sets the object value associated with the control.
        /// <see cref="ucCaptionedContainer"/> has no primary inner control so value of this
        /// property is not set automatically but can be used for data binding if you need.
        /// </summary>
        [Bindable(BindableSupport.Default, BindingDirection.TwoWay)]
        public override object ControlValue
        {
            get { return this.value; }
            set { this.value = value; }
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// </summary>
        [Category("ucCaptionedContainer")]
        [Description("Gets or sets the ReadOnly state of the inner content.")]
        [DefaultValue(false)]
        public override bool ReadOnly
        {
            get { return this.readOnly; }
            set
            {
                if (readOnly != value)
                {
                    ControlTools.SetControlReadonly(PanelContent, value);
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
            get { return BackColor; }
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
            get { return BackColor; }
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
            get { return base.ColorModified; }
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
            get { return SystemColors.GrayText; }
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
            get { return ForeColor; }
            set { }
        }

        /// <summary>
        /// Gets the content panel.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ContentPanel PanelContent
        {
            get { return contentPanel; }
        }

        internal override Panel ContentPanel
        {
            get { return contentPanel; }
        }

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
            contentPanel = pnlContent as ContentPanel;
        }

        #endregion

        #region Methods

        protected override void ResetColor()
        {
            // suppressing color resetting for the main panel
        }

        #endregion
    }
}

