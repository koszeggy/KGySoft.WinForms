#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialogForm.cs
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

using System.Diagnostics.CodeAnalysis;

#region Used Namespaces

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.Libraries.Language;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.WinApi;

#endregion

#region Used Aliases

using ContentAlignment = System.Drawing.ContentAlignment;
using TaskDialog = KGySoft.WinForms.Components.TaskDialog;
using TaskDialogButton = KGySoft.WinForms.Components.TaskDialogButton;
using TaskDialogControl = KGySoft.WinForms.Components.TaskDialogControl;
using TaskDialogRadioButton = KGySoft.WinForms.Components.TaskDialogRadioButton;

#endregion

#endregion

namespace KGySoft.WinForms.Forms
{
    #region Usings

    using Resources = Properties.Resources;

    #endregion

    // Known incompatibilities (they are intended):
    // - Icon position is the same for every icon type
    // - Different animation on toggling expando button resizing
    // - When colored icon background is used without instruction color, only the icon has the colored background
    // - Auto width is calculated differently (better) when expando button or checkbox has very long text.
    // - When Width is 0 (AutoWidth), width is recalculated only when Buttons collection are changed while no command links are used, or when Options is modified
    //   (in native version, width may be changed even when other texts are appearing or disappearing).
    //   Auto width is recalculated even when Width property is reassigned with 0.
    // - RTL mode can be switched back to LTR
    // - Parent window cannot be activated even if its handle is not defined.
    // Added functionalities:
    // - Custom button/link icons
    // - Security question mode with colored background
    // [- Selectable message/details text mode]
    // [- Formattable message/details text mode]
    // [- Help button, if help is subscribed. Watch dynamic subscriptions while the dialog is shown.]
    /// <summary>
    /// A task dialog implementation with Windows Forms technology
    /// </summary>
    internal sealed partial class TaskDialogForm : BaseForm, ITaskDialog
    {
        #region Nested types

        #region Enumerations

        private enum SystemTextIds
        {
            OK = 800,
            Cancel = 801,
            Retry = 803,
            Yes = 805,
            No = 806,
            Close = 807,
        }

        //[Flags]
        //private enum ResizableElements
        //{
        //    None = 0,
        //    MainInstruction = 1,
        //    Buttons = 1 << 1,
        //    MainTexts = 1 << 2,
        //    CommandLinks = 1 << 3,
        //    RadioButtons = 1 << 4,

        //    All = 0xFF,
        //}

        #endregion

        #region Nested classes

        private class Win32Window : IWin32Window
        {
            #region Properties

            public IntPtr Handle { get; set; }

            #endregion
        }

        private class MainInstructionPanel : Panel
        {
            internal TaskDialogForm Owner { get; set; } = null!;

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                // if base was called, flickering would happen when expanding/collapsing details with themed background
                if (DesignMode || !Owner.isSpecialHeadColors)
                {
                    // in design mode avoiding unpainted effect
                    base.OnPaintBackground(e);
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (!DesignMode && Owner.isSpecialHeadColors)
                {
                    using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Owner.gradientStart, Owner.gradientEnd, LinearGradientMode.Horizontal))
                    {
                        e.Graphics.FillRectangle(brush, ClientRectangle);
                    }

                    return;
                }

