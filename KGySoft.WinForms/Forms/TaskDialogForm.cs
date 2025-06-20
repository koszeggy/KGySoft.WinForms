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

#region Used Namespaces

using System;
using System.Collections.Generic;
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
using KGySoft.Drawing.Imaging;
using KGySoft.WinForms.Components;
using KGySoft.WinForms.Controls;
using KGySoft.WinForms.Reflection;
using KGySoft.WinForms.WinApi;

#endregion

#region Used Aliases

using TaskDialog = KGySoft.WinForms.Components.TaskDialog;
using TaskDialogButton = KGySoft.WinForms.Components.TaskDialogButton;
using TaskDialogControl = KGySoft.WinForms.Components.TaskDialogControl;
using TaskDialogRadioButton = KGySoft.WinForms.Components.TaskDialogRadioButton;

#endregion

#endregion

namespace KGySoft.WinForms.Forms
{
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
    // - More detailed Ctrl+C
    // - Description as tooltip for buttons
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

        #endregion

        #region Nested classes

        #region Win32Window class

        private class Win32Window : IWin32Window
        {
            #region Properties

            public IntPtr Handle { get; set; }

            #endregion
        }

        #endregion

        #region MainInstructionPanel class

        private class MainInstructionPanel : Panel
        {
            #region Properties

            internal TaskDialogForm Owner { get; set; } = null!;

            #endregion

            #region Methods

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
                    using LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, Owner.gradientStart, Owner.gradientEnd, LinearGradientMode.Horizontal);
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                    return;
                }

                base.OnPaint(e);
            }

            #endregion
        }

        #endregion

        #region Configuration class

        private class Configuration
        {
            #region Fields

            int baseUnitX;

            #endregion

            #region Properties

            internal bool HasMainInstruction { get; set; }
            internal bool HasMessage { get; set; }
            internal bool HasDetails { get; set; }
            internal bool HasMainText { get; set; }
            internal bool HasRadioButtons { get; set; }
            internal bool HasMainIcon { get; set; }
            internal bool HasFooterIcon { get; set; }
            internal bool HasFooter { get; set; }
            internal bool HasCommandLinks { get; set; }
            internal bool HasVerification { get; set; }
            internal bool HasButtons { get; set; }
            internal bool HasMainControls { get; set; }
            internal bool HasProgressBar { get; set; }
            internal bool IsDetailsVisibleInMain { get; set; }
            internal bool IsDetailsVisibleInFooter { get; set; }
            internal bool IsRightToLeft { get; set; }

            #endregion

            #region Methods

            internal int DluToPixelsX(int dialogUnitX)
            {
                if (baseUnitX == 0)
                    baseUnitX = (int)User32.GetDialogBaseUnits() & 0xFFFF;

                return dialogUnitX * baseUnitX / 4;
            }

            #endregion
        }

        #endregion

        #endregion

        #endregion

        #region Constants

        private const int formReferenceMinWidth = 180; // in DLU
        private const int mainTextReferenceMinHeight = 25;
        private const int messageReferenceMaxWidth = 280; // in DLU
        private const int mainIconBackgroundReferenceHeight = 49;
        private const int mainIconBackgroundReferenceWidth = 50;
        private const int footerIconColumnReferenceWidth = 24;
        private const int checkBoxAndExpandoColumnReferenceWidth = 180;
        private const int progressBarReferenceHeight = 15;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly LockFreeCacheOptions cacheProfile = new()
        {
            InitialCapacity = 6,
            ThresholdCapacity = 6,
            MergeInterval = TimeSpan.FromMilliseconds(100)
        };

        private static readonly IThreadSafeCacheAccessor<SystemTextIds, string> systemTextCache
            = ThreadSafeCacheFactory.Create(GetSystemText, Comparer, cacheProfile);

        private static readonly TaskDialogStandardIcons[] iconsWithColoredHeader = [TaskDialogStandardIcons.SecuritySuccess, TaskDialogStandardIcons.SecurityWarning, TaskDialogStandardIcons.SecurityError, TaskDialogStandardIcons.SecurityShieldGray, TaskDialogStandardIcons.SecurityShieldBlue, TaskDialogStandardIcons.SecurityQuestion];

        private static readonly Color mainInstructionsDefaultThemedColor = Color.FromArgb(0, 51, 153);
        private static readonly Color dividerBottomDefaultThemedColor = Color.FromArgb(223, 223, 223);

        private static readonly Size buttonReferenceSize = new Size(78, 24); // the native version is 23 high, but it is not enough for a 16x16 icon with Standard FlatStyle

        private static readonly Padding buttonsReferenceMargin = new Padding(3, 0, 3, 0);
        private static readonly Padding mainInstructionReferencePadding = new Padding(8, 12, 8, 5);
        private static readonly Padding mainInstructionSpecialColorsReferencePadding = new Padding(8, 15, 8, 15);
        private static readonly Padding labelReferencePadding = new Padding(8, 5, 8, 5);
        private static readonly Padding textsPanelReferencePadding = new Padding(0, 10, 0, 10);
        private static readonly Padding progressBarReferencePadding = new Padding(5);
        private static readonly Padding controlsPanelReferencePadding = new Padding(10, 5, 10, 5);
        private static readonly Padding radioButtonsReferencePadding = new Padding(5, 0, 5, 5);
        private static readonly Padding buttonsPanelReferenceMargin = new Padding(3);
        private static readonly Padding buttonsPanelReferencePadding = new Padding(3);
        private static readonly Padding expandoButtonReferenceMargin = new Padding(3);
        private static readonly Padding checkBoxReferenceMargin = new Padding(8, 3, 3, 3);
        private static readonly Padding footerReferenceMargin = new Padding(3, 0, 3, 0);
        private static readonly Padding footerReferencePadding = new Padding(5, 7, 5, 7);
        private static readonly Padding footerPanelPaddingLtr = new Padding(8, 4, 0, 4);
        private static readonly Padding footerPanelPaddingRtl = new Padding(0, 4, 8, 4);

        private static readonly EnumThreadWndProc enumThreadWindowsCallback = PopulateThreadWindows;

        #endregion

        #region Instance Fields

        private TaskDialogStatus dialogState = TaskDialogStatus.Initializing;
        private TaskDialog host = null!;
        private IWin32Window? ownerWindow;
        private int selectedCustomButtonIndex;
        private bool isDetailsExpanded; // Indicates only the state if details is not empty. Does not mean it is visible.
        private bool isDetailsInFooter;
        private bool isSpecialHeadColors;
        private DateTime dialogStarted;
        private Color gradientStart;
        private Color gradientEnd;
        private Color mainInstructionsColor;
        private bool? cacheMainInstructionsColor;
        private Font? mainInstructionsFont;
        private bool useLinks;
        private bool isRadioButtonChecking;
        private bool isForcedClosing;
        private bool altF4Pressed;
        private bool isResizing;
        private bool isResettingHeight;
        private bool isResettingVisibilities;
        private bool isResetHeightPending;
        private bool isCheckboxChecking;
        private bool isRtlChanging;
        private bool executeNonModal;
        private Point location;

        #endregion

        #endregion

        #region Properties

#if NETFRAMEWORK
        private static IEqualityComparer<SystemTextIds> Comparer => EnumComparer<SystemTextIds>.Comparer;
#else
        private static IEqualityComparer<SystemTextIds>? Comparer => null;
