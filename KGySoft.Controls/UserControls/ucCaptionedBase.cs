// If you want to see ucCaptionedBase derived classes in design time, then remove comment in next line:
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using KGySoft.CoreLibraries;
using KGySoft.Libraries;
using KGySoft.Libraries.Language;

namespace KGySoft.Controls
{
    /// <summary>
    /// Base class of user controls that may have captions (on a groupbox or label)
    /// and content can be disabled by a checkbox (if <see cref="Orientation"/> is groupboxed).
    /// </summary>
    public partial class ucCaptionedBase: ucBase
    {

        #region Fields

        private bool chk = true;
        private bool showCheckBox = false;
        private Orientation orientation = Orientation.GroupBoxed;
        private Size gbSize;
        private bool gbSizeChanging = false;

        #endregion

        #region Properties

        /// <summary>
        /// The inner GroupBox.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("The inner GroupBox.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GroupBox GroupBox
        {
            get { return groupBox; }
        }

        /// <summary>
        /// The inner Label.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("The inner Label.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Label Label
        {
            get { return lblCaption; }
        }

        /// <summary>
        /// Gets or sets the caption of the control.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the caption of the control.")]
        [DefaultValue("Caption")]
        public virtual string Caption
        {
            get { return lblCaption.Text; }
            set { RefreshCaption(value); }
        }

        /// <summary>
        /// Gets or sets whether the outer GroupBox has a checkbox. (Only when <see cref="Orientation"/> is GroupBoxed)
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets whether the outer GroupBox has a checkbox. (Only when Orientation is GroupBoxed)")]
        [DefaultValue(false)]
        [RefreshProperties(RefreshProperties.All)]
        public virtual bool ShowCheckBox
        {
            get { return showCheckBox; }
            set
            {
                if (orientation != Orientation.GroupBoxed)
                    CaptionOrientation = Orientation.GroupBoxed;

                showCheckBox = value;
                chkCheckBox.Visible = value;
                if (!showCheckBox)
                {
                    groupBox.Enabled = true;
                    chkCheckBox.Checked = true;
                }
                else
                {
                    chkCheckBox.Checked = chk;
                    groupBox.Enabled = chk;
                }
                RefreshCaption(Caption);
            }
        }

