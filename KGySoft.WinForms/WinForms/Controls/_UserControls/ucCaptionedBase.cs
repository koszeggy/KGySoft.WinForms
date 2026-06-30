#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ucCaptionedBase.cs
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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Libraries.Language;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms
#pragma warning disable CS8604 // Possible null reference argument - inconsistent nullability annotations on different platforms
#endif

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Base class of user controls that may have captions (on a groupbox or label)
    /// and content can be disabled by a checkbox (if <see cref="Orientation"/> is <see cref="Orientation.GroupBoxed"/>).
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "Compatibility, legacy code")]
    [SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Compatibility, legacy code")]
    [Obsolete("This class is derived from the obsolete ucBase, and it is not recommended to use it anymore.")]
    public partial class ucCaptionedBase : ucBase
    {
        #region Enumerations

        /// <summary>
        /// Specifies the layout orientation and caption placement options for an <see cref="ucCaptionedBase"/> control.
        /// </summary>
        /// <remarks>Use this enumeration to control how captions are displayed relative to a group box or
        /// similar container. The values determine whether the caption appears on a specific side, is omitted, or if
        /// the group is rendered with a standard boxed appearance. The exact visual effect may depend on the control or
        /// framework using this enumeration.</remarks>
        public enum Orientation
        {
            /// <summary>
            /// Represents a group boxed orientation (default).
            /// </summary>
            GroupBoxed,

            /// <summary>
            /// Specifies that the caption is aligned to the left.
            /// </summary>
            CaptionLeft,

            /// <summary>
            /// Specifies that the caption is aligned to the right.
            /// </summary>
            CaptionRight,

            /// <summary>
            /// Specifies that the caption is aligned to the top.
            /// </summary>
            CaptionTop,

            /// <summary>
            /// Specifies that the caption is aligned to the bottom.
            /// </summary>
            CaptionBottom,

            /// <summary>
            /// Indicates that no caption is displayed.
            /// </summary>
            NoCaption
        }

        #endregion

        #region Fields

        private bool chk = true;
        private bool showCheckBox;
        private Orientation orientation = Orientation.GroupBoxed;
        private Size gbSize;
        private bool gbSizeChanging;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the value of the ckeckbox (that can be enabled by <see cref="ShowCheckBox"/> when <see cref="Orientation"/> is GroupBoxed) has been changed.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Occurs when the value of the ckeckbox (that can be enabled by ShowCheckBox when Orientation is GroupBoxed) has been changed.")]
        public event EventHandler CheckedChanged
        {
            add => chkCheckBox.CheckedChanged += value;
            remove => chkCheckBox.CheckedChanged -= value;
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// The inner GroupBox.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("The inner GroupBox.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public GroupBox GroupBox => groupBox;

        /// <summary>
        /// The inner Label.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("The inner Label.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public Label Label => lblCaption;

        /// <summary>
        /// Gets or sets the caption of the control.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the caption of the control.")]
        [DefaultValue("Caption")]
        [AllowNull]
        public virtual string Caption
        {
            get => lblCaption.Text;
            set => RefreshCaption(value);
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
            get => showCheckBox;
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
            get => chk;
            set
            {
                chk = value;
                chkCheckBox.Checked = value;
            }
        }

        /// <summary>
        /// Gets or sets the read-only state of the inner content. Should be overridden in a derived control.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the ReadOnly state of the inner content.")]
        [DefaultValue(false)]
        public override bool ReadOnly
        {
            get => false;
            set => base.ReadOnly = value;
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
            get => base.ForeColor;
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
            get => base.BackColor;
            [DebuggerStepThrough]
            set
            {
                base.BackColor = value;
                if (groupBox == null || lblCaption == null)
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
        [DefaultValue(typeof(Orientation), "GroupBoxed")]
        [RefreshProperties(RefreshProperties.All)]
        public Orientation CaptionOrientation
        {
            get => orientation;
            set => SetOrientation(value);
        }

        /// <summary>
        /// Gets or sets the alignment of the caption.
        /// </summary>
        [Category("ucCaptionedBase")]
        [Description("Gets or sets the alignment of the caption.")]
        [DefaultValue(typeof(ContentAlignment), "MiddleLeft")]
        public ContentAlignment CaptionAlignment
        {
            get => lblCaption.TextAlign;
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

        #endregion

        #region Internal Properties

        internal virtual Panel ContentPanel => pnlContent;

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ucCaptionedBase"/> class.
        /// </summary>
        public ucCaptionedBase()
        {
            InitializeComponent();

            chkCheckBox.Checked = chk; // becase default of this.Checked differs from default of CheckBox.Checked
            lblCaption.Visible = false;
            gbSize = groupBox.Size;
            groupBox.SizeChanged += groupBox_SizeChanged;
            chkCheckBox.CheckedChanged += chkCheckBox_CheckedChanged;
            // marking groupbox not localizable for prevent storing values with leading spaces in dictionary if checkbox is visible
            Language.MarkLocalizable(false, lblCaption, groupBox, pnlContent);
        }

        #endregion

        #region Methods

        #region Protected Methods

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            groupBox.SizeChanged -= groupBox_SizeChanged;
            chkCheckBox.CheckedChanged -= chkCheckBox_CheckedChanged;
            base.Dispose(disposing);
        }

        /// <summary>
        /// Translating content in a way which prevents storing Caption text with leading spaces if check box is visible
        /// </summary>
        /// <param name="translationFinished">In this class it always returns <see langword="true"/>.</param>
        protected override void TranslateContent(ref bool translationFinished)
        {
            RefreshCaption(Language.Translate(Caption));

            // translating children of pnlContent
            if (pnlContent.HasChildren)
            {
                foreach (Control control in pnlContent.Controls)
                    LanguageWinForms.TranslateControls(control);
            }

            translationFinished = true;
        }

        #endregion

        #region Private Methods

        private void RefreshCaption(string? value)
        {
            groupBox.Text = (showCheckBox && groupBox.RightToLeft == RightToLeft.No ? "    " : "") + value;
            lblCaption.Text = value;
        }

        private void SetOrientation(Orientation value)
        {
            if (orientation == value)
                return;

            Size contentSize = ContentPanel.Size;

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

        #endregion

        #region Event handlers

        private void chkCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            chk = chkCheckBox.Checked;
            groupBox.Enabled = !showCheckBox || chkCheckBox.Checked;
        }

        void groupBox_SizeChanged(object? sender, EventArgs e)
        {
            if (gbSizeChanging || orientation != Orientation.GroupBoxed)
                return;
            gbSize = groupBox.Size;
        }

        #endregion

        #endregion
    }
}