#endif

        #endregion

        #region Constructors

        public TaskDialogForm()
        {
            InitializeComponent();
            pnlMainInstruction.Owner = this;
            btnShowHideDetails.ExpandedChanged += btnShowHideDetails_ExpandedChanged;
            pnlMain.SizeChanged += Control_SizeChanged;
            pnlMainControls.SizeChanged += Control_SizeChanged;
            pnlFooter.SizeChanged += Control_SizeChanged;
            chbCheckBox.CheckedChanged += cbCheckBox_CheckedChanged;
            timer.Tick += timer_Tick;
            lblMessage.HyperlinkClicked += AdvancedLabel_HyperlinkClicked;
            lblDetailsMain.HyperlinkClicked += AdvancedLabel_HyperlinkClicked;
            lblDetailsFooter.HyperlinkClicked += AdvancedLabel_HyperlinkClicked;
            lblFooter.HyperlinkClicked += AdvancedLabel_HyperlinkClicked;
            VisualStyleHelper.VisualStylesChanged += VisualStyleHelper_VisualStylesChanged;
            if (SystemFonts.MessageBoxFont is Font font)
                Font = font;
        }

        #endregion

        #region Properties

        #region Private Properties

        private Font MainInstructionsFont
        {
            get
            {
                if (mainInstructionsFont == null)
                {
                    if (VisualStyleHelper.RenderWithVisualStyles)
                    {
                        if (OSUtils.IsVistaOrLater)
                        {
                            // ISSUE: the following throws an exception because only FontProperty.GlyphFont is accepted by VisualStyleRenderer.GetFont
                            //var renderer = new VisualStyleRenderer(classTaskDialog, Constants.TDLG_MAININSTRUCTIONPANE, 0);
                            //using Graphics g = Graphics.FromHwnd(Handle);
                            //mainInstructionsFont = renderer.GetFont(g, (FontProperty)Constants.TMT_FONT);

                            try
                            {
                                mainInstructionsFont = VisualStyleHelper.GetFont(VisualStyleHelper.TaskDialogTheme, Constants.TDLG_MAININSTRUCTIONPANE)
                                    ?? new Font("Segoe UI", 12, FontStyle.Regular, GraphicsUnit.Point);
                            }
                            catch (Exception e) when (!e.IsCritical())
                            {
                                mainInstructionsFont = new Font("Segoe UI", 12, FontStyle.Regular, GraphicsUnit.Point);
                            }
                        }
                        else
                        {
                            // Windows XP
                            mainInstructionsFont = new Font("Arial", 11.75f, FontStyle.Regular, GraphicsUnit.Point);
                        }
                    }
                    else
                    {
                        // No visual styles
                        mainInstructionsFont = new Font(SystemFonts.DialogFont, FontStyle.Bold);
                    }
                }

                return mainInstructionsFont;
            }
        }

        private Color ThemedMainInstructionsColor
        {
            get
            {
                if (mainInstructionsColor.IsEmpty)
                {
                    var color = OSUtils.IsVistaOrLater
                        ? VisualStyleHelper.GetTextColor(VisualStyleHelper.TaskDialogTheme, Constants.TDLG_MAININSTRUCTIONPANE, 1, mainInstructionsDefaultThemedColor)
                        : mainInstructionsDefaultThemedColor;

                    // ISSUE: When changing from high contrast to normal theme, the VisualStyleRenderer.GetColor(ColorProperty.TextColor) keeps returning
                    // the high contrast SystemColors.ControlText color for a while. Skipping the caching until returning from OnSystemColorsChanged or
                    // invalidating in the first Paint does not help. This is still not optimal, because the appearance can be invalid until the label is repainted.
                    if (cacheMainInstructionsColor != true)
                        return color;
                    mainInstructionsColor = color;
                }

                return mainInstructionsColor;
            }
        }

        #endregion

        #region Explicitly Implemented Interface Properties

        TaskDialogStatus ITaskDialog.ShowState => dialogState;

        #endregion

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

        private static void ResetButtonIcon(AdvancedButton button, Icon? icon, PointF scale)
        {
            if (!button.IsElevated)
                button.Image?.Dispose();
            if (icon == null)
            {
                button.Image = null;
                return;
            }

            using Icon resizedIcon = icon.Resize(IconsHelper.SmallIconReferenceSize.Scale(scale));
            button.Image = resizedIcon.ExtractBitmap(0);
        }

        private static void ResetCommandLinkIcon(CommandLinkButton commandLink, Icon? icon, PointF scale)
        {
            if (!commandLink.IsElevated)
                commandLink.Image?.Dispose();
            if (icon == null)
            {
                commandLink.Image = null;
                return;
            }

            Size preferredSize = IconsHelper.LargeIconReferenceSize.Scale(scale);
            IconInfo[] info = icon.GetIconInfo();
            int? preferredOrLargerMin = info.Where(i => i.Size.Width >= preferredSize.Width).Select(i => (int?)i.Size.Width).Min();

            // the icon has at least one image that is not more than twice as large as the preferred size: extracting the nearest image to the preferred size without resizing
            if (!(preferredOrLargerMin > preferredSize.Width * 2)) // not <= because preferredOrLargerMin can be null if only smaller images are available
            {
                commandLink.Image = icon.ExtractNearestBitmap(preferredSize, PixelFormat.Format32bppArgb);
                return;
            }

            // the icon has only more than twice as large image(s) than the preferred size: resizing (shrinking) it to twice of the preferred size
            using Icon resizedIcon = icon.Resize(new Size(preferredSize.Width * 2, preferredSize.Height * 2));
            commandLink.Image = resizedIcon.ExtractBitmap(0);
        }

        /// <summary>
        /// Gets the visible Win32 windows of the current thread along with their owners.
        /// </summary>
        /// <param name="hWnd">The handle of the window that is currently enumerated.</param>
        /// <param name="lParam">A GCHandle to the result dictionary.</param>
        /// <returns><see langword="true"/> to continue the enumeration.</returns>
        private static bool PopulateThreadWindows(IntPtr hWnd, IntPtr lParam)
        {
            if (!User32.IsWindowVisible(hWnd))
                return true;
            IntPtr owner = User32.GetWindowLong(hWnd, Constants.GWLP_HWNDPARENT);
            ((Dictionary<IntPtr, IntPtr>)GCHandle.FromIntPtr(lParam).Target!).Add(hWnd, owner);
            return true;
        }

        #endregion

        #region Instance Methods

        #region Protected Methods

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // normally host is never null here, but during debugging the handle can be created before Execute is called
            if (host != null!)
                host.Handle = Handle;
        }

        protected override void OnLoad(EventArgs e)
        {
            bool isLoaded = IsLoaded;
            base.OnLoad(e);
            dialogState = TaskDialogStatus.Showing;
            if (isLoaded) // can happen when RightToLeft changes
            {
                if (!isRtlChanging)
                    return;

                isRtlChanging = false;
                Location = location;
                return;
            }

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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.C:
                    CopyToClipboard();
                    break;
                case Keys.Alt | Keys.F4:
                    altF4Pressed = true;
                    break;
                case Keys.Escape when ControlBox:
                    DialogResult = DialogResult.Cancel;
                    break;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        protected override void OnHelpButtonClicked(CancelEventArgs e)
        {
            host.OnHelpRequested();
            e.Cancel = true; // to prevent actually changing to request help mode
        }

        protected override void OnHelpRequested(HelpEventArgs hevent)
        {
            host.OnHelpRequested();
            hevent.Handled = true; // to prevent invoking host.OnHelpRequested() multiple times
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // if the dialog was opened without an owner, simulating the native task dialog behavior that opens in a non-modal way
            if (executeNonModal)
            {
                // Removing the owner and making the form non-modal (Owner is always null here, so not setting that).
                User32.SetWindowLong(Handle, Constants.GWLP_HWNDPARENT, IntPtr.Zero);
                this.SetState(Constants.ControlStates_Modal, false); // without this, the form cannot be closed before closing possible child windows

                // Enabling every top-level window that do not own other windows (not using Application.OpenForms because that ignores native Win32 windows).
                // Now that we cleared the owner of this form, the caller form will be among the windows to enable.
                var threadWindows = new Dictionary<IntPtr, IntPtr>(); // key: self window handle, value: owner window handle (if any)
                GCHandle handle = GCHandle.Alloc(threadWindows);
                User32.EnumThreadWindows(Kernel32.GetCurrentThreadId(), enumThreadWindowsCallback, GCHandle.ToIntPtr(handle));
                handle.Free();
                var owners = new HashSet<IntPtr>(threadWindows.Values.Where(h => h != IntPtr.Zero));
                foreach (IntPtr hWnd in threadWindows.Keys)
                {
                    // enabling the window only if it does not own any other window
                    if (!owners.Contains(hWnd))
                        User32.EnableWindow(hWnd, true);
                }
            }

            // Fixing the height in some cases, especially when opening the dialog on a display with a different DPI than the one of the main display.
            ResetHeights(GetConfiguration());
        }

        protected override void OnDeviceScaleChanged(DeviceScaleChangedEventArgs e)
        {
            base.OnDeviceScaleChanged(e);
            Configuration cfg = GetConfiguration();
            PointF scale = e.NewScale;
            isResizing = true;
            SuspendLayout();
            Point suggestedCenter = e.SuggestedBounds.GetCenter();
            try
            {
                ResetConstraints(scale, false);
                ResetPaddings(scale, false);
            }
            finally
            {
                ResumeLayout(false); // performing layout is not needed here, because ResetWidths and ResetHeights will do it
                isResizing = false;
            }

            if (cfg.HasButtons)
            {
                foreach (AdvancedButton button in pnlButtons.Controls.Cast<AdvancedButton>().Where(b => b.Tag is TaskDialogButton { IsElevated: false, CustomIcon: not null }))
                    ResetButtonIcon(button, ((TaskDialogButton)button.Tag!).CustomIcon!, scale);
            }

            if (cfg.HasCommandLinks)
            {
                foreach (CommandLinkButton commandLink in pnlCommandLinks.Controls.Cast<CommandLinkButton>().Where(b => b.Tag is TaskDialogButton { IsElevated: false, CustomIcon: not null }))
                    ResetCommandLinkIcon(commandLink, ((TaskDialogButton)commandLink.Tag!).CustomIcon!, scale);
            }

            ResetMainIcon(cfg, scale);
            ResetFooterIcon(cfg, scale);
            ResetWidths(cfg, scale, suggestedCenter);
            ResetHeights(cfg, suggestedCenter);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
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
            else if (isRtlChanging)
            {
                // Changing RightToLeft causes the dialog close. We let it happen because the parent may also change,
                // and if we cancel the closing here, then a dialog may turn a non-modal form. Reopening as a dialog is handled in ITaskDialog.Execute
                if (DialogResult != DialogResult.Ignore)
                {
                    isRtlChanging = false;
                    dialogState = TaskDialogStatus.Closing;
                }
                else
                    location = Location;
            }
            else
            {
                isForcedClosing = true;
                dialogState = TaskDialogStatus.Closing;
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            if (isRtlChanging)
                return;

            dialogState = TaskDialogStatus.Closed;
            host.Handle = IntPtr.Zero;

            // closing from dispose or other serious reason: omitting Closed event
            if (!isForcedClosing)
                host.OnClosed();
        }

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

            FreeRadioButtons();
            FreeButtons();
            FreeCommandLinks();
            btnShowHideDetails.ExpandedChanged -= btnShowHideDetails_ExpandedChanged;
            pnlMain.SizeChanged -= Control_SizeChanged;
            pnlMainControls.SizeChanged -= Control_SizeChanged;
            pnlFooter.SizeChanged -= Control_SizeChanged;
            chbCheckBox.CheckedChanged -= cbCheckBox_CheckedChanged;
            timer.Tick -= timer_Tick;
            lblMessage.HyperlinkClicked -= AdvancedLabel_HyperlinkClicked;
            lblDetailsMain.HyperlinkClicked -= AdvancedLabel_HyperlinkClicked;
            lblDetailsFooter.HyperlinkClicked -= AdvancedLabel_HyperlinkClicked;
            lblFooter.HyperlinkClicked -= AdvancedLabel_HyperlinkClicked;
            VisualStyleHelper.VisualStylesChanged -= VisualStyleHelper_VisualStylesChanged;

            if (disposing)
            {
                components?.Dispose();
                mainInstructionsFont?.Dispose();
            }

            base.Dispose(disposing);
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
                HasFooterIcon = host.CustomFooterIcon != null || host.FooterIcon != TaskDialogStandardIcons.None,
                HasFooter = !String.IsNullOrEmpty(host.FooterText),
                HasCommandLinks = ((host.Options & (TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon)) != TaskDialogOptions.None) && host.Buttons.Count > 0,
                HasVerification = !String.IsNullOrEmpty(host.CheckBoxText),
                HasProgressBar = host.ProgressBarStyle != TaskDialogProgressBarStyle.None,
                IsRightToLeft = (host.Options & TaskDialogOptions.RightToLeftLayout) != TaskDialogOptions.None
            };

            result.HasMainText = result.HasMessage || (result.HasDetails && !isDetailsInFooter && isDetailsExpanded);
            result.HasButtons = host.StandardButtons != TaskDialogStandardButtonFlags.None
                || (host.StandardButtons == TaskDialogStandardButtonFlags.None && host.Buttons.Count == 0) // This creates an OK button
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
            chbCheckBox.Checked = host.CheckBoxChecked;
            if (ownerWindow != null && (host.Options & TaskDialogOptions.PositionRelativeToWindow) != TaskDialogOptions.None)
                StartPosition = FormStartPosition.CenterParent;
            else
                StartPosition = FormStartPosition.CenterScreen;

            dialogStarted = DateTime.UtcNow; // for full compatibility it should be in ResetSettings
        }

        /// <summary>
        /// Can be called multiple times during the lifetime of the dialog
        /// </summary>
        private void ResetSettings()
        {
            isDetailsExpanded = (host.Options & TaskDialogOptions.DetailsExpanded) != TaskDialogOptions.None;
            isDetailsInFooter = (host.Options & TaskDialogOptions.ExpandFooterArea) != TaskDialogOptions.None;
            useLinks = (host.Options & TaskDialogOptions.HyperlinksEnabled) != TaskDialogOptions.None;
            Configuration cfg = GetConfiguration();
            PointF scale = this.GetScale();

            // options - Show... properties do not check their change so we do it here to prevent unnecessary style reset or handle recreation
            bool showControlBox = (host.Options & TaskDialogOptions.AllowCancel) != TaskDialogOptions.None
                || executeNonModal && (host.Options & TaskDialogOptions.AllowMinimize) != TaskDialogOptions.None
                || (host.StandardButtons & TaskDialogStandardButtonFlags.Cancel) != TaskDialogStandardButtonFlags.None;
            if (ControlBox != showControlBox)
                ControlBox = showControlBox;
            bool showMinimizeBox = executeNonModal && (host.Options & TaskDialogOptions.AllowMinimize) != TaskDialogOptions.None;
            if (MinimizeBox != showMinimizeBox)
                MinimizeBox = showMinimizeBox;
            bool showHelpButton = host.IsHelpRequestedAssigned;
            if (HelpButton != showHelpButton)
                HelpButton = showHelpButton;
            bool showIcon = executeNonModal || (host.Options & TaskDialogOptions.ForceShowSysMenu) != TaskDialogOptions.None;
            if (ShowIcon != showIcon)
                ShowIcon = showIcon;
            bool showInTaskbar = executeNonModal || (host.Options & TaskDialogOptions.ForceShowInTaskbar) != TaskDialogOptions.None;
            if (ShowInTaskbar != showInTaskbar)
                ShowInTaskbar = showInTaskbar;
            HyperlinkResolveMode resolve = useLinks ? HyperlinkResolveMode.ResolveHrefsOnly : HyperlinkResolveMode.None;
            lblMessage.ResolveHyperlinks = resolve;
            lblDetailsMain.ResolveHyperlinks = resolve;
            lblFooter.ResolveHyperlinks = resolve;
            lblDetailsFooter.ResolveHyperlinks = resolve;
            var rtl = cfg.IsRightToLeft ? RightToLeft.Yes : RightToLeft.No;
            isRtlChanging = dialogState == TaskDialogStatus.Showing && rtl != RightToLeft;
            RightToLeft = rtl;
            pnlFooterIcon.Padding = cfg.IsRightToLeft ? footerPanelPaddingRtl : footerPanelPaddingLtr;

            // Modal forms on Windows: when changing RTL, the DialogResult is set to Cancel in older framework targets, causing the dialog to close.
            // To make it work the same way on all platforms, we set it to Ignore, signaling the check in OnFormClosing.
            if (isRtlChanging && OSUtils.IsWindows && !OSUtils.IsMono)
                DialogResult = DialogResult.Ignore;

            // size constraints
            ResetConstraints(scale, true);

            // visibilities
            ResetVisibilities(cfg);

            // setting icon
            ResetMainIcon(cfg, scale);
            ResetFooterIcon(cfg, scale);

            // paddings and margins (should be after resetting main icon)
            ResetPaddings(scale, true);

            // set theme
            ResetTheme(scale);

            // set texts
            ResetCaption();
            isResizing = true;
            lblMainInstruction.Text = cfg.HasMainInstruction ? host.MainInstruction : String.Empty;
            lblMessage.Text = cfg.HasMessage ? host.Message : String.Empty;
            lblDetailsFooter.Text = cfg.HasDetails && isDetailsInFooter ? host.DetailsText : String.Empty;
            lblDetailsMain.Text = cfg.HasDetails && !isDetailsInFooter ? host.DetailsText : String.Empty;
            ResetShowHideDetailsText();
            chbCheckBox.Text = cfg.HasVerification ? host.CheckBoxText : String.Empty;
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
            ResetRadioButtons(cfg, scale);

            // buttons
            ResetButtons(cfg, scale);

            // command links
            ResetCommandLinks(cfg, scale);

            // set default button
            ResetDefaultButton(cfg);

            // Adjusting expando button (this can resize height)
            isResizing = true;
            btnShowHideDetails.IsExpanded = isDetailsExpanded;
            isResizing = false;

            // setting sizes
            ResetWidths(cfg, scale);

            // setting heights (will be called also on OnShow if needed - this is just to prevent immediate resizing when form is appearing)
            ResetHeights(cfg);
        }

        private void ResetConstraints(PointF scale, bool isFullReset)
        {
            pnlMainTexts.MinimumSize = new Size(0, mainTextReferenceMinHeight).Scale(scale);
            pnlMainIconBackground.MinimumSize = new Size(0, mainIconBackgroundReferenceHeight).Scale(scale);
            
            // exiting if buttons will be completely reset
            if (isFullReset)
                return;

            foreach (AdvancedButton button in pnlButtons.Controls)
            {
                // NOTE: FlowLayoutPanel does not like if the unconstrained dimension of MaximumSize is 0 here.
                // Strange, it works well when setting MaximumSize = (0, y) before adding the button it to the panel.
                button.MinimumSize = buttonReferenceSize.Scale(scale);
                button.MaximumSize = new Size(Int32.MaxValue, buttonReferenceSize.Height.Scale(scale.Y));
            }
        }

        private void ResetPaddings(PointF scale, bool isFullReset)
        {
            // No need to reset the icons paddings, because they are centered anyway. The default padding just ensures that bigger icons have some space around them.
            lblMainInstruction.Padding = isSpecialHeadColors ? mainInstructionSpecialColorsReferencePadding.Scale(scale) : mainInstructionReferencePadding.Scale(scale);
            pnlMainTexts.Padding = textsPanelReferencePadding.Scale(scale);
            lblMessage.Padding = lblDetailsMain.Padding = labelReferencePadding.Scale(scale);
            pnlProgressBar.Padding = progressBarReferencePadding.Scale(scale);
            pbProgress.Height = progressBarReferenceHeight.Scale(scale.Y);
            pnlRadioButtons.Padding = pnlCommandLinks.Padding = controlsPanelReferencePadding.Scale(scale);
            pnlButtons.Margin = buttonsPanelReferenceMargin.Scale(scale);
            pnlButtons.Padding = buttonsPanelReferencePadding.Scale(scale);
            btnShowHideDetails.Margin = expandoButtonReferenceMargin.Scale(scale);
            chbCheckBox.Margin = checkBoxReferenceMargin.Scale(scale);
            lblFooter.Margin = footerReferenceMargin.Scale(scale);
            lblFooter.Padding = lblDetailsFooter.Padding = footerReferencePadding.Scale(scale);

            // exiting if buttons will be completely reset
            if (isFullReset)
                return;

            foreach (AdvancedButton button in pnlButtons.Controls)
                button.Margin = buttonsReferenceMargin.Scale(scale);
            foreach (AdvancedRadioButton radioButton in pnlRadioButtons.Controls)
                radioButton.Padding = radioButtonsReferencePadding.Scale(scale);
        }

        private void ResetChecksWidth(Configuration cfg, bool minSize)
        {
            if (!minSize && !cfg.HasButtons)
                return;

            int maxWidth = pnlMainControls.Width / 2;
            int desiredWidth = 0;

            // not calculating the minimum size requirement if there are no custom buttons and not the minimum size is set
            if (minSize || !cfg.HasCommandLinks)
            {
                // setting minimum size so buttons may consume the rest place: counting desiredWidth from checks
                if (cfg.HasVerification)
                    desiredWidth = chbCheckBox.GetPreferredSize(Size.Empty).Width + chbCheckBox.Margin.Horizontal + pnlChecks.Margin.Horizontal;

                // Expando texts are calculated even if invisible, so checks panel is not rearranged when details text appears
                if (desiredWidth < maxWidth)
                    desiredWidth = Math.Max(desiredWidth, btnShowHideDetails.GetPreferredSize(Size.Empty).Width) + btnShowHideDetails.Margin.Horizontal + pnlChecks.Margin.Horizontal;
            }

            if (!minSize)
            {
                // Sharing the remaining size that is not needed by the buttons.
                // This may provide enough place for changing show/hide details or checkbox text, and also for changing button texts/elevated statuses
                // Counting desiredWidth from buttons. Precondition: form width and buttons width is calculated now.
                int buttonsDesiredWidth = pnlButtons.Width + pnlButtons.Margin.Horizontal;

                if (cfg.HasCommandLinks)
                    // There are no custom buttons: offering the maximum remaining size to the checkbox and the expando button
                    desiredWidth = ClientSize.Width - buttonsDesiredWidth;
                else
                    // halving the remaining size between buttons and the checkbox/expando button
                    desiredWidth = (desiredWidth + ClientSize.Width - buttonsDesiredWidth) / 2;
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
                    chbCheckBox.Visible = cfg.HasVerification;
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
                            pnlMainControls.ColumnStyles[0].Width = pnlMainControls.Width / 2f; // will be refined in ResetChecksWidth
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

        private void ResetRadioButtons(Configuration cfg, PointF scale)
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
                    AutoSize = true,
                    Name = radioButton.Name,
                    Text = radioButton.Text,
                    Checked = !checkedSet && radioButton.Checked,
                    Enabled = radioButton.Enabled,
                    Dock = DockStyle.Top,
                    Padding = radioButtonsReferencePadding.Scale(scale),
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

                ToolTip.SetToolTip(rb, radioButton.Description);
                radioButton.Id = index++;
                pnlRadioButtons.Controls.Add(rb);
                rb.BringToFront();
            }
        }

        /// <summary>
        /// Resets standard and custom buttons (not command links).
        /// </summary>
        private void ResetButtons(Configuration cfg, PointF scale)
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
                AddStandardButton(TaskDialogStandardButtonFlags.OK, scale);
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
                            Margin = buttonsReferenceMargin.Scale(scale),
                            MinimumSize = buttonReferenceSize.Scale(scale),
                            MaximumSize = new Size(Int32.MaxValue, buttonReferenceSize.Height.Scale(scale.Y)),
                            IsElevated = button.IsElevated
                        };

                        btn.Click += Button_Click;
                        ToolTip.SetToolTip(btn, button.Description);
                        button.Id = index++;
                        if (!button.IsElevated && button.CustomIcon != null)
                            ResetButtonIcon(btn, button.CustomIcon, scale);

                        pnlButtons.Controls.Add(btn);
                    }
                }

                // standard buttons
                if (host.StandardButtons != TaskDialogStandardButtonFlags.None)
                {
                    foreach (TaskDialogStandardButtonFlags flag in Enum<TaskDialogStandardButtonFlags>.GetFlags(host.StandardButtons))
                        AddStandardButton(flag, scale);
                }
            }
        }

        private void ResetCommandLinks(Configuration cfg, PointF scale)
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
                    ResetCommandLinkIcon(btn, button.CustomIcon, scale);

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
                // standard button
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

        private void ResetHeights(Configuration cfg, Point? suggestedCenter = null)
        {
            // after resetting visibilities, ResetHeights always called
            if (isResettingVisibilities)
                return;

            if (isResettingHeight)
            {
                isResetHeightPending = true;
                return;
            }

            Screen screen = Screen.FromControl(this);
            Rectangle screenBounds = screen.WorkingArea;
            int screenHeight = screenBounds.Height;
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
                            pnlMainInstruction.Height = pnlMainIconBackground.Height = Math.Max(lblMainInstruction.Height, pnlMainIconBackground.MinimumSize.Height);
                        else
                            pnlMainInstruction.Height = lblMainInstruction.Height + pnlMainInstruction.Padding.Vertical;
                    }
                    else if (isSpecialHeadColors)
                        pnlMainIconBackground.Height = pnlMainIconBackground.MinimumSize.Height;

                    // Workaround: pnlMainTexts.Height (AutoSize does not work)
                    if (cfg.HasMainText)
                    {
                        pnlMainTexts.Height = (cfg.HasMessage ? lblMessage.Height : 0)
                            + (cfg.HasDetails && cfg.IsDetailsVisibleInMain ? lblDetailsMain.Height : 0)
                            + pnlMainTexts.Padding.Vertical;
                    }
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

                    if (cfg.HasProgressBar)
                        pnlProgressBar.Height = pbProgress.Height + pnlProgressBar.Padding.Vertical;

                    // pnlMain(Content) height (AutoSize does not work correctly)
                    int desiredHeight;
                    if (cfg.HasCommandLinks)
                        desiredHeight = pnlCommandLinks.Top + pnlCommandLinks.Height;
                    else if (cfg.HasRadioButtons)
                        desiredHeight = pnlRadioButtons.Top + pnlRadioButtons.Height;
                    else if (cfg.HasProgressBar)
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
                                desiredHeight += chbCheckBox.Height + chbCheckBox.Margin.Vertical;

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

                SetHeight(Math.Min(desiredClientHeight + heightClientDiff, screenHeight), suggestedCenter, screen);
            }
            finally
            {
                isResettingHeight = false;
            }

            if (isResetHeightPending)
            {
                isResetHeightPending = false;
                ResetHeights(cfg, suggestedCenter);
            }
        }

        private void ResetWidths(Configuration cfg, PointF scale, Point? suggestedCenter = null)
        {
            isResizing = true;
            try
            {
                // setting form width
                Screen screen = Screen.FromControl(this);
                Rectangle screenBounds = screen.WorkingArea;
                int screenWidth = screenBounds.Width;
                int minimumWidth = cfg.DluToPixelsX(formReferenceMinWidth).Scale(scale.X);
                if (host.Width > 0)
                {
                    int desiredWidth = Math.Max(minimumWidth, cfg.DluToPixelsX(host.Width).Scale(scale.X));
                    SetWidth(Math.Min(desiredWidth, screenWidth), suggestedCenter, screen);
                }
                // auto width
                else
                {
                    // regular buttons: up to screen width
                    int desiredWidth = minimumWidth;
                    if (cfg.HasButtons)
                    {
                        // setting button sizes without limits to get desired size
                        pnlButtons.SuspendLayout();
                        try
                        {
                            foreach (Button button in pnlButtons.Controls)
                                button.Size = button.GetPreferredSize(Size.Empty);
                        }
                        finally
                        {
                            pnlButtons.ResumeLayout();
                        }

                        int preferredWidth = pnlButtons.GetPreferredSize(Size.Empty).Width + pnlButtons.Margin.Horizontal + pnlButtons.Padding.Horizontal;
                        if (cfg.HasVerification || cfg.HasDetails)
                            preferredWidth += checkBoxAndExpandoColumnReferenceWidth.Scale(scale.X);
                        if (preferredWidth > desiredWidth)
                            desiredWidth = preferredWidth;
                    }

                    // lblMessage, lblDetailsMain, command links text: up to 280 DLU
                    int maxWidth = cfg.DluToPixelsX(messageReferenceMaxWidth).Scale(scale.X);
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
                    SetWidth(Math.Min(desiredWidth + widthClientDiff, screenWidth), suggestedCenter, screen);
                }

                // setting pnlChecks minimum width (It always has priority regardless of form width. Its maximum size is smaller than minimum form size so it is ok)
                if (cfg.HasVerification || cfg.HasDetails)
                    ResetChecksWidth(cfg, true);

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
                    ResetChecksWidth(cfg, false);
            }
            finally
            {
                isResizing = false;
            }
        }

        private void SetWidth(int width, Point? suggestedCenter, Screen screen)
        {
            bool adjustExceeding = suggestedCenter == null;
            suggestedCenter ??= new Point(Left + Width / 2, Top + Height / 2);
            Bounds = new Rectangle(suggestedCenter.Value.X - width / 2, suggestedCenter.Value.Y - Height / 2, width, Height).EnsureScreen(screen, adjustExceeding);
        }

        private void SetHeight(int height, Point? suggestedCenter, Screen screen)
        {
            bool adjustExceeding = suggestedCenter == null;
            suggestedCenter ??= new Point(Left + Width / 2, Top + Height / 2);
            Bounds = new Rectangle(suggestedCenter.Value.X - Width / 2, suggestedCenter.Value.Y - height / 2, Width, height).EnsureScreen(screen, adjustExceeding);
        }

        private void AddStandardButton(TaskDialogStandardButtonFlags standardButton, PointF scale)
        {
            AdvancedButton btn = new AdvancedButton
            {
                UseVisualStyleBackColor = true,
                AutoSize = false,
                Margin = buttonsReferenceMargin.Scale(scale),
                Size = new Size(70, 23).Scale(scale),
                MinimumSize = buttonReferenceSize.Scale(scale),
                MaximumSize = new Size(Int32.MaxValue, buttonReferenceSize.Height.Scale(scale.Y)),
            };
            if ((host.Options & TaskDialogOptions.TranslateStandardButtons) != TaskDialogOptions.None)
                btn.Text = Res.Get(standardButton);
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
                        throw new InvalidOperationException(Res.InternalError($"Unexpected button: {standardButton}"));
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
                    throw new InvalidOperationException(Res.InternalError($"Unexpected button: {standardButton}"));
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
                btnShowHideDetails.TextExpanded = Res.TaskDialogHideDetails;
                btnShowHideDetails.TextCollapsed = Res.TaskDialogShowDetails;
            }
            else
            {
                btnShowHideDetails.TextExpanded = String.IsNullOrEmpty(host.HideDetailsText) ? host.ShowDetailsText : host.HideDetailsText;
                btnShowHideDetails.TextCollapsed = String.IsNullOrEmpty(host.ShowDetailsText) ? host.HideDetailsText : host.ShowDetailsText;
            }
        }

        private void ResetTheme(PointF scale)
        {
            // clearing caches
            btnShowHideDetails.ResetTheme();
            mainInstructionsFont?.Dispose();
            mainInstructionsFont = null;

            // font
            if (!String.IsNullOrEmpty(host.MainInstruction))
            {
                // MainInstructionsFont always returns the system scale size so scaling if needed
                Font font = MainInstructionsFont;
                lblMainInstruction.Font = scale == ScaleHelper.SystemScale ? font : new ScalingFont(font, ScaleHelper.SystemScale).GetScaled(scale);
            }
            else
                lblMainInstruction.Font = Font;

            // colors
            bool isThemed = VisualStyleHelper.RenderWithVisualStyles;
            bool highContrast = VisualStyleHelper.HighContrast;
            cacheMainInstructionsColor ??= isThemed; // Not allowing caching the themed fore color if starting with non-themed rendering. See more details in ThemedMainInstructionsColor.
            pnlMain.BackColor = isThemed ? SystemColors.Window : SystemColors.Control;
            pnlMain.ForeColor = isThemed ?
                SystemColors.WindowText :
                SystemColors.WindowText.ToColor32().TolerantEquals(SystemColors.Control, 128) ? SystemColors.ControlText : SystemColors.WindowText;
            pnlCommandLinks.BackColor = isThemed ? SystemColors.Window : SystemColors.Control;
            pnlCommandLinks.ForeColor = isThemed ? SystemColors.WindowText : SystemColors.ControlText;
            Color dividerBottom = !isThemed ? SystemColors.Control
                : highContrast ? SystemColors.WindowText
                : dividerBottomDefaultThemedColor;
            Color dividerTop = isThemed ? SystemColors.Window : SystemColors.GrayText;
            pnlDividerMainBottom.BackColor = dividerBottom;
            pnlDividerControlsBottom.BackColor = dividerBottom;
            pnlDividerFooterTop.BackColor = dividerTop;
            pnlDividerFooterBottom.BackColor = dividerBottom;
            pnlDividerDetailsFooterTop.BackColor = dividerTop;
            if (isSpecialHeadColors)
            {
                lblMainInstruction.ForeColor = host.Icon == TaskDialogStandardIcons.SecurityWarning ? Color.Black : Color.White;
                pnlMainIconBackground.BackColor = gradientStart;
            }
            else
            {
                Color foreColor;
                if (isThemed)
                {
                    foreColor = ThemedMainInstructionsColor;
                    if (foreColor.ToColor32().TolerantEquals(SystemColors.Window, 128))
                        foreColor = SystemColors.WindowText;
                }
                else
                    foreColor = SystemColors.ControlText;

                lblMainInstruction.ForeColor = foreColor;
                pnlMainIconBackground.BackColor = pnlMain.BackColor;
            }

            // progress bar
            if (!OSUtils.IsVistaOrLater)
                pbProgress.Style = AdvancedProgressBarStyle.ThemedShiny;
        }

        private void ResetMainIcon(Configuration cfg, PointF scale)
        {
            bool hasMainIcon = cfg.HasMainIcon;
            pnlMainIcon.Visible = hasMainIcon;
            pnlMain.ColumnStyles[0].Width = hasMainIcon ? mainIconBackgroundReferenceWidth.Scale(scale.X) : 0;
            bool requireSpecialHeadColors = !VisualStyleHelper.HighContrast && host.Icon.In(iconsWithColoredHeader);

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
                icon = TaskDialog.DefaultIcon;
            else if (host.CustomIcon != null)
                icon = host.CustomIcon;
            else
                icon = host.Icon.ToIcon();

            pbMainIcon.Image?.Dispose();
            if (hasMainIcon)
            {
                using var resizedIcon = icon.Resize(IconsHelper.LargeIconReferenceSize.Scale(scale));
                pbMainIcon.Image = resizedIcon.ExtractBitmap(0);
            }
            else
                pbMainIcon.Image = null;

            if (ShowIcon)
            {
                // Bug: cannot dispose previous icon because DefaultIcon can be disposed, too.
                var formIcon = host.FormIcon ?? icon;
                Icon = formIcon;
            }

            if (dialogState != TaskDialogStatus.Initializing &&
                (isSpecialHeadColors != requireSpecialHeadColors || requireSpecialHeadColors))
            {
                // if changing to special colors, increasing height forward to prevent
                // the flickering if scrollbars. 14 is the padding difference
                if (isSpecialHeadColors != requireSpecialHeadColors && requireSpecialHeadColors)
                    Height += 14;

                isSpecialHeadColors = requireSpecialHeadColors;
                ResetTheme(scale);
                pnlMainInstruction.Invalidate();
                ResetHeights(GetConfiguration());
            }
            else
                isSpecialHeadColors = requireSpecialHeadColors;
        }

        private void ResetFooterIcon(Configuration cfg, PointF scale)
        {
            bool hasFooterIcon = cfg.HasFooterIcon;
            pnlFooterIcon.Visible = hasFooterIcon;
            pnlFooter.ColumnStyles[0].Width = hasFooterIcon ? footerIconColumnReferenceWidth.Scale(scale.X) : 0;
            if (hasFooterIcon)
                pbFooterIcon.Height = IconsHelper.SmallIconReferenceSize.Height.Scale(scale.Y);

            pbFooterIcon.Image?.Dispose();
            if (!hasFooterIcon)
            {
                pbFooterIcon.Image = null;
                return;
            }

            Icon icon = host.CustomFooterIcon ?? host.FooterIcon.ToIcon();
            using Icon resizedIcon = icon.Resize(IconsHelper.SmallIconReferenceSize.Scale(scale));
            pbFooterIcon.Image = resizedIcon.ExtractBitmap(0);
            if (host.CustomFooterIcon == null)
                icon.Dispose();
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
                    ResetVisibilities(cfg = GetConfiguration());

                // because of suspending, control is not resized here
                if (!updateDescription)
                    control.Text = value;
                else
                    ((CommandLinkButton)control).Description = value;

                if (control.Visible)
                {
                    // Adjusting form height to prevent scrollbar flickering
                    if (control.AutoSize)
                    {
                        if ((preferredSize = control.GetPreferredSize(new Size(control.Width, 0))).Height > origSize.Height)
                        {
                            // possible divider visibility change
                            if (visibilityChange)
                                preferredSize.Height += 2;

                            Height += preferredSize.Height - origSize.Height;
                        }
                    }
                    else
                        preferredSize = control.GetPreferredSize(Size.Empty);
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
            else
                control.Size = preferredSize;
            if (visibilityChange || origSize != preferredSize)
                ResetHeights(cfg ?? GetConfiguration());

            // workaround: hide scrollbar if it gets accidentally visible
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
                        ResetCommandLinkIcon(commandLinkButton, taskDialogButton.CustomIcon, DeviceScale);
                    else if (commandLinkButton.Image != null)
                        ResetCommandLinkIcon(commandLinkButton, null, default);

                    // Adjusting form height to prevent scrollbar flickering
                    if ((preferredSize = control.GetPreferredSize(new Size(control.Width, 0))).Height > origSize.Height)
                        Height += preferredSize.Height - origSize.Height;
                }
                else
                {
                    AdvancedButton button = ((AdvancedButton)control);
                    button.IsElevated = taskDialogButton.IsElevated;
                    if (!taskDialogButton.IsElevated && taskDialogButton.CustomIcon != null)
                        ResetButtonIcon(button, taskDialogButton.CustomIcon, DeviceScale);
                    else if (button.Image != null)
                        ResetButtonIcon(button, null, default);
                    preferredSize = control.GetPreferredSize(Size.Empty);
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
            else
                control.Size = preferredSize;
            if (origSize != preferredSize)
                ResetHeights(GetConfiguration());

            // workaround: hide scrollbar if it gets accidentally visible
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
                    control.BringToFront();
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
                    return pnlCommandLinks.Controls[host.Buttons.Count - taskDialogControl.Id - 1];

                // custom button - if there is no standard button, direct indexing
                if (host.StandardButtons == TaskDialogStandardButtonFlags.None)
                    return pnlButtons.Controls[taskDialogControl.Id];

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

        private void CopyToClipboard()
        {
            #region Local Methods

            static string? Strip(string? text)
            {
                if (text == null || !text.Contains('&'))
                    return text;
                
                if (!text.Contains("&&"))
                    return text.Replace("&", String.Empty);

                return text.Split(["&&"], StringSplitOptions.None).Select(s => s.Replace("&", String.Empty)).Join('&');
            }

            #endregion

            var result = new StringBuilder();
            Configuration cfg = GetConfiguration();

            result.AppendLine(Res.TaskDialogCaption);
            result.AppendLine(Text);
            if (cfg.HasMainInstruction)
            {
                result.AppendLine();
                result.AppendLine(Res.TaskDialogMainInstruction);
                result.AppendLine(host.MainInstruction);
            }

            if (cfg.HasMessage)
            {
                result.AppendLine();
                result.AppendLine(Res.TaskDialogMessage);
                result.AppendLine(host.Message);
            }

            if (cfg.HasDetails && !isDetailsInFooter)
            {
                result.AppendLine();
                result.AppendLine(Res.TaskDialogDetails);
                result.AppendLine(host.DetailsText);
            }

            if (cfg.HasRadioButtons)
            {
                result.AppendLine();
                foreach (TaskDialogRadioButton radioButton in host.RadioButtons)
                    result.AppendLine(radioButton.Checked ? Res.TaskDialogRadioButtonChecked(Strip(radioButton.Text)) : Res.TaskDialogRadioButtonUnchecked(Strip(radioButton.Text)));
            }

            if (cfg.HasCommandLinks)
            {
                bool useGlyph = (host.Options & TaskDialogOptions.UseCommandLinks) != 0;
                result.AppendLine();
                foreach (TaskDialogButton button in host.Buttons)
                {
                    if (button.IsElevated)
                        result.AppendLine(Res.TaskDialogButtonElevated(Strip(button.Text)));
                    else if (button.CustomIcon != null)
                        result.AppendLine(Res.TaskDialogButtonCustomIcon(Strip(button.Text)));
                    else if (useGlyph)
                        result.AppendLine(Res.TaskDialogButtonCommandLink(Strip(button.Text)));
                    else if (useGlyph)
                        result.AppendLine(Res.TaskDialogButton(Strip(button.Text)));
                }
            }

            if (cfg.HasDetails)
            {
                result.AppendLine();
                result.Append(isDetailsExpanded ? Res.TaskDialogExpandoButtonExpanded(Strip(btnShowHideDetails.Text)) : Res.TaskDialogExpandoButtonCollapsed(Strip(btnShowHideDetails.Text)));
                if (cfg.HasVerification || !cfg.HasButtons)
                    result.AppendLine();
            }

            if (cfg.HasVerification)
            {
                result.AppendLine();
                result.Append(host.CheckBoxChecked ? Res.TaskDialogCheckBoxChecked(Strip(host.CheckBoxText)) : Res.TaskDialogCheckBoxUnchecked(Strip(host.CheckBoxText)));
                if (!cfg.HasButtons)
                    result.AppendLine();
            }

            if (cfg.HasButtons)
            {
                if (!cfg.HasVerification && !cfg.HasDetails)
                    result.AppendLine();
                else
                    result.Append("  ");

                bool first = true;
                foreach (var button in pnlButtons.Controls.OfType<AdvancedButton>())
                {
                    if (!first)
                        result.Append(" ");
                    first = false;

                    if (button.IsElevated)
                        result.Append(Res.TaskDialogButtonElevated(Strip(button.Text)));
                    else if (button.Image != null)
                        result.Append(Res.TaskDialogButtonCustomIcon(Strip(button.Text)));
                    else
                        result.Append(Res.TaskDialogButton(Strip(button.Text)));
                }

                result.AppendLine();
            }

            if (cfg.HasFooter)
            {
                result.AppendLine();
                result.AppendLine(Res.TaskDialogFooter);
                result.AppendLine(host.FooterText);
            }

            if (cfg.HasDetails && isDetailsInFooter)
            {
                result.AppendLine();
                result.AppendLine(Res.TaskDialogDetails);
                result.AppendLine(host.DetailsText);
            }

            Clipboard.SetText(result.ToString());
        }

        #endregion

        #region Explicitly Implemented Interface Methods

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
            executeNonModal = owner == IntPtr.Zero && OSUtils.IsWindows && !OSUtils.IsMono;
            if (owner != IntPtr.Zero)
                ownerWindow = new Win32Window { Handle = owner };
            
            FirstInit();

            // This forces to create the handle. May cause some resets and additional DPI changes, but it's still better than handling
            // the side effects of the deferred handle creation (e.g. the ResumeLayout in ResetHeights may change the screen,
            // recursive reentrancy in OnDeviceScaleChanged when setting MinimumSize in ResetConstraints, etc.).
            host.Handle = Handle;
            ResetSettings();

            // showing the dialog
            do
            {
                // If the native task dialog is shown without an owner, it does not block its caller, while works as ShowDialog in terms of blocking the call
                // until the form is closed. Here we mimic the same behavior: though the top-level windows will be blocked by ShowDialog,
                // we unblock them once this form is shown. Additionally, the currently active window becomes the owner of this form, which we reset as well.
                if (ownerWindow == null)
                    ShowDialog(); // there is no Show method that is both blocking and non-modal, so we use ShowDialog here, and adjusting the owner in OnShown
                else
                    ShowDialog(ownerWindow);

                // the handle of the owner may change, too
                if (isRtlChanging && ownerWindow != null)
                {
                    IntPtr newOwner = User32.GetActiveWindow();
                    if (newOwner != IntPtr.Zero)
                        ownerWindow = new Win32Window { Handle = User32.GetActiveWindow() };
                }
            } while (isRtlChanging);

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

            checkBoxChecked = chbCheckBox.Checked;
            return result;
        }

        void ITaskDialog.PropertyChanged(string propName)
        {
            if (dialogState == TaskDialogStatus.Initializing || dialogState == TaskDialogStatus.Closed)
                throw new InvalidOperationException(Res.InternalError("Changing property in invalid state."));

            Configuration cfg;
            PointF scale = this.GetScale();
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
                    UpdateText(chbCheckBox, host.CheckBoxText, true, false);
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

                        ResetButtons(cfg, scale);
                    }
                    finally
                    {
                        ResumeLayout();
                    }

                    ResetDefaultButton(cfg);
                    if (host.Width == 0)
                        ResetWidths(cfg, scale);

                    ResetHeights(cfg);
                    return;

                case TaskDialog.PropertyDefaultStandardButton:
                    ResetDefaultButton(GetConfiguration());
                    return;

                case TaskDialog.PropertyWidth:
                    ResetWidths(cfg = GetConfiguration(), scale);
                    ResetHeights(cfg);
                    return;

                case TaskDialog.PropertyOptions:
                    ResetSettings();
                    return;

                case TaskDialog.PropertyCheckBoxChecked:
                    isCheckboxChecking = true;
                    try
                    {
                        chbCheckBox.Checked = host.CheckBoxChecked;
                    }
                    finally
                    {
                        isCheckboxChecking = false;
                    }
                    return;

                case TaskDialog.PropertyIcon:
                case TaskDialog.PropertyCustomIcon:
                    ResetMainIcon(GetConfiguration(), this.GetScale());
                    return;

                case TaskDialog.PropertyFooterIcon:
                case TaskDialog.PropertyCustomFooterIcon:
                    ResetFooterIcon(GetConfiguration(), this.GetScale());
                    return;

                case TaskDialog.PropertyProgressBarStyle:
                    if (host.ProgressBarStyle == TaskDialogProgressBarStyle.None || !pbProgress.Visible)
                    {
                        // turning off progress bar
                        if (host.ProgressBarStyle == TaskDialogProgressBarStyle.None)
                            pnlProgressBar.Visible = false;
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
                            ToolTip.SetToolTip(control, button.Description);
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
                        ToolTip.SetToolTip(control, radioButton.Description);
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
            PointF scale = this.GetScale();
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
                    ResetButtons(cfg, scale);
                else
                    ResetCommandLinks(cfg, scale);
            }
            finally
            {
                ResumeLayout();
            }

            ResetDefaultButton(cfg);
            if (host.Width == 0 || buttonsChanged)
                ResetWidths(cfg, scale);

            ResetHeights(cfg);
        }

        void ITaskDialog.RadioButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index)
        {
            Configuration cfg = GetConfiguration();
            SuspendLayout();
            try
            {
                // updating visibilities if the radio buttons panel will just appear/disappear
                if (!pnlRadioButtons.Visible && cfg.HasRadioButtons || pnlRadioButtons.HasChildren && !cfg.HasRadioButtons)
                    ResetVisibilities(cfg);

                ResetRadioButtons(cfg, this.GetScale());
            }
            finally
            {
                ResumeLayout();
            }

            ResetHeights(cfg);
        }

        void ITaskDialog.TimerChanged(bool enabled) => timer.Enabled = enabled;

        #endregion

        #region Event Handlers
        // ReSharper disable InconsistentNaming

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
                if ((host.Icon != TaskDialogStandardIcons.None || host.CustomIcon != null) && !(hasMainInstruction && host.Icon.In(iconsWithColoredHeader)))
                    iconHeight = pnlMainIconBackground.MinimumSize.Height;

                int minHeight = Math.Max(pnlMainTexts.MinimumSize.Height, iconHeight - (hasMainInstruction ? lblMainInstruction.Height : 0));
                int mainTextHeight = Math.Max(minHeight,
                    (isDetailsExpanded ? detailsHeight : 0)
                    + (!String.IsNullOrEmpty(host.Message) ? lblMessage.Height : 0)
                    + pnlMainTexts.Padding.Vertical);
                diff = mainTextHeight - Math.Max(pnlMainTexts.Height, minHeight);
                int formHeight = Math.Min(Height + diff, screenHeight);
                resetHeightsNeeded = Height >= screenHeight || Height + diff > screenHeight;

                // when expanding, setting form height first to prevent appearing scrollbars for a moment
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

            // workaround: hide scrollbar if it gets accidentally visible
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
            if (!Visible || isResizing || WindowState == FormWindowState.Minimized)
                return;

            ResetHeights(GetConfiguration());
        }

        private void cbCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (dialogState == TaskDialogStatus.Initializing || isCheckboxChecking)
                return;

            host.OnCheckBoxCheckedChanged(chbCheckBox.Checked);
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

        private void AdvancedLabel_HyperlinkClicked(object? sender, HyperlinkClickedEventArgs e) => host.OnHyperlinkClicked(e);

        private void VisualStyleHelper_VisualStylesChanged(object? sender, EventArgs e)
        {
            PointF scale = this.GetScale();
            Configuration cfg = GetConfiguration();
            mainInstructionsColor = Color.Empty;
            ResetTheme(scale);
            ResetWidths(cfg, scale);
            ResetHeights(cfg);
        }

        // ReSharper restore InconsistentNaming
        #endregion

        #endregion

        #endregion
    }
}