                base.OnPaint(e);
            }
        }

        private class Configuration
        {
            // not static because can be changed with system settings
            int baseUnitX;

            internal bool HasMainInstruction { get; set; }
            internal bool HasMessage { get; set; }
            internal bool HasDetails { get; set; }
            internal bool HasMainText { get; set; }
            internal bool HasRadioButtons { get; set; }
            internal bool HasMainIcon { get; set; }
            internal bool HasFooter { get; set; }
            internal bool HasCommandLinks { get; set; }
            internal bool HasVerification { get; set; }
            internal bool HasButtons { get; set; }
            internal bool HasMainControls { get; set; }
            internal bool HasProgressBar { get; set; }
            internal bool IsDetailsVisibleInMain { get; set; }
            internal bool IsDetailsVisibleInFooter { get; set; }
            internal bool IsRightToLeft { get; set; }

            internal int DluToPixelsX(int dialogUnitX)
            {
                if (baseUnitX == 0)
                {
                    baseUnitX = (int)User32.GetDialogBaseUnits() & 0xFFFF;
                }

                return dialogUnitX * baseUnitX / 4;
            }
        }

        #endregion

        #endregion

        #region Fields

        #region Static Fields

        [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local", Justification = "False alarm, GetSystemText is the loader delegate")]
        private static readonly Cache<SystemTextIds, string> systemTextCache = new Cache<SystemTextIds, string>(GetSystemText, 6, EnumComparer<SystemTextIds>.Comparer);
        
        private static readonly TaskDialogStandardIcons[] iconsWithColoredHeader = new[] { TaskDialogStandardIcons.SecuritySuccess, TaskDialogStandardIcons.SecurityWarning, TaskDialogStandardIcons.SecurityError, TaskDialogStandardIcons.SecurityShieldGray, TaskDialogStandardIcons.SecurityShieldBlue, TaskDialogStandardIcons.SecurityQuestion };

        #endregion

        #region Instance Fields

        private TaskDialogStatus dialogState = TaskDialogStatus.Initializing;
        private TaskDialog host = null!;
        private IWin32Window? ownerWindow;
        private int selectedCustomButtonIndex;
        private bool isDetailsExpanded; // Indicates only the state if details is not empty. Does not mean it is visible.
        private bool isDetailsInFooter;
        DateTime dialogStarted;
        private bool isSpecialHeadColors;
        private Color gradientStart;
        private Color gradientEnd;
        private bool useLinks;
        private bool isRadioButtonChecking;
        private bool isForcedClosing;
        private bool isThemed;
        private Font? themedFontLarge;
        private Font? themedFontSmall;
        private bool altF4Pressed;
        private bool isResizing;
        private bool isResettingHeight;
        private bool isResettingVisibilities;
        private bool isResetHeightPending;
        private bool isCheckboxChecking;
        private bool isRecreatingDialog;

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        public TaskDialogForm()
        {
            InitializeComponent();
            pnlMainInstruction.Owner = this;
            HandleCreated += TaskDialogForm_HandleCreated;
            Load += TaskDialogForm_Load;
            FormClosing += TaskDialogForm_FormClosing;
            Closed += TaskDialogForm_Closed;
            KeyDown += TaskDialogForm_KeyDown;
            btnShowHideDetails.ExpandedChanged += btnShowHideDetails_ExpandedChanged;
            pnlMain.SizeChanged += Control_SizeChanged;
            pnlMainControls.SizeChanged += Control_SizeChanged;
            pnlFooter.SizeChanged += Control_SizeChanged;
            cbCheckBox.CheckedChanged += cbCheckBox_CheckedChanged;
            timer.Tick += timer_Tick;
            lblMessage.HyperlinkClicked += TaskDialogForm_HyperlinkClicked;
            lblDetailsMain.HyperlinkClicked += TaskDialogForm_HyperlinkClicked;
            lblDetailsFooter.HyperlinkClicked += TaskDialogForm_HyperlinkClicked;
            lblFooter.HyperlinkClicked += TaskDialogForm_HyperlinkClicked;
            HelpRequested += TaskDialogForm_HelpRequested;
        }

        #endregion

        #region Explicit Disposing

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            // Happens only when TaskDialog.Dispose was called while showing: forcing close and waiting for being closed
            if (dialogState != TaskDialogStatus.Closed)
            {
                isForcedClosing = true;
                dialogState = TaskDialogStatus.Closed;
            }

            if (disposing && (components != null))
            {
                components.Dispose();
            }

            FreeRadioButtons();
            FreeButtons();
            FreeCommandLinks();
            HandleCreated -= TaskDialogForm_HandleCreated;
            Load -= TaskDialogForm_Load;
            FormClosing -= TaskDialogForm_FormClosing;
            Closed -= TaskDialogForm_Closed;
            KeyDown -= TaskDialogForm_KeyDown;
            btnShowHideDetails.ExpandedChanged -= btnShowHideDetails_ExpandedChanged;
            pnlMain.SizeChanged -= Control_SizeChanged;
            pnlMainControls.SizeChanged -= Control_SizeChanged;
            pnlFooter.SizeChanged -= Control_SizeChanged;
            cbCheckBox.CheckedChanged -= cbCheckBox_CheckedChanged;
            timer.Tick -= timer_Tick;
            lblMessage.HyperlinkClicked -= TaskDialogForm_HyperlinkClicked;
            lblDetailsMain.HyperlinkClicked -= TaskDialogForm_HyperlinkClicked;
            lblDetailsFooter.HyperlinkClicked -= TaskDialogForm_HyperlinkClicked;
            lblFooter.HyperlinkClicked -= TaskDialogForm_HyperlinkClicked;
            HelpRequested -= TaskDialogForm_HelpRequested;

            if (disposing)
            {
                if (themedFontLarge != null)
                {
                    themedFontLarge.Dispose();
                    themedFontLarge = null;
                }

                if (themedFontSmall != null)
                {
                    themedFontSmall.Dispose();
                    themedFontSmall = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #endregion

        #region Properties

        private Font ThemedFontLarge
        {
            get
            {
                if (themedFontLarge != null)
                    return themedFontLarge;

                themedFontLarge = new Font("Arial", 11.75f, FontStyle.Regular, GraphicsUnit.Point);
                return themedFontLarge;
            }
        }

        private Font ThemedFontSmall
        {
            get
            {
                if (themedFontSmall != null)
                    return themedFontSmall;

                themedFontSmall = new Font("Arial", 8f, FontStyle.Regular, GraphicsUnit.Point);
                return themedFontSmall;
            }
        }

        #endregion

        #region Methods

        #region Static Methods

        private static string GetSystemText(SystemTextIds id)
        {
            IntPtr handle = Kernel32.GetModuleHandle("user32.dll");
            bool isLoaded = handle != IntPtr.Zero;
            try
            {
                if (!isLoaded)
                    handle = Kernel32.LoadLibrary(Path.Combine(Environment.SystemDirectory, "user32.dll"));

                // cannot load: giving up
                if (handle == IntPtr.Zero)
                    return Enum<SystemTextIds>.ToString(id);

                IntPtr result;
                int length = User32.LoadString(handle, (int)id, out result, 0);

                // cannot access string
                if (length == 0)
                    return Enum<SystemTextIds>.ToString(id);

                return Marshal.PtrToStringAuto(result, length)!;
            }
            finally
            {
                if (handle != IntPtr.Zero && !isLoaded)
                    Kernel32.FreeLibrary(handle);
            }
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ResetHeights(GetConfiguration());
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            // can happen when RightToLeft changes
            if (dialogState == TaskDialogStatus.Showing)
            {
                isRecreatingDialog = true;
            }

            base.OnHandleDestroyed(e);
        }

        #endregion

        #region Private Methods

        private Configuration GetConfiguration()
        {
            Configuration result = new Configuration
            {
                HasMainInstruction = !String.IsNullOrEmpty(host.MainInstruction),
                HasMessage = !String.IsNullOrEmpty(host.Message),
                HasDetails = !String.IsNullOrEmpty(host.DetailsText),
                HasRadioButtons = host.RadioButtons.Count > 0,
                HasMainIcon = host.CustomIcon != null || host.Icon != TaskDialogStandardIcons.None,
                HasFooter = !String.IsNullOrEmpty(host.FooterText),
                HasCommandLinks = ((host.Options & (TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon)) != TaskDialogOptions.None) && host.Buttons.Count > 0,
                HasVerification = !String.IsNullOrEmpty(host.CheckBoxText),
                HasProgressBar = host.ProgressBarStyle != TaskDialogProgressBarStyle.None,
                IsRightToLeft = (host.Options & TaskDialogOptions.RightToLeftLayout) != TaskDialogOptions.None
            };

            result.HasMainText = result.HasMessage || (result.HasDetails && !isDetailsInFooter && isDetailsExpanded);
            result.HasButtons = host.StandardButtons != TaskDialogStandardButtonFlags.None
                || (host.StandardButtons == TaskDialogStandardButtonFlags.None && host.Buttons.Count == 0) // This creates a cancel button
                || (!result.HasCommandLinks && host.Buttons.Count > 0);
            result.HasMainControls = result.HasDetails || result.HasVerification || result.HasButtons;
            result.IsDetailsVisibleInFooter = result.HasDetails && isDetailsInFooter && isDetailsExpanded;
            result.IsDetailsVisibleInMain = result.HasDetails && !isDetailsInFooter && isDetailsExpanded;

            return result;
        }

        /// <summary>
        /// Called before event subscriptions
        /// </summary>
        private void FirstInit()
        {
            selectedCustomButtonIndex = -1;
            cbCheckBox.Checked = host.CheckBoxChecked;
            if (ownerWindow != null && (host.Options & TaskDialogOptions.PositionRelativeToWindow) != TaskDialogOptions.None)
                StartPosition = FormStartPosition.CenterParent;
            else
                StartPosition = FormStartPosition.CenterScreen;

            dialogStarted = DateTime.UtcNow; // for full compatibility it should be in ResetSettings
        }

        /// <summary>
        /// Can be called multiple times during the life of the dialog
        /// </summary>
        private void ResetSettings()
        {
            isDetailsExpanded = (host.Options & TaskDialogOptions.DetailsExpanded) != TaskDialogOptions.None;
            isDetailsInFooter = (host.Options & TaskDialogOptions.ExpandFooterArea) != TaskDialogOptions.None;
            useLinks = (host.Options & TaskDialogOptions.HyperlinksEnabled) != TaskDialogOptions.None;
            Configuration cfg = GetConfiguration();

            // options
            ControlBox = (host.Options & TaskDialogOptions.AllowCancel) != TaskDialogOptions.None || (host.StandardButtons & TaskDialogStandardButtonFlags.Cancel) != TaskDialogStandardButtonFlags.None;
            MinimizeBox = (host.Options & TaskDialogOptions.AllowMinimize) != TaskDialogOptions.None;
            HyperlinkResolveMode resolve = useLinks ? HyperlinkResolveMode.ResolveHrefsOnly : HyperlinkResolveMode.None;
            lblMessage.ResolveHyperlinks = resolve;
            lblDetailsMain.ResolveHyperlinks = resolve;
            lblFooter.ResolveHyperlinks = resolve;
            lblDetailsFooter.ResolveHyperlinks = resolve;
            MinimumSize = new Size(cfg.DluToPixelsX(180), 0);
            RightToLeft = cfg.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;

            // visibilities
            ResetVisibilities(cfg);

            // setting icon
            ResetMainIcon();
            ResetFooterIcon();

            // set theme
            ResetTheme();

            // set texts
            ResetCaption();
            isResizing = true;
            lblMainInstruction.Text = cfg.HasMainInstruction ? host.MainInstruction : String.Empty;
            lblMessage.Text = cfg.HasMessage ? host.Message : String.Empty;
            lblDetailsFooter.Text = cfg.HasDetails && isDetailsInFooter ? host.DetailsText : String.Empty;
            lblDetailsMain.Text = cfg.HasDetails && !isDetailsInFooter ? host.DetailsText : String.Empty;
            ResetShowHideDetailsText();
            cbCheckBox.Text = cfg.HasVerification ? host.CheckBoxText : String.Empty;
            lblFooter.Text = cfg.HasFooter ? host.FooterText : String.Empty;
            isResizing = false;

            // set progress bar
            if (cfg.HasProgressBar)
            {
                pbProgress.IsMarquee = host.ProgressBarStyle == TaskDialogProgressBarStyle.Marquee;
                pbProgress.State = host.ProgressBarState;
                pbProgress.MarqueeAnimationSpeed = host.ProgressBarMarqueeAnimationSpeed;
                pbProgress.Minimum = host.ProgressBarMinimum;
                pbProgress.Maximum = host.ProgressBarMaximum;
                pbProgress.Value = host.ProgressBarValue;
            }

            // set radio buttons
            ResetRadioButtons(cfg);

            // buttons
            ResetButtons(cfg);

            // command links
            ResetCommandLinks(cfg);

            // set default button
            ResetDefaultButton(cfg);

            // Adjusting expando button (this can resize height)
            isResizing = true;
            btnShowHideDetails.IsExpanded = isDetailsExpanded;
            isResizing = false;

            // setting sizes
            ResetWidths(cfg);

            // setting heights (will be called also on OnShow if needed - this is just to prevent immediate resizing when form is appearing)
            ResetHeights(cfg);

        }

        private void ResetChecksWidth(Configuration cfg, bool minSize)
        {
            int maxWidth = cfg.DluToPixelsX(120);
            int desiredWidth = 0;

            if (minSize)
            {
                // setting minimum size so buttons may comsum the rest place: counting desiredWidth from checks
                if (cfg.HasVerification)
                    desiredWidth = cbCheckBox.GetPreferredSize(Size.Empty).Width + cbCheckBox.Margin.Horizontal + pnlChecks.Margin.Horizontal;

                // Expando texts are calculated even if invisible, so checks panel is not rearranged when details text appears
                if (desiredWidth < maxWidth)
                    desiredWidth = Math.Max(desiredWidth, btnShowHideDetails.GetPreferredSize(Size.Empty).Width) + btnShowHideDetails.Margin.Horizontal + pnlChecks.Margin.Horizontal;
            }
            else
            {
                // Setting maximum size consuming what is not needed by the buttons. This may provide enough place for a changing show/hide details or checkbox text
                // Counting desiredWidth from buttons. Precondition: form width and buttons width is calculated now.
                if (!cfg.HasButtons)
                    return;

                desiredWidth = ClientSize.Width - pnlButtons.Width - pnlButtons.Margin.Horizontal;
            }

            pnlMainControls.ColumnStyles[0].Width = Math.Min(maxWidth, desiredWidth);
        }

        private void ResetVisibilities(Configuration cfg)
        {
            isResettingVisibilities = true;
            try
            {
                lblDetailsMain.FadingAnimationOptions = FadingOptions.StandardEffects;
                lblDetailsFooter.FadingAnimationOptions = FadingOptions.StandardEffects;
                pnlMainInstruction.Visible = cfg.HasMainInstruction;
                lblMessage.Visible = cfg.HasMessage;
                lblDetailsMain.Visible = cfg.HasDetails && !isDetailsInFooter && isDetailsExpanded;

                pnlProgressBar.Visible = cfg.HasProgressBar;
                pnlRadioButtons.Visible = host.RadioButtons.Count > 0;
                pnlCommandLinks.Visible = cfg.HasCommandLinks;

                pnlDividerMainBottom.Visible = cfg.HasMainControls || cfg.HasFooter;

                pnlMainControls.Visible = cfg.HasMainControls;
                if (cfg.HasMainControls)
                {
                    btnShowHideDetails.Visible = cfg.HasDetails;
                    cbCheckBox.Visible = cfg.HasVerification;
                    pnlButtons.Visible = cfg.HasButtons;

                    // buttons only
                    if (!cfg.HasDetails && !cfg.HasVerification)
                    {
                        pnlChecks.Visible = false;
                        pnlMainControls.ColumnStyles[0].SizeType = SizeType.Absolute;
                        pnlMainControls.ColumnStyles[0].Width = 0f;
                        pnlMainControls.ColumnStyles[1].SizeType = SizeType.Percent;
                        pnlMainControls.ColumnStyles[1].Width = 100f;
                    }
                    else
                    {
                        pnlChecks.Visible = true;

                        if (cfg.HasButtons)
                        {
                            pnlMainControls.ColumnStyles[0].SizeType = SizeType.Absolute;
                            pnlMainControls.ColumnStyles[0].Width = 200f; // can be refined below
                            pnlMainControls.ColumnStyles[1].SizeType = SizeType.Percent;
                            pnlMainControls.ColumnStyles[1].Width = 100f;
                        }
                        else
                        {
                            pnlMainControls.ColumnStyles[0].SizeType = SizeType.Percent;
                            pnlMainControls.ColumnStyles[0].Width = 100f;
                            pnlMainControls.ColumnStyles[1].SizeType = SizeType.Absolute;
                            pnlMainControls.ColumnStyles[1].Width = 0f;
                        }

                        if (dialogState != TaskDialogStatus.Initializing)
                            ResetChecksWidth(cfg, false);
                    }
                }
                pnlDividerControlsBottom.Visible = cfg.HasMainControls && (cfg.HasFooter || cfg.IsDetailsVisibleInFooter);

                pnlDividerFooterTop.Visible = cfg.HasMainControls && cfg.HasFooter;
                pnlFooter.Visible = cfg.HasFooter;

                // prevent scrollbar flickering
                if (dialogState != TaskDialogStatus.Initializing && !pnlDividerFooterBottom.Visible && cfg.HasFooter && cfg.IsDetailsVisibleInFooter)
                    Height += pnlDividerFooterBottom.Height;
                pnlDividerFooterBottom.Visible = cfg.HasFooter && cfg.IsDetailsVisibleInFooter;

                // prevent scrollbar flickering
                if (dialogState != TaskDialogStatus.Initializing && !pnlDividerDetailsFooterTop.Visible && cfg.IsDetailsVisibleInFooter)
                    Height += pnlDividerDetailsFooterTop.Height;
                pnlDividerDetailsFooterTop.Visible = cfg.IsDetailsVisibleInFooter;

                // prevent scrollbar flickering
                if (dialogState != TaskDialogStatus.Initializing && !lblDetailsFooter.Visible && cfg.IsDetailsVisibleInFooter)
                    Height += lblDetailsFooter.Height;
                lblDetailsFooter.Visible = cfg.IsDetailsVisibleInFooter;

                if (dialogState != TaskDialogStatus.Initializing)
                    FixControlOrder();
            }
            finally
            {
                isResettingVisibilities = false;
            }
        }

        private void ResetCaption()
        {
            if (String.IsNullOrEmpty(host.Caption))
            {
                string?[] args = Environment.GetCommandLineArgs();
                if (!String.IsNullOrEmpty(args[0]))
                    Text = Path.GetFileName(args[0]);
                else
                {
                    ProcessModule? mainModule = Process.GetCurrentProcess().MainModule;
                    Text = mainModule != null ? mainModule.ModuleName : String.Empty;
                }
            }
            else
            {
                Text = host.Caption;
            }
        }

        private void ResetRadioButtons(Configuration cfg)
        {
            if (dialogState != TaskDialogStatus.Initializing)
            {
                FreeRadioButtons();
                while (pnlRadioButtons.HasChildren)
                    pnlRadioButtons.Controls[0].Dispose();
            }

            if (!cfg.HasRadioButtons)
                return;

            int index = 0;
            bool checkedSet = false;
            foreach (TaskDialogRadioButton radioButton in host.RadioButtons)
            {
                AdvancedRadioButton rb = new AdvancedRadioButton
                {
                    AutoSize = false,
                    CheckAlign = ContentAlignment.TopLeft,
                    Name = radioButton.Name,
                    Text = radioButton.Text,
                    Checked = !checkedSet && radioButton.Checked,
                    Enabled = radioButton.Enabled,
                    Dock = DockStyle.Top,
                    Padding = new Padding(5, 0, 5, 5),
                    Tag = radioButton
                };

                rb.CheckedChanged += RadioButton_CheckedChanged;

                if (radioButton.Checked)
                {
                    if (checkedSet)
                        radioButton.CheckedInternal = false;
                    else
                        checkedSet = true;
                }
                BaseToolTip.SetToolTip(rb, radioButton.Description);
                radioButton.Id = index++;
                pnlRadioButtons.Controls.Add(rb);
                rb.BringToFront();
            }
        }

        /// <summary>
        /// Resets standard and custom buttons (not command links).
        /// </summary>
        private void ResetButtons(Configuration cfg)
        {
            if (dialogState != TaskDialogStatus.Initializing)
            {
                FreeButtons();
                while (pnlButtons.HasChildren)
                    pnlButtons.Controls[0].Dispose();
            }

            if (!cfg.HasButtons)
                return;

            // a simple OK button
            if (host.StandardButtons == TaskDialogStandardButtonFlags.None && host.Buttons.Count == 0)
            {
                AddStandardButton(TaskDialogStandardButtonFlags.OK);
            }
            else
            {
                // custom buttons
                if (!cfg.HasCommandLinks && host.Buttons.Count > 0)
                {
                    int index = 0;
                    foreach (TaskDialogButton button in host.Buttons)
                    {
                        AdvancedButton btn = new AdvancedButton
                            {
                                UseVisualStyleBackColor = true,
                                AutoSize = false,
                                Name = button.Name,
                                Text = button.Text,
                                Enabled = button.Enabled,
                                TextImageRelation = TextImageRelation.ImageBeforeText,
                                Tag = button,
                                Margin = new Padding(3, 0, 3, 0),
                                AutoSizeMode = AutoSizeMode.GrowAndShrink, // though AutoSize is false, GetPreferredSize would otherwise union with self Size
                                Size = new Size(70, 23),
                                MinimumSize = new Size(70, 23),
                                IsElevated = button.IsElevated
                            };

                        btn.Click += Button_Click;
                        BaseToolTip.SetToolTip(btn, button.Description);
                        button.Id = index++;
                        if (!button.IsElevated && button.CustomIcon != null)
                            btn.Image = button.CustomIcon.ToAlphaBitmap();

                        pnlButtons.Controls.Add(btn);
                    }
                }

                // standard buttons
                if (host.StandardButtons != TaskDialogStandardButtonFlags.None)
                {
                    foreach (TaskDialogStandardButtonFlags flag in Enum<TaskDialogStandardButtonFlags>.GetFlags(host.StandardButtons, false))
                    {
                        AddStandardButton(flag);
                    }
                }
            }
        }

        private void ResetCommandLinks(Configuration cfg)
        {
            if (dialogState != TaskDialogStatus.Initializing)
            {
                FreeCommandLinks();
                while (pnlCommandLinks.HasChildren)
                    pnlCommandLinks.Controls[0].Dispose();
            }

            if (!cfg.HasCommandLinks)
                return;

            int index = 0;
            foreach (TaskDialogButton button in host.Buttons)
            {
                string text = button.Text ?? String.Empty;
                StringBuilder description = new StringBuilder(button.Description);
                int newLine = text.IndexOf('\n');
                if (newLine >= 0)
                {
                    description.Insert(0, text.Substring(newLine).Trim() + Environment.NewLine);
                    text = text.Substring(0, newLine).Trim();
                }

                CommandLinkButton btn = new CommandLinkButton
                    {
                        Name = button.Name,
                        Text = text,
                        Description = description.ToString(),
                        Enabled = button.Enabled,
                        Tag = button,
                        Dock = DockStyle.Top,
                        IsElevated = button.IsElevated,
                        UseDefaultGlyph = (host.Options & TaskDialogOptions.UseCommandLinks) != TaskDialogOptions.None
                    };

                btn.Click += Button_Click;
                button.Id = index++;
                if (!button.IsElevated && button.CustomIcon != null)
                    btn.Image = button.CustomIcon.ToAlphaBitmap();

                pnlCommandLinks.Controls.Add(btn);
                btn.BringToFront();
            }
        }

        private void ResetDefaultButton(Configuration cfg)
        {
            TaskDialogButton? defaultDialogButton = host.Buttons.FirstOrDefault(b => b.IsDefault);
            Button? defaultButton;
            if (defaultDialogButton != null)
            {
                Control buttonsParent = cfg.HasCommandLinks ? pnlCommandLinks : pnlButtons;
                defaultButton = buttonsParent.Controls.Cast<Button>().First(b => b.Tag == defaultDialogButton);
            }
            else
            {
                // standard buton
                defaultButton = null;
                if (host.DefaultStandardButton != TaskDialogStandardButtons.None)
                {
                    defaultButton = pnlButtons.Controls.Cast<Button>().FirstOrDefault(b => b.DialogResult == (DialogResult)host.DefaultStandardButton
                        || (host.DefaultStandardButton == TaskDialogStandardButtons.Close && b.DialogResult == DialogResult.Abort));
                }

                // neither custom nor standard: first button is the default
                if (defaultButton == null)
                {
                    if (cfg.HasCommandLinks)
                        defaultButton = pnlCommandLinks.Controls.Cast<Button>().LastOrDefault(b => b.Enabled); // due to BringToFronts, last command link is the topmost one;
                    else
                        defaultButton = pnlButtons.Controls.Cast<Button>().FirstOrDefault(b => b.Enabled);
                }
            }

            AcceptButton = defaultButton;
            if (defaultButton != null)
                defaultButton.Select();
        }

        private void ResetHeights(Configuration cfg)
        {
            // after resetting visibilities, ResetHeights always called
            if (isResettingVisibilities)
                return;

            if (isResettingHeight)
            {
                isResetHeightPending = true;
                return;
            }

            Rectangle screen = Screen.FromControl(this).WorkingArea;
            int screenHeight = screen.Height;
            isResettingHeight = true;
            try
            {
                SuspendLayout();
                try
                {
                    // adjusting main instruction and icon height
                    if (cfg.HasMainInstruction)
                    {
                        if (isSpecialHeadColors)
                            pnlMainInstruction.Height =
                                pnlMainIconBackground.Height =
                                Math.Max(lblMainInstruction.Height, pnlMainIconBackground.MinimumSize.Height);
                        else
                            pnlMainInstruction.Height = lblMainInstruction.Height + pnlMainInstruction.Padding.Vertical;
                    }
                    else if (isSpecialHeadColors)
                        pnlMainIconBackground.Height = pnlMainIconBackground.MinimumSize.Height;

                    // Workaround: pnlMainTexts.Height (AutoSize does not work)
                    if (cfg.HasMainText)
                        pnlMainTexts.Height = (cfg.HasMessage ? lblMessage.Height : 0)
                                              + (cfg.HasDetails && cfg.IsDetailsVisibleInMain ? lblDetailsMain.Height : 0)
                                              + pnlMainTexts.Padding.Vertical;
                    else
                        pnlMainTexts.Height = pnlMainTexts.MinimumSize.Height;

                    // Workaround: pnlCommandLinks.Height (AutoSize does not work)
                    if (cfg.HasCommandLinks)
                    {
                        Control lastButton = pnlCommandLinks.Controls[0];
                        // last button is at index 0. due to BringToFronts
                        pnlCommandLinks.Height = lastButton.Top + lastButton.Height + 10;
                    }

                    // pnlRadioButtons.Height
                    if (cfg.HasRadioButtons)// && (affectedElements & ResizableElements.RadioButtons) != ResizableElements.None)
                    {
                        Control lastButton = pnlRadioButtons.Controls[0];
                        // last button is at index 0. due to BringToFronts
                        pnlRadioButtons.Height = lastButton.Top + lastButton.Height + 10;
                    }

                    // pnlMain(Content) height (AutoSize does not work correctly)
                    int desiredHeight;
                    if (cfg.HasCommandLinks)
                        desiredHeight = pnlCommandLinks.Top + pnlCommandLinks.Height;
                    else if (cfg.HasRadioButtons)
                        desiredHeight = pnlRadioButtons.Top + pnlRadioButtons.Height;
                    else if (host.ProgressBarStyle != TaskDialogProgressBarStyle.None)
                        desiredHeight = pnlProgressBar.Top + pnlProgressBar.Height;
                    else if (cfg.HasMainText)
                        desiredHeight = pnlMainTexts.Top + pnlMainTexts.Height;
                    else if (cfg.HasMainInstruction)
                        desiredHeight = pnlMainInstruction.Top + pnlMainInstruction.Height;
                    else
                        desiredHeight = pnlMainTexts.MinimumSize.Height;

                    pnlMain.Height = pnlMainContent.Height = Math.Max(desiredHeight, pnlMainTexts.MinimumSize.Height);

                    // pnlMainControls.Height (AutoSize works after all, but causes for a moment to shrink the whole main window)
                    if (cfg.HasMainControls)
                    {
                        desiredHeight = 0;
                        if (cfg.HasVerification || cfg.HasDetails)
                        {
                            if (cfg.HasDetails)
                                desiredHeight = btnShowHideDetails.Height + btnShowHideDetails.Margin.Vertical;

                            if (cfg.HasVerification)
                                desiredHeight += cbCheckBox.Height + cbCheckBox.Margin.Vertical;

                            desiredHeight += pnlChecks.Margin.Vertical;
                            pnlChecks.Height = desiredHeight;
                        }

                        if (cfg.HasButtons)
                            desiredHeight = Math.Max(desiredHeight, pnlButtons.GetPreferredSize(new Size(pnlButtons.Width, 0)).Height + pnlButtons.Margin.Vertical);

                        pnlMainControls.Height = desiredHeight;
                    }

                    // pnlFooter.Height (AutoSize works after all, but causes for a moment to shrink the whole main window)
                    if (cfg.HasFooter)
                    {
                        pnlFooter.Height = lblFooter.GetPreferredSize(new Size(lblFooter.Width, 0)).Height;
                    }
                }
                finally
                {
                    ResumeLayout();
                }

                // calculate form height
                int heightClientDiff = Height - ClientSize.Height;
                int desiredClientHeight;

                // setting ClientSize: when scrollbar is visible, pnlMain.Top can be negative, which must be compensated
                if (cfg.IsDetailsVisibleInFooter)
                    desiredClientHeight = lblDetailsFooter.Top + lblDetailsFooter.Height - pnlMain.Top;
                else if (cfg.HasFooter)
                    desiredClientHeight = pnlFooter.Top + pnlFooter.Height - pnlMain.Top;
                else if (cfg.HasMainControls)
                    desiredClientHeight = pnlMainControls.Top + pnlMainControls.Height - pnlMain.Top;
                else
                    desiredClientHeight = pnlMain.Height - pnlMain.Top;

                Height = Math.Min(desiredClientHeight + heightClientDiff, screenHeight);
            }
            finally
            {
                isResettingHeight = false;
            }

            // When form is invisible, avoiding recursion, otherwise, window may remain small due to false Top values in invisible state
            if (isResetHeightPending && Visible)
            {
                isResetHeightPending = false;
                ResetHeights(cfg);
            }

            if (Top - screen.Top + Height > screenHeight)
                Top = screenHeight + screen.Top - Height;
        }

        private void ResetWidths(Configuration cfg)
        {
            // setting pnlChecks minimum width (It always has priority regardles of form width. Its maximum size is smaller than minimum form size so it is ok)
            if (cfg.HasVerification || cfg.HasDetails)
            {
                ResetChecksWidth(cfg, true);
            }

            // setting form width
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            int screenWidth = screen.Width;
            if (host.Width > 0)
            {
                int desiredWidth = Math.Max(MinimumSize.Width, cfg.DluToPixelsX(host.Width));
                Width = Math.Min(desiredWidth, screenWidth);
            }
            // auto width
            else
            {
                // regular buttons: up to screen width
                int desiredWidth = MinimumSize.Width;
                if (cfg.HasButtons)
                {
                    // setting button sizes without limits to get desired size
                    pnlButtons.SuspendLayout();
                    try
                    {
                        foreach (Button button in pnlButtons.Controls)
                        {
                            button.Size = button.GetPreferredSize(Size.Empty);
                        }
                    }
                    finally
                    {
                        pnlButtons.ResumeLayout();
                    }

                    int preferredWidth = pnlButtons.GetPreferredSize(Size.Empty).Width + (ClientSize.Width - pnlButtons.Width);
                    if (preferredWidth > desiredWidth)
                        desiredWidth = preferredWidth;
                }

                // lblMessage, lblDetailsMain, command links text: up to 280 DLU
                int maxWidth = cfg.DluToPixelsX(280);
                if (desiredWidth < maxWidth)
                {
                    // message
                    if (cfg.HasMessage)
                    {
                        int preferredWidth = lblMessage.GetPreferredSize(Size.Empty).Width + pnlMainTexts.Padding.Horizontal;
                        if (cfg.HasMainIcon)
                            preferredWidth += pnlMainIcon.Width;

                        if (preferredWidth > desiredWidth)
                            desiredWidth = Math.Min(preferredWidth, maxWidth);
                    }

                    // details in main (regardless visibility)
                    if (cfg.HasDetails && !isDetailsInFooter && desiredWidth < maxWidth)
                    {
                        int preferredWidth = lblDetailsMain.GetPreferredSize(Size.Empty).Width + pnlMainTexts.Padding.Horizontal;
                        if (cfg.HasMainIcon)
                            preferredWidth += pnlMainIcon.Width;

                        if (preferredWidth > desiredWidth)
                            desiredWidth = Math.Min(preferredWidth, maxWidth);
                    }

                    // command link buttons
                    if (cfg.HasCommandLinks && desiredWidth < maxWidth)
                    {
                        foreach (CommandLinkButton commandLinkButton in pnlCommandLinks.Controls)
                        {
                            int preferredWidth = commandLinkButton.GetPreferredSize(Size.Empty).Width + pnlCommandLinks.Padding.Horizontal;
                            if (cfg.HasMainIcon)
                                preferredWidth += pnlMainIcon.Width;

                            if (preferredWidth > desiredWidth)
                                desiredWidth = Math.Min(preferredWidth, maxWidth);

                            if (desiredWidth == maxWidth)
                                break;
                        }
                    }
                }

                int widthClientDiff = Width - ClientSize.Width;
                Width = Math.Min(desiredWidth + widthClientDiff, screenWidth);
            }

            // resetting button sizes along with max size so they will not be wider than text
            if (cfg.HasButtons)
            {
                Size maxButtonSize = new Size(pnlButtons.Width - pnlButtons.Padding.Horizontal, 0);

                pnlButtons.SuspendLayout();
                try
                {
                    foreach (Button button in pnlButtons.Controls)
                    {
                        button.MaximumSize = maxButtonSize;
                        button.Size = button.GetPreferredSize(maxButtonSize);
                    }
                }
                finally
                {
                    pnlButtons.ResumeLayout();
                }
            }

            // reset pnlChecks maximum width
            if (cfg.HasVerification || cfg.HasDetails)
            {
                ResetChecksWidth(cfg, false);
            }

            if (Left - screen.Left + Width > screenWidth)
                Left = screenWidth + screen.Left - Width;
        }

        private void AddStandardButton(TaskDialogStandardButtonFlags standardButton)
        {
            AdvancedButton btn = new AdvancedButton
            {
                UseVisualStyleBackColor = true,
                AutoSize = false,
                Margin = new Padding(3, 0, 3, 0),
                AutoSizeMode = AutoSizeMode.GrowAndShrink, // though AutoSize is false, GetPreferredSize would otherwise union with self Size
                Size = new Size(70, 23),
                MinimumSize = new Size(70, 23),
            };
            if ((host.Options & TaskDialogOptions.TranslateStandardButtons) != TaskDialogOptions.None)
            {
                btn.Text = Language.Translate("&" + Enum<TaskDialogStandardButtonFlags>.ToString(standardButton) + "__TaskDialogForm");
            }
            else
            {
                switch (standardButton)
                {
                    case TaskDialogStandardButtonFlags.OK:
                        btn.Text = systemTextCache[SystemTextIds.OK];
                        break;
                    case TaskDialogStandardButtonFlags.Cancel:
                        btn.Text = systemTextCache[SystemTextIds.Cancel];
                        break;
                    case TaskDialogStandardButtonFlags.Close:
                        btn.Text = systemTextCache[SystemTextIds.Close];
                        break;
                    case TaskDialogStandardButtonFlags.Retry:
                        btn.Text = systemTextCache[SystemTextIds.Retry];
                        break;
                    case TaskDialogStandardButtonFlags.Yes:
                        btn.Text = systemTextCache[SystemTextIds.Yes];
                        break;
                    case TaskDialogStandardButtonFlags.No:
                        btn.Text = systemTextCache[SystemTextIds.No];
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("standardButton");
                }
            }

            switch (standardButton)
            {
                case TaskDialogStandardButtonFlags.OK:
                    btn.DialogResult = DialogResult.OK;
                    break;
                case TaskDialogStandardButtonFlags.Cancel:
                    btn.DialogResult = DialogResult.Cancel;
                    break;
                case TaskDialogStandardButtonFlags.Close:
                    btn.DialogResult = DialogResult.Abort;
                    break;
                case TaskDialogStandardButtonFlags.Retry:
                    btn.DialogResult = DialogResult.Retry;
                    break;
                case TaskDialogStandardButtonFlags.Yes:
                    btn.DialogResult = DialogResult.Yes;
                    break;
                case TaskDialogStandardButtonFlags.No:
                    btn.DialogResult = DialogResult.No;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("standardButton");
            }

            pnlButtons.Controls.Add(btn);
            //if (btn.DialogResult == DialogResult.OK)
            //    AcceptButton = btn;
            if (btn.DialogResult == DialogResult.Cancel)
                CancelButton = btn;
        }

        private void ResetShowHideDetailsText()
        {
            if (String.IsNullOrEmpty(host.ShowDetailsText) && String.IsNullOrEmpty(host.HideDetailsText))
            {
                btnShowHideDetails.TextExpanded = Language.Translate("Hide &details__TaskDialogForm");
                btnShowHideDetails.TextCollapsed = Language.Translate("See &details__TaskDialogForm");
            }
            else
            {
                btnShowHideDetails.TextExpanded = String.IsNullOrEmpty(host.HideDetailsText) ? host.ShowDetailsText : host.HideDetailsText;
                btnShowHideDetails.TextCollapsed = String.IsNullOrEmpty(host.ShowDetailsText) ? host.HideDetailsText : host.ShowDetailsText;
            }
        }

        private void ResetTheme()
        {
            isThemed = Application.RenderWithVisualStyles;

            // colors
            BackColor = isThemed ? Color.FromArgb(240, 240, 240) : SystemColors.Control;
            pnlMain.BackColor = isThemed ? Color.White : SystemColors.Control;
            Color dividerBottom = isThemed ? Color.FromArgb(223, 223, 223) : SystemColors.Control;
            Color dividerTop = isThemed ? Color.White : SystemColors.GrayText;
            pnlDividerMainBottom.BackColor = dividerBottom;
            pnlDividerControlsBottom.BackColor = dividerBottom;
            pnlDividerFooterTop.BackColor = dividerTop;
            pnlDividerFooterBottom.BackColor = dividerBottom;
            pnlDividerDetailsFooterTop.BackColor = dividerTop;
            if (isSpecialHeadColors)
            {
                lblMainInstruction.ForeColor = host.Icon == TaskDialogStandardIcons.SecurityWarning ? Color.Black : Color.White;
                pnlMainIconBackground.BackColor = gradientStart;
                lblMainInstruction.Padding = new Padding(8, 15, 8, 15);
            }
            else
            {
                lblMainInstruction.ForeColor = isThemed ? Color.FromArgb(0, 51, 153) : SystemColors.WindowText;
                pnlMainIconBackground.BackColor = pnlMain.BackColor;
                lblMainInstruction.Padding = new Padding(8, 12, 8, 5);
            }

            // fonts
            if (!String.IsNullOrEmpty(host.MainInstruction))
            {
                lblMainInstruction.Font = isThemed ? ThemedFontLarge : new Font(SystemFonts.DialogFont, FontStyle.Bold);
            }

            // progress bar
            if (!WindowsUtils.IsVistaOrLater)
            {
                pbProgress.Style = AdvancedProgressBarStyle.ThemedShiny;
            }
        }

        private void ResetMainIcon()
        {
            bool hasMainIcon = host.CustomIcon != null || host.Icon != TaskDialogStandardIcons.None;
            pnlMainIcon.Visible = hasMainIcon;
            pnlMain.ColumnStyles[0].Width = hasMainIcon ? 50f : 0f;
            bool requireSpecialHeadColors = !SystemInformation.HighContrast && host.Icon.In(iconsWithColoredHeader);

            if (requireSpecialHeadColors)
            {
                switch (host.Icon)
                {
                    case TaskDialogStandardIcons.SecuritySuccess:
                        gradientStart = Color.FromArgb(21, 118, 21);
                        gradientEnd = Color.FromArgb(57, 150, 63);
                        break;
                    case TaskDialogStandardIcons.SecurityWarning:
                        gradientStart = Color.FromArgb(242, 177, 0);
                        gradientEnd = Color.FromArgb(254, 205, 72);
                        break;
                    case TaskDialogStandardIcons.SecurityError:
                        gradientStart = Color.FromArgb(172, 1, 0);
                        gradientEnd = Color.FromArgb(227, 1, 0);
                        break;
                    case TaskDialogStandardIcons.SecurityShieldBlue:
                    case TaskDialogStandardIcons.SecurityQuestion:
                        gradientStart = Color.FromArgb(4, 80, 130);
                        gradientEnd = Color.FromArgb(28, 120, 133);
                        break;
                    case TaskDialogStandardIcons.SecurityShieldGray:
                        gradientStart = Color.FromArgb(157, 143, 133);
                        gradientEnd = Color.FromArgb(164, 152, 144);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            Icon icon;
            if (!hasMainIcon)
                icon = Resources.TaskDialogIcon;
            else if (host.CustomIcon != null)
                icon = host.CustomIcon;
            else
            {
                // Will not be disposed because Icon property uses the instance
                switch (host.Icon)
                {
                    case TaskDialogStandardIcons.Information:
                        icon = Icons.SystemInformation;
                        break;
                    case TaskDialogStandardIcons.Warning:
                        icon = Icons.SystemWarning;
                        break;
                    case TaskDialogStandardIcons.Error:
                        icon = Icons.SystemError;
                        break;
                    case TaskDialogStandardIcons.Question:
                        icon = Icons.SystemQuestion;
                        break;
                    case TaskDialogStandardIcons.SecuritySuccess:
                        icon = Icons.SecuritySuccess;
                        break;
                    case TaskDialogStandardIcons.SecurityWarning:
                        icon = Icons.SecurityWarning;
                        break;
                    case TaskDialogStandardIcons.SecurityError:
                        icon = Icons.SecurityError;
                        break;
                    case TaskDialogStandardIcons.SecurityShield:
                    case TaskDialogStandardIcons.SecurityShieldBlue:
                    case TaskDialogStandardIcons.SecurityShieldGray:
                        icon = Icons.SecurityShield;
                        break;
                    case TaskDialogStandardIcons.SecurityQuestion:
                        icon = Icons.SecurityQuestion;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            if (pbMainIcon.Image != null)
                pbMainIcon.Image.Dispose();
            pbMainIcon.Image = icon.ExtractNearestBitmap(pbMainIcon.Size, PixelFormat.Format32bppArgb, false);

            // Bug: cannot dispose previous icon because DefaultIcon can be disposed, too. It is internal so I cannot check it.
            //if (Icon != DefaultIcon)
            //    Icon.Dispose();
            Icon = host.FormIcon ?? icon;

            if (dialogState != TaskDialogStatus.Initializing &&
                (isSpecialHeadColors != requireSpecialHeadColors || requireSpecialHeadColors))
            {
                // if changing to special colors, increasing height forward to prevent
                // the flickering if scrollbars. 14 is the padding difference
                if (isSpecialHeadColors != requireSpecialHeadColors && requireSpecialHeadColors)
                {
                    Height += 14;
                }

                isSpecialHeadColors = requireSpecialHeadColors;
                ResetTheme();
                pnlMainInstruction.Invalidate();
                ResetHeights(GetConfiguration());
            }
            else
            {
                isSpecialHeadColors = requireSpecialHeadColors;
            }
        }

        private void ResetFooterIcon()
        {
            bool hasFooterIcon = host.CustomFooterIcon != null || host.FooterIcon != TaskDialogStandardIcons.None;
            pnlFooterIcon.Visible = hasFooterIcon;
            pnlFooter.ColumnStyles[0].Width = hasFooterIcon ? 24f : 0f;

            if (!hasFooterIcon)
                return;

            if (host.CustomFooterIcon != null)
            {
                pbFooterIcon.Image = host.CustomFooterIcon.ToAlphaBitmap();
                return;
            }

            Icon icon;
            switch (host.FooterIcon)
            {
                case TaskDialogStandardIcons.Information:
                    icon = Icons.SystemInformation;
                    break;
                case TaskDialogStandardIcons.Warning:
                    icon = Icons.SystemWarning;
                    break;
                case TaskDialogStandardIcons.Error:
                    icon = Icons.SystemError;
                    break;
                case TaskDialogStandardIcons.Question:
                    icon = Icons.SystemQuestion;
                    break;
                case TaskDialogStandardIcons.SecuritySuccess:
                    icon = Icons.SecuritySuccess;
                    break;
                case TaskDialogStandardIcons.SecurityWarning:
                    icon = Icons.SecurityWarning;
                    break;
                case TaskDialogStandardIcons.SecurityError:
                    icon = Icons.SecurityError;
                    break;
                case TaskDialogStandardIcons.SecurityShield:
                case TaskDialogStandardIcons.SecurityShieldBlue:
                case TaskDialogStandardIcons.SecurityShieldGray:
                    icon = Icons.SecurityShield;
                    break;
                case TaskDialogStandardIcons.SecurityQuestion:
                    icon = Icons.SecurityQuestion;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            using (icon)
            {
                pbFooterIcon.Image = icon.ExtractNearestBitmap(pbFooterIcon.Size, PixelFormat.Format32bppArgb, false);
            }
        }

        private void FreeRadioButtons()
        {
            if (!pnlRadioButtons.HasChildren)
                return;

            foreach (RadioButton radioButton in pnlRadioButtons.Controls)
            {
                radioButton.Tag = null;
                radioButton.CheckedChanged -= RadioButton_CheckedChanged;
            }
        }

        private void FreeButtons()
        {
            if (!pnlButtons.HasChildren)
                return;

            foreach (Button button in pnlButtons.Controls)
            {
                button.Tag = null;
                if (button.DialogResult != DialogResult.None)
                    button.Click -= Button_Click;
            }
        }

        private void FreeCommandLinks()
        {
            if (!pnlCommandLinks.HasChildren)
                return;

            foreach (CommandLinkButton button in pnlCommandLinks.Controls)
            {
                button.Tag = null;
                button.Click -= Button_Click;
            }
        }

        /// <summary>
        /// Updates text of a control (does not handle if it has to be appear now)
        /// </summary>
        private void UpdateText(Control control, string? value, bool affectsVisibility, bool updateDescription)
        {
            if (!updateDescription && control.Text == value)
                return;

            Size origSize = control.Visible ? control.Size : Size.Empty;
            Size preferredSize = origSize;
            bool visibilityChange = affectsVisibility && (String.IsNullOrEmpty(value) || control.Text.Length == 0);
            Configuration? cfg = null;
            SuspendLayout();
            try
            {
                if (visibilityChange)
                {
                    ResetVisibilities(cfg = GetConfiguration());
                }

                // because of suspending, control is not resized here
                if (!updateDescription)
                    control.Text = value;
                else
                    ((CommandLinkButton)control).Description = value;

                // Adjusting form height to prevent scrollbar flickering
                if (control.Visible && (preferredSize = control.GetPreferredSize(new Size(control.Width, 0))).Height > origSize.Height)
                {
                    // possible divider visibility change
                    if (visibilityChange)
                        preferredSize.Height += 2;

                    Height += preferredSize.Height - origSize.Height;
                }
            }
            finally
            {
                // control is actually resized here
                isResizing = true;
                ResumeLayout();
                isResizing = false;
            }

            if (control.AutoSize)
                preferredSize = control.Size;
            if (visibilityChange || origSize != preferredSize)
                ResetHeights(cfg ?? GetConfiguration());

            // workaround: hide scrollbar if gets accidentaly visible
            int screenHeight = Screen.FromControl(this).WorkingArea.Height;
            if (Height < screenHeight)
                AdjustFormScrollbars(false);
        }

        private void UpdateButtonIcon(Control control, TaskDialogButton taskDialogButton)
        {
            Size origSize = control.Size;
            Size preferredSize;
            SuspendLayout();
            try
            {
                // because of suspending, control is not resized here
                if (control is CommandLinkButton commandLinkButton)
                {
                    commandLinkButton.IsElevated = taskDialogButton.IsElevated;
                    if (!taskDialogButton.IsElevated && taskDialogButton.CustomIcon != null)
                        commandLinkButton.Image = taskDialogButton.CustomIcon.ToAlphaBitmap();
                    else if (commandLinkButton.Image != null)
                        commandLinkButton.Image = null;
                }
                else
                {
                    AdvancedButton button = ((AdvancedButton)control);
                    button.IsElevated = taskDialogButton.IsElevated;
                    if (!taskDialogButton.IsElevated && taskDialogButton.CustomIcon != null)
                        button.Image = taskDialogButton.CustomIcon.ToAlphaBitmap();
                    else if (button.Image != null)
                        button.Image = null;
                }

                // Adjusting form height to prevent scrollbar flickering
                if ((preferredSize = control.GetPreferredSize(new Size(control.Width, 0))).Height > origSize.Height)
                {
                    Height += preferredSize.Height - origSize.Height;
                }
            }
            finally
            {
                // control is actually resized here
                isResizing = true;
                ResumeLayout();
                isResizing = false;
            }

            if (control.AutoSize)
                preferredSize = control.Size;
            if (origSize != preferredSize)
                ResetHeights(GetConfiguration());

            // workaround: hide scrollbar if gets accidentaly visible
            int screenHeight = Screen.FromControl(this).WorkingArea.Height;
            if (Height < screenHeight)
                AdjustFormScrollbars(false);
        }

        /// <summary>
        /// Fixing controls order. This might be ruined when controls are made visible lately.
        /// </summary>
        private void FixControlOrder()
        {
            SuspendLayout();
            try
            {
                IOrderedEnumerable<Control> controls = this.Controls.Cast<Control>().OrderBy(c => c.TabIndex);
                foreach (Control control in controls)
                {
                    control.BringToFront();
                }
            }
            finally
            {
                ResumeLayout();
            }
        }

        private Control GetControl(TaskDialogControl taskDialogControl)
        {
            if (taskDialogControl is TaskDialogButton)
            {
                // command link
                if ((host.Options & (TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon)) != 0)
                {
                    return pnlCommandLinks.Controls[host.Buttons.Count - taskDialogControl.Id - 1];
                }

                // custom button - if there is no standard button, direct indexing
                if (host.StandardButtons == TaskDialogStandardButtonFlags.None)
                {
                    return pnlButtons.Controls[taskDialogControl.Id];
                }

                // if there are standard buttons, searching
                foreach (Control control in pnlButtons.Controls)
                {
                    if (control.Tag == taskDialogControl)
                        return control;
                }
            }

            // radio button
            return pnlRadioButtons.Controls[host.RadioButtons.Count - taskDialogControl.Id - 1];
        }

        #endregion

        #region Event Handlers
        // ReSharper disable InconsistentNaming

        private void TaskDialogForm_HandleCreated(object? sender, EventArgs e) => host.Handle = Handle;

        private void TaskDialogForm_Load(object? sender, EventArgs e)
        {
            dialogState = TaskDialogStatus.Showing;
            switch (host.Icon)
            {
                case TaskDialogStandardIcons.Information:
                    SystemSounds.Asterisk.Play();
                    break;
                case TaskDialogStandardIcons.Warning:
                    SystemSounds.Exclamation.Play();
                    break;
                case TaskDialogStandardIcons.Error:
                    SystemSounds.Hand.Play();
                    break;
                case TaskDialogStandardIcons.Question:
                    SystemSounds.Question.Play();
                    break;
            }

            host.OnCreated();
            timer.Enabled = host.IsTickAssigned;
        }

        private void TaskDialogForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyData == (Keys.Alt | Keys.F4))
                altF4Pressed = true;
            if (e.KeyData == Keys.Escape && ControlBox)
                DialogResult = DialogResult.Cancel;
        }

        private void TaskDialogForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // preventing ALT+F4 if there is no X (Cancel) option
                if (altF4Pressed && !ControlBox)
                {
                    e.Cancel = true;
                    altF4Pressed = false;
                    return;
                }

                CancelEventArgs args = new CancelEventArgs(false);
                host.OnClosing(args);
                e.Cancel = args.Cancel;
                dialogState = args.Cancel ? TaskDialogStatus.Showing : TaskDialogStatus.Closing;
                if (args.Cancel)
                    selectedCustomButtonIndex = -1;
            }
            else if (e.CloseReason == CloseReason.None)
            {
                // preventing closing the dialog when just recreating it (RightToLeft changes)
                e.Cancel = isRecreatingDialog;
                isRecreatingDialog = false;
            }
            else
            {
                isForcedClosing = true;
                dialogState = TaskDialogStatus.Closing;
            }
        }

        private void TaskDialogForm_Closed(object? sender, EventArgs e)
        {
            dialogState = TaskDialogStatus.Closed;
            host.Handle = IntPtr.Zero;

            // closing from dispose or other serious reason: omitting Closed event
            if (!isForcedClosing)
            {
                host.OnClosed();
            }
        }

        private void RadioButton_CheckedChanged(object? sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender!;
            isRadioButtonChecking = true;
            try
            {
                ((TaskDialogRadioButton)rb.Tag!).Checked = rb.Checked;
            }
            finally
            {
                isRadioButtonChecking = false;
            }
        }

        private void Button_Click(object? sender, EventArgs e)
        {
            Button btn = (Button)sender!;
            HandledEventArgs args = new HandledEventArgs(true);
            TaskDialogButton button = (TaskDialogButton)btn.Tag!;
            button.OnClick(args);
            if (!args.Handled)
            {
                selectedCustomButtonIndex = button.Id;
                Close();
            }
        }

        private void btnShowHideDetails_ExpandedChanged(object? sender, EventArgs e)
        {
            isDetailsExpanded = btnShowHideDetails.IsExpanded;
            if (String.IsNullOrEmpty(host.DetailsText) || isResizing)
                return;

            int diff;
            int detailsHeight;
            Rectangle screen = Screen.FromControl(this).WorkingArea;
            int screenHeight = screen.Height;
            bool resetHeightsNeeded = false;

            // details in main text area
            if ((host.Options & TaskDialogOptions.ExpandFooterArea) == TaskDialogOptions.None)
            {
                detailsHeight = lblDetailsMain.GetPreferredSize(new Size(pnlMainTexts.Width - pnlMainTexts.Padding.Horizontal, 0)).Height;

                // counting icon height only when it is non-themed or there is no main instruction
                int iconHeight = 0;
                bool hasMainInstruction = !String.IsNullOrEmpty(host.MainInstruction);
                if ((host.Icon != TaskDialogStandardIcons.None || host.CustomIcon != null)
                    && !(hasMainInstruction && host.Icon.In(iconsWithColoredHeader)))
                {
                    iconHeight = pnlMainIconBackground.MinimumSize.Height;
                }

                int minHeight = Math.Max(pnlMainTexts.MinimumSize.Height, iconHeight - (hasMainInstruction ? lblMainInstruction.Height : 0));
                int mainTextHeight = Math.Max(minHeight,
                    (isDetailsExpanded ? detailsHeight : 0)
                    + (!String.IsNullOrEmpty(host.Message) ? lblMessage.Height : 0)
                    + pnlMainTexts.Padding.Vertical);
                diff = mainTextHeight - Math.Max(pnlMainTexts.Height, minHeight);
                int formHeight = Math.Min(Height + diff, screenHeight);
                resetHeightsNeeded = Height >= screenHeight || Height + diff > screenHeight;

                // when expnding, setting form height first to prevent appearing scrollbars for a moment
                if (isDetailsExpanded && !resetHeightsNeeded)
                    Height = formHeight;

                isResizing = true;
                try
                {
                    pnlMainTexts.Height = mainTextHeight;
                    if (!resetHeightsNeeded)
                        pnlMain.Height = pnlMainContent.Height += diff;
                    lblDetailsMain.FadingAnimationOptions = FadingOptions.StandardEffects;
                    if (isDetailsExpanded && dialogState != TaskDialogStatus.Initializing)
                        lblDetailsMain.FadingAnimationOptions |= FadingOptions.Appearing;
                    lblDetailsMain.Visible = isDetailsExpanded;

                    // when collapsing, setting form height at the end to prevent appearing scrollbars for a moment
                    if (!isDetailsExpanded && !resetHeightsNeeded)
                        Height = formHeight;
                }
                finally
                {
                    isResizing = false;
                }
            }
            // details in footer
            else
            {
                SuspendLayout();
                try
                {
                    FixControlOrder();
                    detailsHeight = lblDetailsFooter.GetPreferredSize(new Size(ClientSize.Width, 0)).Height;
                    diff = detailsHeight + pnlDividerDetailsFooterTop.Height;
                    bool hasFooter = !String.IsNullOrEmpty(host.FooterText);
                    diff += hasFooter ? pnlDividerFooterBottom.Height : pnlDividerControlsBottom.Height;
                    resetHeightsNeeded = Height >= screenHeight || Height + diff > screenHeight;

                    // when expanding, setting form height first to prevent appearing scrollbars for a moment
                    isResizing = true;
                    try
                    {
                        if (isDetailsExpanded && !resetHeightsNeeded)
                            Height = Math.Min(Height + diff, screenHeight);

                        pnlDividerDetailsFooterTop.Visible = isDetailsExpanded;
                        if (hasFooter)
                            pnlDividerFooterBottom.Visible = isDetailsExpanded;
                        else
                            pnlDividerControlsBottom.Visible = isDetailsExpanded;

                        lblDetailsFooter.FadingAnimationOptions = FadingOptions.StandardEffects;
                        if (isDetailsExpanded && dialogState != TaskDialogStatus.Initializing)
                            lblDetailsFooter.FadingAnimationOptions |= FadingOptions.Appearing;
                        lblDetailsFooter.Visible = isDetailsExpanded;

                        // when collapsing, setting form height at the end to prevent appearing scrollbars for a moment
                        if (!isDetailsExpanded && !resetHeightsNeeded)
                            Height = Math.Min(Height - diff, screenHeight);
                    }
                    finally
                    {
                        isResizing = false;
                    }
                }
                finally
                {
                    ResumeLayout(!resetHeightsNeeded);
                }
            }

            // resetting heights are needed when form scrollbar is visible or will appear/disappear
            Configuration cfg = GetConfiguration();
            if (resetHeightsNeeded)
                ResetHeights(cfg);
            else if (Top - screen.Top + Height > screenHeight)
                Top = screenHeight + screen.Top - Height;

            // workaround: hide scrollbar if gets accidentaly visible
            if (Height < screenHeight)
                AdjustFormScrollbars(false);

            // turning off appearance animation if details are not physically visible
            if (cfg.IsDetailsVisibleInMain)
            {
                int top = lblDetailsMain.Top + pnlMainTexts.Top + pnlMain.Top;
                if (top + lblDetailsMain.Height < 0 || top >= ClientSize.Height)
                    lblDetailsMain.FadingAnimationOptions = FadingOptions.StandardEffects;
            }
            else if (cfg.IsDetailsVisibleInFooter)
            {
                if (lblDetailsFooter.Top + lblDetailsFooter.Height < 0 || lblDetailsFooter.Top >= ClientSize.Height)
                    lblDetailsFooter.FadingAnimationOptions = FadingOptions.StandardEffects;
            }

            // invoking host change
            host.OnDetailsVisibleChanged(new TaskDialogDetailsVisibleChangedEventArgs(btnShowHideDetails.IsExpanded));
        }

        private void Control_SizeChanged(object? sender, EventArgs e)
        {
            // watching this event to recalculate sizes if scrollbar of the form appears/disappears
            if (!Visible || isResizing)
            {
                return;
            }

            ResetHeights(GetConfiguration());
        }

        private void cbCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (dialogState == TaskDialogStatus.Initializing || isCheckboxChecking)
                return;

            host.OnCheckBoxCheckedChanged(cbCheckBox.Checked);
        }

        private void timer_Tick(object? sender, EventArgs e)
        {
            // called only when host.Tick is subscribed so it is not a waste to create event args here
            TaskDialogTickEventArgs args = new TaskDialogTickEventArgs((int)(DateTime.UtcNow - dialogStarted).TotalMilliseconds);
            host.OnTick(args);
            if (args.Reset)
            {
                dialogStarted = DateTime.UtcNow;
            }
        }

        private void TaskDialogForm_HyperlinkClicked(object? sender, HyperlinkClickedEventArgs e) => host.OnHyperlinkClicked(e);

        private void TaskDialogForm_HelpRequested(object? sender, HelpEventArgs hlpevent) => host.OnHelpRequested();

        // ReSharper restore InconsistentNaming
        #endregion

        #endregion

        #endregion

        #region ITaskDialog Members

        TaskDialogStatus ITaskDialog.ShowState => dialogState;

        void ITaskDialog.Close(TaskDialogResult result)
        {
            switch (result)
            {
                case TaskDialogResult.Close:
                    DialogResult = DialogResult.Abort;
                    break;
                case TaskDialogResult.Custom:
                    DialogResult = DialogResult.Ignore;
                    break;
                default:
                    DialogResult = (DialogResult)result;
                    break;
            }
        }

        TaskDialogResult ITaskDialog.Execute(TaskDialog taskDialog, IntPtr owner, out int selectedButtonIndex, out int selectedRadioButtonIndex, out bool checkBoxChecked)
        {
            host = taskDialog;
            if (owner != IntPtr.Zero)
                ownerWindow = new Win32Window { Handle = owner };

            FirstInit();
            ResetSettings();

            // showing the dialog
            if (ownerWindow == null)
                ShowDialog();
            else
                ShowDialog(ownerWindow);

            // mapping result
            TaskDialogResult result;
            switch (DialogResult)
            {
                case DialogResult.Abort: // Abort is mapped to Close
                    result = TaskDialogResult.Close;
                    break;
                case DialogResult.Ignore: // can happen on forced closing with Custom result
                    result = TaskDialogResult.Custom;
                    break;
                default:
                    result = (TaskDialogResult)DialogResult;
                    break;
            }

            selectedButtonIndex = selectedCustomButtonIndex;
            if (selectedButtonIndex >= 0)
                result = TaskDialogResult.Custom;

            selectedRadioButtonIndex = -1;
            TaskDialogRadioButton? selectedRadioButton = host.RadioButtons.FirstOrDefault(rb => rb.Checked);
            if (selectedRadioButton != null)
                selectedRadioButtonIndex = selectedRadioButton.Id;

            checkBoxChecked = cbCheckBox.Checked;
            return result;
        }

        void ITaskDialog.PropertyChanged(string propName)
        {
            if (dialogState == TaskDialogStatus.Initializing || dialogState == TaskDialogStatus.Closed)
            {
                throw new InvalidOperationException("Changing property in invalid state.");
            }

            Configuration cfg;
            switch (propName)
            {
                case TaskDialog.PropertyMessage:
                    UpdateText(lblMessage, host.Message, true, false);
                    return;

                case TaskDialog.PropertyMainInstruction:
                    UpdateText(lblMainInstruction, host.MainInstruction, true, false);
                    return;

                case TaskDialog.PropertyFooterText:
                    UpdateText(lblFooter, host.FooterText, true, false);
                    return;

                case TaskDialog.PropertyDetailsText:
                    UpdateText(isDetailsInFooter ? lblDetailsFooter : lblDetailsMain, host.DetailsText, true, false);
                    return;

                case TaskDialog.PropertyCaption:
                    ResetCaption();
                    return;

                case TaskDialog.PropertyCheckBoxText:
                    UpdateText(cbCheckBox, host.CheckBoxText, true, false);
                    return;

                case TaskDialog.PropertyShowDetailsText:
                case TaskDialog.PropertyHideDetailsText:
                    if (((btnShowHideDetails.IsExpanded && propName == TaskDialog.PropertyHideDetailsText)
                        || (!btnShowHideDetails.IsExpanded && propName == TaskDialog.PropertyShowDetailsText))
                        && (!String.IsNullOrEmpty(host.ShowDetailsText) && !String.IsNullOrEmpty(host.HideDetailsText)))
                    {
                        UpdateText(btnShowHideDetails, propName == TaskDialog.PropertyShowDetailsText ? host.ShowDetailsText : host.HideDetailsText, false, false);
                    }
                    else
                    {
                        ResetShowHideDetailsText();
                        ResetHeights(GetConfiguration());
                    }
                    return;

                case TaskDialog.PropertyStandardButtons:
                    cfg = GetConfiguration();
                    SuspendLayout();
                    try
                    {
                        // updating visibilities if the buttons panel will just appear/disappear
                        if (!pnlButtons.Visible ||
                            host.StandardButtons == TaskDialogStandardButtonFlags.None && !cfg.HasButtons)
                        {
                            ResetVisibilities(cfg);
                        }

                        ResetButtons(cfg);
                    }
                    finally
                    {
                        ResumeLayout();
                    }

                    ResetDefaultButton(cfg);
                    if (host.Width == 0)
                    {
                        ResetWidths(cfg);
                    }

                    ResetHeights(cfg);
                    return;

                case TaskDialog.PropertyDefaultStandardButton:
                    ResetDefaultButton(GetConfiguration());
                    return;

                case TaskDialog.PropertyWidth:
                    ResetWidths(cfg = GetConfiguration());
                    ResetHeights(cfg);
                    return;

                case TaskDialog.PropertyOptions:
                    ResetSettings();
                    return;

                case TaskDialog.PropertyCheckBoxChecked:
                    isCheckboxChecking = true;
                    try
                    {
                        cbCheckBox.Checked = host.CheckBoxChecked;
                    }
                    finally
                    {
                        isCheckboxChecking = false;
                    }
                    return;

                case TaskDialog.PropertyIcon:
                case TaskDialog.PropertyCustomIcon:
                    ResetMainIcon();
                    return;

                case TaskDialog.PropertyFooterIcon:
                case TaskDialog.PropertyCustomFooterIcon:
                    ResetFooterIcon();
                    return;

                case TaskDialog.PropertyProgressBarStyle:
                    if (host.ProgressBarStyle == TaskDialogProgressBarStyle.None || !pbProgress.Visible)
                    {
                        // turning off progress bar
                        if (host.ProgressBarStyle == TaskDialogProgressBarStyle.None)
                        {
                            pnlProgressBar.Visible = false;
                        }
                        // turning on progress bar
                        else
                        {
                            // preventing flickering scrollbar is possible
                            Height += pnlProgressBar.Height;
                            pnlProgressBar.Visible = true;
                        }

                        ResetHeights(GetConfiguration());
                    }

                    if (host.ProgressBarStyle != TaskDialogProgressBarStyle.None)
                        pbProgress.IsMarquee = host.ProgressBarStyle == TaskDialogProgressBarStyle.Marquee;

                    return;

                case TaskDialog.PropertyProgressBarState:
                    pbProgress.State = host.ProgressBarState;
                    return;

                case TaskDialog.PropertyProgressBarMinimum:
                    pbProgress.Minimum = host.ProgressBarMinimum;
                    return;

                case TaskDialog.PropertyProgressBarMaximum:
                    pbProgress.Maximum = host.ProgressBarMaximum;
                    return;

                case TaskDialog.PropertyProgressBarValue:
                    pbProgress.Value = host.ProgressBarValue;
                    return;

                case TaskDialog.PropertyProgressBarMarqueeAnimationSpeed:
                    pbProgress.MarqueeAnimationSpeed = host.ProgressBarMarqueeAnimationSpeed;
                    return;

                default:
                    throw new NotSupportedException("Not supported property: " + propName);
            }
        }

        void ITaskDialog.ControlPropertyChanged(TaskDialogControl taskDialogControl, string propName)
        {
            if (taskDialogControl is TaskDialogButton button)
            {
                Control control = GetControl(button);
                switch (propName)
                {
                    case TaskDialogButtonBase.PropertyText:
                        UpdateText(control, button.Text, false, false);
                        return;

                    case TaskDialogButtonBase.PropertyDescription:
                        if (control is CommandLinkButton)
                            UpdateText(control, button.Description, false, true);
                        else
                            BaseToolTip.SetToolTip(control, button.Description);
                        return;

                    case TaskDialogButtonBase.PropertyEnabled:
                        control.Enabled = button.Enabled;
                        return;

                    case TaskDialogButton.PropertyIsDefault:
                        ResetDefaultButton(GetConfiguration());
                        return;

                    case TaskDialogButton.PropertyIsElevated:
                    case TaskDialogButton.PropertyCustomIcon:
                        UpdateButtonIcon(control, button);
                        return;

                    default:
                        throw new NotSupportedException("Not supported button property: " + propName);
                }
            }

            if (taskDialogControl is TaskDialogRadioButton radioButton)
            {
                Debug.Assert(pnlRadioButtons.Controls.Count > radioButton.Id);
                Control control = GetControl(radioButton);
                switch (propName)
                {
                    case TaskDialogButtonBase.PropertyText:
                        UpdateText(control, radioButton.Text, false, false);
                        return;

                    case TaskDialogButtonBase.PropertyDescription:
                        BaseToolTip.SetToolTip(control, radioButton.Description);
                        return;

                    case TaskDialogButtonBase.PropertyEnabled:
                        control.Enabled = radioButton.Enabled;
                        return;

                    case TaskDialogRadioButton.PropertyChecked:
                        // if invoked from Checked event handler, exiting, because TaskDialogRadioButton is set there
                        if (isRadioButtonChecking)
                            return;

                        // index 0. is at bottom, so indexing backwards
                        ((RadioButton)pnlRadioButtons.Controls[host.RadioButtons.Count - radioButton.Id - 1]).Checked = radioButton.Checked;
                        return;

                    default:
                        throw new NotSupportedException("Not supported radio button property: " + propName);
                }
            }

            throw new InvalidOperationException("Invalid control type");
        }

        void ITaskDialog.CustomButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index)
        {
            Configuration cfg = GetConfiguration();
            bool buttonsChanged = (host.Options & (TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon)) == 0;
            SuspendLayout();
            try
            {
                // updating visibilities if the buttons panel will just appear/disappear
                if (!pnlCommandLinks.Visible && cfg.HasCommandLinks || // command link appears
                    pnlCommandLinks.HasChildren && !cfg.HasCommandLinks || // command link disappears
                    !pnlButtons.Visible && cfg.HasButtons || // buttons appear
                    pnlButtons.HasChildren && !cfg.HasButtons) // buttons disappear
                {
                    ResetVisibilities(cfg);
                }

                if (buttonsChanged)
                    ResetButtons(cfg);
                else
                    ResetCommandLinks(cfg);
            }
            finally
            {
                ResumeLayout();
            }

            ResetDefaultButton(cfg);
            if (host.Width == 0 || buttonsChanged)
            {
                ResetWidths(cfg);
            }

            ResetHeights(cfg);
        }

        void ITaskDialog.RadioButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index)
        {
            Configuration cfg = GetConfiguration();
            SuspendLayout();
            try
            {
                // updating visibilities if the radio buttons panel will just appear/disappear
                if (!pnlRadioButtons.Visible && cfg.HasRadioButtons ||
                    pnlRadioButtons.HasChildren && !cfg.HasRadioButtons)
                {
                    ResetVisibilities(cfg);
                }

                ResetRadioButtons(cfg);
            }
            finally
            {
                ResumeLayout();
            }

            ResetHeights(cfg);
        }

        void ITaskDialog.TimerChanged(bool enabled)
        {
            timer.Enabled = enabled;
        }

        #endregion
    }
}