        /// <summary>
        /// Gets or sets the checked state of the checkbox in the outer groupbox. (Only when the <see cref="Orientation"/> is groupboxed)
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the checked state of the checkbox in the outer groupbox. (Only when the Orientation is groupboxed)")]
        [DefaultValue(true)]
        [Bindable(BindableSupport.Yes, BindingDirection.TwoWay)]
        public virtual bool Checked
        {
            get { return (chk); }
            set
            {
                chk = value;
                chkCheckBox.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the ReadOnly state of the inner content.
        /// <remarks>Must override!</remarks>
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the ReadOnly state of the inner content.")]
        [DefaultValue(false)]
        public override bool ReadOnly
        {
            get { return false; }
            set { base.ReadOnly = value; }
        }

        /// <summary>
        /// Gets or sets the text color of the caption and the default text color of inner panel content.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the text color of the caption and the default text color of inner panel content.")]
        [DefaultValue(typeof(Color), "ControlText")]
        public override Color ForeColor
        {
            [DebuggerStepThrough]
            get { return base.ForeColor; }
            [DebuggerStepThrough]
            set
            {
                base.ForeColor = value;
                if (groupBox == null || lblCaption == null)
                    return;
                groupBox.ForeColor = value;
                lblCaption.ForeColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the background color of the caption and the container control.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the background color of the caption and the container control.")]
        [DefaultValue(typeof(Color), "Control")]
        public override Color BackColor
        {
            [DebuggerStepThrough]
            get { return base.BackColor; }
            [DebuggerStepThrough]
            set
            {
                base.BackColor = value;
                if (groupBox == null || lblCaption == null || ContentPanel == null)
                    return;
                groupBox.BackColor = value;
                lblCaption.BackColor = value;
                ContentPanel.BackColor = value;
            }
        }

        /// <summary>
        /// Gets or sets the orientation of the caption.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the orientation of the caption.")]
        [DefaultValue(typeof(ucCaptionedBase.Orientation), "GroupBoxed")]
        [RefreshProperties(RefreshProperties.All)]
        public Orientation CaptionOrientation
        {
            get { return orientation; }
            set { SetOrientation(value); }
        }

        /// <summary>
        /// Gets or sets the alignment of the caption.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the alignment of the caption.")]
        [DefaultValue(typeof(ContentAlignment), "MiddleLeft")]
        public ContentAlignment CaptionAlignment
        {
            get { return lblCaption.TextAlign; }
            set
            {
                lblCaption.TextAlign = value;
                groupBox.RightToLeft = value.In(ContentAlignment.TopRight, ContentAlignment.MiddleRight, ContentAlignment.BottomRight) ? RightToLeft.Yes : RightToLeft.No;
                ContentPanel.RightToLeft = RightToLeft.No;
                RefreshCaption(Caption);
            }
        }

        /// <summary>
        /// Gets or sets the size of the caption.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the size of the caption.")]
        [DefaultValue(0)]
        public int CaptionSize
        {
            get
            {
                switch (orientation)
                {
                    case Orientation.CaptionLeft:
                    case Orientation.CaptionRight:
                        return lblCaption.Width;
                    case Orientation.CaptionTop:
                    case Orientation.CaptionBottom:
                        return lblCaption.Height;
                    default: return 0;
                }
            }
            set
            {
                switch (orientation)
                {
                    case Orientation.CaptionLeft:
                    case Orientation.CaptionRight:
                        lblCaption.Width = value;
                        break;
                    case Orientation.CaptionTop:
                    case Orientation.CaptionBottom:
                        lblCaption.Height = value;
                        break;
                }
            }
        }

        internal virtual Panel ContentPanel
        {
            get { return pnlContent; }
        }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the value of the ckeckbox (that can be enabled by <see cref="ShowCheckBox"/> when <see cref="Orientation"/> is GroupBoxed) has been changed.
        /// </summary>
        [
            Category("ucCaptionedBase"),
            Description("Occurs when the value of the ckeckbox (that can be enabled by ShowCheckBox when Orientation is GroupBoxed) has been changed.")
        ]
        public event EventHandler CheckedChanged
        {
            add { chkCheckBox.CheckedChanged += value; }
            remove { chkCheckBox.CheckedChanged -= value; }
        }

        #endregion

        #region Constructor, methods

        public ucCaptionedBase()
        {
            InitializeComponent();

            chkCheckBox.Checked = chk; // becase default of this.Checked differs from default of CheckBox.Checked
            lblCaption.Visible = false;
            gbSize = groupBox.Size;
            groupBox.SizeChanged += new EventHandler(groupBox_SizeChanged);
            chkCheckBox.CheckedChanged += new EventHandler(chkCheckBox_CheckedChanged);
            // marking groupbox not localizable for prevent storing values with leading spaces in dictionary if checkbox is visible
            Language.MarkLocalizable(false, lblCaption, groupBox, pnlContent);
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            groupBox.SizeChanged -= groupBox_SizeChanged;
            chkCheckBox.CheckedChanged -= chkCheckBox_CheckedChanged;
            base.Dispose(disposing);
        }

        private void RefreshCaption(string value)
        {
            groupBox.Text = (showCheckBox && groupBox.RightToLeft == RightToLeft.No ? "    " : "") + value;
            lblCaption.Text = value;
        }

        private void SetOrientation(Orientation value)
        {
            if (orientation == value)
                return;

            System.Drawing.Size contentSize = ContentPanel.Size;

            switch (value)
            {
                case Orientation.NoCaption:
                    groupBox.Visible = false;
                    pnlTopPadding.Visible = false;
                    showCheckBox = false;
                    chkCheckBox.Visible = false;
                    lblCaption.Visible = false;
                    ContentPanel.Parent = this;
                    Size = contentSize;
                    break;
                case Orientation.GroupBoxed:
                    pnlTopPadding.Visible = true;
                    chkCheckBox.Visible = ShowCheckBox;
                    lblCaption.Visible = false;
                    gbSizeChanging = true;
                    groupBox.Visible = true;
                    ContentPanel.Parent = groupBox;
                    Size = new Size(gbSize.Width, gbSize.Height + pnlTopPadding.Height);
                    gbSizeChanging = false;
                    break;
                default:
                    groupBox.Visible = false;
                    pnlTopPadding.Visible = false;
                    showCheckBox = false;
                    chkCheckBox.Visible = false;
                    lblCaption.Visible = true;
                    ContentPanel.Parent = this;
                    switch (value)
                    {
                        case Orientation.CaptionLeft: lblCaption.Dock = DockStyle.Left; break;
                        case Orientation.CaptionRight: lblCaption.Dock = DockStyle.Right; break;
                        case Orientation.CaptionTop: lblCaption.Dock = DockStyle.Top; break;
                        case Orientation.CaptionBottom: lblCaption.Dock = DockStyle.Bottom; break;
                    }
                    // if last time it was not left/right aligned
                    if (value.In(Orientation.CaptionLeft, Orientation.CaptionRight)
                        && !orientation.In(Orientation.CaptionLeft, Orientation.CaptionRight))
                    {
                        Size = new Size(contentSize.Width + 80, contentSize.Height);
                        lblCaption.Width = 80;
                    }
                    // if last time it was not top/bottom aligned
                    else if (value.In(Orientation.CaptionTop, Orientation.CaptionBottom)
                        && !orientation.In(Orientation.CaptionTop, Orientation.CaptionBottom))
                    {
                        Size = new Size(contentSize.Width, contentSize.Height + 16);
                        lblCaption.Height = 16;
                    }
                    ContentPanel.BringToFront();
                    break;
            }
            orientation = value;
            RefreshCaption(Caption);
        }

        /// <summary>
        /// Translating content in a way which prevents storing Caption text with leading spaces if check box is visible
        /// </summary>
        protected override void TranslateContent(ref bool translationFinished)
        {
            RefreshCaption(Language.Translate(Caption));

            // translating children of pnlContent
            if (pnlContent.HasChildren)
            {
                foreach (Control control in pnlContent.Controls)
                {
                    LanguageWinForms.TranslateControls(control);
                }
            }

            translationFinished = true;
        }

        #endregion

        #region Handled events

        private void chkCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            chk = chkCheckBox.Checked;
            groupBox.Enabled = !showCheckBox || chkCheckBox.Checked;
        }

        void groupBox_SizeChanged(object sender, EventArgs e)
        {
            if (gbSizeChanging || orientation != Orientation.GroupBoxed)
                return;
            gbSize = groupBox.Size;
        }

        #endregion

        #region Nested types

        public enum Orientation
        {
            GroupBoxed,
            CaptionLeft,
            CaptionRight,
            CaptionTop,
            CaptionBottom,
            NoCaption
        }

        #endregion

    }
}
