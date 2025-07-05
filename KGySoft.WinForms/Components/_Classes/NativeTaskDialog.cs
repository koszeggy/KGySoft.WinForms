#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NativeTaskDialog.cs
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms.Components
{

    /// <summary>
    /// A wrapper class around the in-built task dialog available from Vista
    /// </summary>
    internal sealed class NativeTaskDialog : ITaskDialog
    {
        #region Nested Classes

        private sealed class NativeWindowHandler : NativeWindow
        {
            #region Fields

            private readonly NativeTaskDialog owner;

            #endregion

            #region Constructors

            internal NativeWindowHandler(NativeTaskDialog owner)
            {
                this.owner = owner;
                AssignHandle(owner.dialogHandle);
            }

            #endregion

            #region Methods

            protected override void WndProc(ref Message m)
            {
                switch (m.Msg)
                {
                    case Constants.WM_DPICHANGED:
                        base.WndProc(ref m);
                        
                        owner.scale = default;

                        Icon? customMainIcon = owner.host.CustomIcon ?? owner.host.EmulatedStandardMainIcon;
                        Icon? customFooterIcon = owner.host.CustomFooterIcon ?? owner.host.EmulatedStandardFooterIcon;

                        // The condition of reallocation is actually more complex, but in worst case the UpdateCustomIcon call(s) below
                        // will reallocate the dialog. If this happens, it might be reallocated twice, but for the next time isEverReallocated will be true.
                        if ((customFooterIcon != null || customMainIcon != null) && owner.isEverReallocated)
                        {
                            owner.ReallocateDialog();
                            return;
                        }

                        if (customMainIcon != null)
                            owner.UpdateCustomIcon(Constants.TDI_MAIN, customMainIcon);
                        if (customFooterIcon != null)
                            owner.UpdateCustomIcon(Constants.TDI_FOOTER, customFooterIcon);
                        return;

                    default:
                        base.WndProc(ref m);
                        return;
                }
            }

            #endregion
        }

        #endregion

        #region Constants

        private const int firstButtonId = 1000;
        private const int firstRadioButtonId = 10000;

        #endregion

        #region Fields

        #region Static Fields

        private static readonly TaskDialogStandardIcons[] whiteBackgroundIcons = new[] { TaskDialogStandardIcons.None, TaskDialogStandardIcons.Information, TaskDialogStandardIcons.Warning, TaskDialogStandardIcons.Error, TaskDialogStandardIcons.SecurityShield };

        #endregion

        #region Instance Fields

        private readonly TaskDialogCallbackProc callback;

        private TaskDialogStatus dialogState = TaskDialogStatus.Initializing;
        private TaskDialog host = null!;
        private IntPtr ownerHandle;
        private IntPtr dialogHandle;
        private Dictionary<TASKDIALOG_ELEMENTS, IntPtr>? updatedTexts;
        private bool isForcedClosing;
        private bool ignoreFirstRadioButtonCheck;
        TASKDIALOGCONFIG config;
        private int eventHandlerCount;
        private bool isReallocatePending;
        private bool isCheckedChanging;
        private bool isRadioButtonClicked;
        private bool isEverReallocated;
        private Icon? mainIcon;
        private Icon? footerIcon;
        private Icon? smallFormIcon;
        private NativeWindowHandler? nativeHandler;
        private PointF scale;

        #endregion

        #endregion

        #region Properties

        #region Static Properties

        /// <summary>
        /// Returns true if the current operating system and application supports native TaskDialog.
        /// </summary>
        internal static bool IsAvailable
        {
            get
            {
                // NOTE: it is possible to activate Comctl32 V6 even with disabled visual styles 
                // but it fails if Comctl32 V5 is already loaded. In that case reporting false here.
                return OSHelper.IsWindowsVistaOrLater && VisualStyleHelper.InitializedWithVisualStyles && ThemingActivationContext.IsThemingAvailable;
            }
        }

        #endregion

        #region Instance Properties
        
        #region Private Properties

        private PointF Scale
        {
            get
            {
                if (scale.IsEmpty)
                    scale = dialogHandle == IntPtr.Zero ? ScaleHelper.SystemScale : ScaleHelper.GetScale(dialogHandle);
                return scale;
            }
        }

        private bool HasFormIcon => config.hwndParent == IntPtr.Zero || (host.Options & TaskDialogOptions.ForceShowSysMenu) != 0;
        private bool HasForcedFormIcon => config.hwndParent != IntPtr.Zero && (host.Options & TaskDialogOptions.ForceShowSysMenu) != 0;
        private bool ShowInTaskbar => config.hwndParent == IntPtr.Zero || (host.Options & TaskDialogOptions.ForceShowInTaskbar) != 0;

        #endregion

        #region Explicitly Implemented Interface Properties

        TaskDialogStatus ITaskDialog.ShowState => dialogState;

        #endregion

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        internal NativeTaskDialog() => callback = ProcessDialogMessages;

        #endregion

        #region Destructor

        ~NativeTaskDialog() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        /// <summary>
        /// Building native buttons and returning the unamanged pointer to the result array
        /// </summary>
        private static IntPtr AllocateButtons(IList? buttons, int startId, bool needDescription)
        {
            if (buttons == null || buttons.Count == 0)
                return IntPtr.Zero;

            // building native structures
            TASKDIALOG_BUTTON[] nativeButtons = new TASKDIALOG_BUTTON[buttons.Count];
            for (int i = 0; i < nativeButtons.Length; i++)
            {
                TaskDialogButtonBase button = (TaskDialogButtonBase)buttons[i]!;
                button.Id = nativeButtons[i].nButtonID = i + startId;
                StringBuilder text = new StringBuilder(button.Text ?? String.Empty);
                if (needDescription && !String.IsNullOrEmpty(button.Description))
                {
                    // bug: if there is description without text, Out of memory exception occurs so appending a space
                    if (text.Length == 0)
                        text.Append(" ");
                    text.Append("\n"); // Environment.NewLine would cause double new lines
                    text.Append(button.Description);
                }

                // bug: text is empty, Out of memory exception occurs so assigning a space
                nativeButtons[i].pszButtonText = text.Length == 0 ? " " : text.ToString();
            }

            // allocating unmanaged memory and marshaling elements
            int buttonSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
            IntPtr result = Marshal.AllocHGlobal(nativeButtons.Length * buttonSize);
            for (int i = 0; i < nativeButtons.Length; i++)
                Marshal.StructureToPtr(nativeButtons[i], new IntPtr((long)result + i * buttonSize), false);

            return result;
        }

        /// <summary>
        /// Freeing allocated unmanaged memory for native buttons
        /// </summary>
        private static void FreeButtons(IntPtr buttonsArray, uint count)
        {
            if (buttonsArray == IntPtr.Zero)
                return;

            int buttonSize = Marshal.SizeOf(typeof(TASKDIALOG_BUTTON));
            for (int i = 0; i < count; i++)
                Marshal.DestroyStructure(new IntPtr((long)buttonsArray + i * buttonSize), typeof(TASKDIALOG_BUTTON));

            Marshal.FreeHGlobal(buttonsArray);
        }

        private static bool IsBackgroundDifferent(TaskDialogStandardIcons icon1, TaskDialogStandardIcons icon2)
            => icon1 != icon2 && !(icon1.In(whiteBackgroundIcons) && icon2.In(whiteBackgroundIcons));

        #endregion

        #region Instance Methods

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Can be called multiple times during the life of a native dialog
        /// </summary>
        private void ResetSettings()
        {
            // setting standard configuration
            config = new TASKDIALOGCONFIG();
            config.cbSize = (uint)Marshal.SizeOf(typeof(TASKDIALOGCONFIG));
            config.hwndParent = ownerHandle;
            config.hInstance = IntPtr.Zero;
            config.dwFlags = (TASKDIALOG_FLAGS)((int)host.Options & TaskDialog.NativeOptionsMask);
            config.dwCommonButtons = host.StandardButtons;
            config.pszWindowTitle = host.Caption;
            config.hMainIcon = (IntPtr)host.Icon; // overridden if custom
            config.pszMainInstruction = host.MainInstruction;
            config.pszContent = host.Message;
            config.nDefaultButton = (int)host.DefaultStandardButton; // overridden if there is a default custom button
            config.pszVerificationText = host.CheckBoxText;
            config.pszExpandedInformation = host.DetailsText;
            config.pszExpandedControlText = host.HideDetailsText;
            config.pszCollapsedControlText = host.ShowDetailsText;
            config.hFooterIcon = (IntPtr)host.FooterIcon; // overridden if custom
            config.pszFooter = host.FooterText;
            config.pfCallback = callback;
            config.cxWidth = (uint)host.Width;

            // setting custom main icon
            if (host.Icon is TaskDialogStandardIcons.Question or TaskDialogStandardIcons.SecurityQuestion || host.CustomIcon != null)
            {
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN;
                if (host.CustomIcon is Icon customIcon)
                    config.hMainIcon = ResetMainIcon(customIcon).Handle;
                else
                {
                    // only when initializing, otherwise, will be changed by UpdateStandardIcon
                    if (dialogState == TaskDialogStatus.Initializing)
                        host.EmulatedStandardMainIcon = host.Icon.ToIcon();
                    config.hMainIcon = ResetMainIcon(host.EmulatedStandardMainIcon)?.Handle ?? IntPtr.Zero;
                }
            }

            // setting custom footer icon
            if (host.FooterIcon is TaskDialogStandardIcons.Question or TaskDialogStandardIcons.SecurityQuestion || host.CustomFooterIcon != null)
            {
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER;
                if (host.CustomFooterIcon is Icon customFooterIcon)
                    config.hFooterIcon = ResetFooterIcon(customFooterIcon).Handle;
                else
                {
                    // only when initializing, otherwise, will be changed by UpdateStandardFooterIcon
                    if (dialogState == TaskDialogStatus.Initializing)
                        host.EmulatedStandardFooterIcon = host.FooterIcon.ToIcon();
                    config.hFooterIcon = ResetFooterIcon(host.EmulatedStandardFooterIcon)?.Handle ?? IntPtr.Zero;
                }
            }

            // configuring flags, which are not in TaskDialog.Options because they were redundant
            if (host.CheckBoxChecked)
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_VERIFICATION_FLAG_CHECKED;
            if (host.ProgressBarStyle == TaskDialogProgressBarStyle.Marquee)
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR;
            else if (host.ProgressBarStyle == TaskDialogProgressBarStyle.Regular)
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR;
            if (host.IsTickAssigned)
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_CALLBACK_TIMER;
            if (host.Width == 0)
                config.dwFlags |= TASKDIALOG_FLAGS.TDF_SIZE_TO_CONTENT;

            // configuring custom buttons
            config.pButtons = AllocateButtons(host.Buttons, firstButtonId, (host.Options & (TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon)) != TaskDialogOptions.None);
            if (config.pButtons != IntPtr.Zero)
            {
                config.cButtons = (uint)host.Buttons.Count;
                TaskDialogButton? defaultButton = host.Buttons.FirstOrDefault(b => b.IsDefault);
                if (defaultButton != null)
                    config.nDefaultButton = defaultButton.Id;
            }
            else
                config.dwFlags &= ~(TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS | TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON);

            // configuring radio buttons
            config.pRadioButtons = AllocateButtons(host.RadioButtons, firstRadioButtonId, false);
            if (config.pRadioButtons != IntPtr.Zero)
            {
                config.cRadioButtons = (uint)host.RadioButtons.Count;
                TaskDialogRadioButton? checkedRadioButton = null;
                bool checkedFound = false;
                foreach (TaskDialogRadioButton rb in host.RadioButtons)
                {
                    if (rb.Checked)
                    {
                        if (checkedFound)
                            rb.CheckedInternal = false;
                        else
                        {
                            checkedRadioButton = rb;
                            checkedFound = true;
                        }
                    }
                }

                if (checkedRadioButton != null)
                {
                    config.nDefaultRadioButton = checkedRadioButton.Id;
                    ignoreFirstRadioButtonCheck = true;
                }
                else
                    config.dwFlags |= TASKDIALOG_FLAGS.TDF_NO_DEFAULT_RADIO_BUTTON;
            }
        }

        [return: NotNullIfNotNull(nameof(icon))]
        private Icon? ResetMainIcon(Icon? icon)
        {
            mainIcon?.Dispose();
            return mainIcon = icon?.Resize(IconsHelper.LargeIconReferenceSize.Scale(Scale));
        }

        [return: NotNullIfNotNull(nameof(icon))]
        private Icon? ResetFooterIcon(Icon? icon)
        {
            footerIcon?.Dispose();
            return footerIcon = icon?.Resize(IconsHelper.SmallIconReferenceSize.Scale(Scale));
        }

        private void ResetSmallFormIcon(Icon icon)
        {
            smallFormIcon?.Dispose();
            smallFormIcon = icon.Resize(IconsHelper.SmallIconReferenceSize.Scale(Scale));
            User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_SMALL, smallFormIcon.Handle);
        }

        /// <summary>
        /// The callback that receives messages from the Task Dialog when various events occur.
        /// </summary>
        /// <param name="hwnd">The window handle of the dialog</param>
        /// <param name="uNotification">The message being passed.</param>
        /// <param name="wParam">wParam which is interpreted differently depending on the message.</param>
        /// <param name="lParam">wParam which is interpreted differently depending on the message.</param>
        /// <param name="refData">The refrence data that was set to TaskDialog.CallbackData.</param>
        /// <returns>A HRESULT value. The return value is specific to the message being processed. </returns>
        private int ProcessDialogMessages(IntPtr hwnd, TASKDIALOG_NOTIFICATIONS uNotification, IntPtr wParam, IntPtr lParam, IntPtr refData)
        {
            eventHandlerCount++;
            try
            {
                switch (uNotification)
                {
                    case TASKDIALOG_NOTIFICATIONS.TDN_DIALOG_CONSTRUCTED:
                        // this is executed multiple times, even when the dialog is reallocated, though the handle is always the same
                        dialogHandle = hwnd;
                        host.Handle = hwnd;
                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_CREATED:
                        // performing the rest of the initialization, which needs an already created dialog - executed only once
                        PointF oldScale = scale;
                        scale = default;
                        InitializeCreatedDialog(true);
                        dialogState = TaskDialogStatus.Showing;
                        if (host.Icon == TaskDialogStandardIcons.Question)
                            SystemSounds.Question.Play();
                        if (ScaleHelper.IsThreadPerMonitorAware)
                            nativeHandler = new NativeWindowHandler(this);

                        if (Scale != oldScale)
                        {
                            if ((host.CustomIcon ?? host.EmulatedStandardMainIcon) is Icon customMainIcon)
                                UpdateCustomIcon(Constants.TDI_MAIN, customMainIcon);
                            if ((host.CustomFooterIcon ?? host.EmulatedStandardFooterIcon) is Icon customFooterIcon)
                                UpdateCustomIcon(Constants.TDI_FOOTER, customFooterIcon);
                        }

                        host.OnCreated();
                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_NAVIGATED:
                        // performing the rest of the initialization after the dialog is reallocated
                        InitializeCreatedDialog(false);
                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_BUTTON_CLICKED:
                        // explicit Close call (or setting DialogResult) fires a click event so handling Click only when not already closing
                        bool isClosing = dialogState == TaskDialogStatus.Closing;
                        if (!isClosing)
                        {
                            int index = (int)wParam - firstButtonId;

                            // handling click event of a custom button
                            if (index >= 0 && index < host.Buttons.Count)
                            {
                                HandledEventArgs e = new HandledEventArgs(true);
                                host.Buttons[index].OnClick(e);
                                isClosing = !e.Handled;
                            }
                            else
                                isClosing = true;
                        }

                        // handling if closing the dialog
                        if (isClosing)
                        {
                            // closing from dispose: omitting Closing event
                            if (isForcedClosing)
                            {
                                dialogState = TaskDialogStatus.Closing;
                                return 0;
                            }

                            // TODO? is a DialogResult/button index required in event args? (from wParam)
                            CancelEventArgs e = new CancelEventArgs(false);
                            host.OnClosing(e);
                            isClosing = !e.Cancel;

                            // this may restore showing state after an explicit closing
                            dialogState = isClosing ? TaskDialogStatus.Closing : TaskDialogStatus.Showing;
                        }

                        return Convert.ToInt32(!isClosing);

                    case TASKDIALOG_NOTIFICATIONS.TDN_HYPERLINK_CLICKED:
                        {
                            string link = Marshal.PtrToStringUni(lParam)!;
                            HyperlinkClickedEventArgs e = new HyperlinkClickedEventArgs(link);
                            host.OnHyperlinkClicked(e);
                            return Convert.ToInt32(e.Handled);
                        }

                    case TASKDIALOG_NOTIFICATIONS.TDN_TIMER:
                        {
                            // called only when host.Tick is subscribed so it is not a waste to create event args here
                            TaskDialogTickEventArgs e = new TaskDialogTickEventArgs((int)wParam);
                            host.OnTick(e);
                            return Convert.ToInt32(e.Reset);
                        }

                    case TASKDIALOG_NOTIFICATIONS.TDN_DESTROYED:
                        dialogState = TaskDialogStatus.Closed;
                        host.Handle = IntPtr.Zero;

                        // closing from dispose: omitting Closed event
                        if (!isForcedClosing)
                            host.OnClosed();

                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_RADIO_BUTTON_CLICKED:
                        {
                            // if there is a default radio button, this event is fired even without clicking it
                            if (ignoreFirstRadioButtonCheck)
                            {
                                ignoreFirstRadioButtonCheck = false;
                                return 0;
                            }

                            int index = (int)wParam - firstRadioButtonId;

                            // setting the radio button as checked (if this is a change, event will be fired)
                            if (index >= 0 && index < host.RadioButtons.Count)
                            {
                                isRadioButtonClicked = true;
                                try
                                {
                                    host.RadioButtons[index].Checked = true;
                                }
                                finally
                                {
                                    isRadioButtonClicked = false;
                                }
                            }

                            return 0;
                        }

                    case TASKDIALOG_NOTIFICATIONS.TDN_VERIFICATION_CLICKED:
                        host.OnCheckBoxCheckedChanged((int)wParam == 1);
                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_HELP:
                        host.OnHelpRequested();
                        return 0;

                    case TASKDIALOG_NOTIFICATIONS.TDN_EXPANDO_BUTTON_CLICKED:
                        host.OnDetailsVisibleChanged(new TaskDialogDetailsVisibleChangedEventArgs(wParam != IntPtr.Zero));
                        return 0;

                    default:
                        Debug.Fail("Unsupported notification");
                        return 0;
                }
            }
            finally
            {
                eventHandlerCount--;
                if (isReallocatePending)
                    ReallocateDialog();
            }
        }

        /// <summary>
        /// Initializing things, which can be performed only once the dialog is created.
        /// </summary>
        private void InitializeCreatedDialog(bool isFirstCreate)
        {
            bool hasFormIcon = HasFormIcon;

            // setting forced icon (only if modal)
            if (config.hwndParent != IntPtr.Zero)
            {
                nint oldExStyle = User32.GetWindowLong(host.Handle, Constants.GWL_EXSTYLE);
                nint exStyle = oldExStyle;
                if (hasFormIcon)
                    exStyle &= ~Constants.WS_EX_DLGMODALFRAME;
                else
                    exStyle |= Constants.WS_EX_DLGMODALFRAME;
                if (ShowInTaskbar)
                    exStyle |= Constants.WS_EX_APPWINDOW;
                else
                    exStyle &= ~Constants.WS_EX_APPWINDOW;

                if (exStyle != oldExStyle)
                    User32.SetWindowLong(host.Handle, Constants.GWL_EXSTYLE, exStyle);

                // the code above just toggles the same style as the Form.ShowIcon property
                if (hasFormIcon && host.CustomIcon is null)
                {
                    if (isFirstCreate)
                        host.FormIcon = host.Icon.ToIcon(); // including None, which is the default icon
                    else if (host.Icon == TaskDialogStandardIcons.None)
                        User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON, Constants.TDI_MAIN, IntPtr.Zero);
                }
                // removing the icon
                else if (oldExStyle != exStyle)
                    ClearFormIcon();
            }

            // Custom, (Security)Question or forced icon: setting the good quality icon as form icon (the native dialog would not handle it nicely).
            if (hasFormIcon && host.FormIcon != null)
            {
                User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_BIG, host.FormIcon.Handle);
                ResetSmallFormIcon(host.FormIcon);
            }

            // only when reallocating the dialog (may happen after a custom -> standard icon change), otherwise, will be changed by UpdateStandardIcon
            else if (config.hwndParent == IntPtr.Zero && /*host.Icon != TaskDialogStandardIcons.None &&*/ !isFirstCreate) // on first init this is redundant but when icon has been changed from custom to standard, NAVIGATE does not update title icon
                User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON, Constants.TDI_MAIN, (IntPtr)host.Icon);

            // Progress bar
            if (host.ProgressBarStyle != TaskDialogProgressBarStyle.None)
            {
                if (host.ProgressBarStyle == TaskDialogProgressBarStyle.Marquee)
                    UpdateProgressBarMarqueeAnimationSpeed(host.ProgressBarMarqueeAnimationSpeed);
                else
                {
                    UpdateProgressBarRange((ushort)host.ProgressBarMinimum, (ushort)host.ProgressBarMaximum);

                    // note: setting state first, otherwise, it animates in green until reaches value, then changes color
                    UpdateProgressBarState(host.ProgressBarState);
                    UpdateProgressBarValue(host.ProgressBarValue);
                }
            }

            // Elevated and disabled buttons
            foreach (TaskDialogButton button in host.Buttons)
            {
                if (button.IsElevated)
                    UpdateElevatedStatus(button);
                if (!button.Enabled)
                    UpdateButtonEnabled(button);
            }

            // Disabled radio buttons
            foreach (TaskDialogRadioButton radioButton in host.RadioButtons)
            {
                if (!radioButton.Enabled)
                    UpdateRadioButtonEnabled(radioButton);
            }
        }

        private void UpdateElevatedStatus(TaskDialogButton button)
        {
            // bug workaround: clearing elevated state makes arrow icon appear even if display style is no glyph, so reallocating
            // NOTE: hidden feature possibility: enum TaskDialogButtonIcons { None, Arrow, ElevationIcon } (config is always NOICON, to display arrow set&clear elevated state)
            // actually looks horrible
            if (!button.IsElevated && (config.dwFlags & (TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS | TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON)) == TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON)
            {
                ReallocateDialog();
                return;
            }

            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE, new IntPtr(button.Id), new IntPtr(button.IsElevated ? 1 : 0));
        }

        private void UpdateButtonEnabled(TaskDialogButton button)
            => User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_ENABLE_BUTTON, new IntPtr(button.Id), new IntPtr(button.Enabled ? 1 : 0));

        private void UpdateRadioButtonEnabled(TaskDialogRadioButton radioButton)
            => User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_ENABLE_RADIO_BUTTON, new IntPtr(radioButton.Id), new IntPtr(radioButton.Enabled ? 1 : 0));

        private void UpdateText(TASKDIALOG_ELEMENTS element, string? text)
        {
            IntPtr ptrText;
            if (updatedTexts == null)
                updatedTexts = new Dictionary<TASKDIALOG_ELEMENTS, IntPtr>(EnumComparer<TASKDIALOG_ELEMENTS>.Comparer);
            else if (updatedTexts.TryGetValue(element, out ptrText) && ptrText != IntPtr.Zero)
            {
                // if there is already an updated text for given element, freeing it first
                Marshal.FreeHGlobal(ptrText);
                updatedTexts.Remove(element);
            }

            // allocating unmanaged memory for the new string
            ptrText = Marshal.StringToHGlobalUni(text);
            if (ptrText != IntPtr.Zero)
                updatedTexts[element] = ptrText;

            // updating the text in dialog
            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_ELEMENT_TEXT, (IntPtr)element, ptrText);
        }

        private void UpdateStandardIcon(IntPtr element, TaskDialogStandardIcons icon)
        {
            if (element == Constants.TDI_FOOTER && String.IsNullOrEmpty(host.FooterText))
                return;

            // (Security)Question icon is emulated
            if (icon == TaskDialogStandardIcons.Question || icon == TaskDialogStandardIcons.SecurityQuestion)
            {
                // it will trigger UpdateCustomIcon
                if (element == Constants.TDI_MAIN)
                    host.EmulatedStandardMainIcon = icon.ToIcon();
                else
                    host.EmulatedStandardFooterIcon = icon.ToIcon();
                return;
            }

            if (element == Constants.TDI_MAIN && HasForcedFormIcon)
                host.FormIcon = icon.ToIcon();

            // Recreating if needed
            if (((element == Constants.TDI_MAIN) && (config.dwFlags & TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN) != 0) // currently a custom current main icon is used
                || ((element == Constants.TDI_FOOTER) && (config.dwFlags & TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER) != 0) // currently a custom current footer icon is used
                || element == Constants.TDI_MAIN && IsBackgroundDifferent(icon, (TaskDialogStandardIcons)config.hMainIcon) // background color changes
                )
            {
                ReallocateDialog();

                // Switching from custom icon to None does not restore the default form icon, which is fixed here
                if (icon == TaskDialogStandardIcons.None && element == Constants.TDI_MAIN && HasFormIcon)
                    User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON, Constants.TDI_MAIN, IntPtr.Zero);
                return;
            }

            // storing icon in config so it can be compared later again
            if (element == Constants.TDI_MAIN)
                config.hMainIcon = (IntPtr)icon;
            else
                config.hFooterIcon = (IntPtr)icon;

            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON, element, (IntPtr)icon);
            if (element == Constants.TDI_MAIN)
            {
                if (!HasFormIcon)
                    ClearFormIcon();
                else if (host.FormIcon != null)
                {
                    User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_BIG, host.FormIcon.Handle);
                    ResetSmallFormIcon(host.FormIcon);
                }
            }
        }

        private void UpdateCustomIcon(IntPtr element, Icon? icon)
        {
            if (element == Constants.TDI_FOOTER && String.IsNullOrEmpty(host.FooterText))
                return;

            // Recreating, if standard icon was used, or when the dialog was previously reallocated.
            // Otherwise, an AccessViolationException or ExecutionEngineException may occur when sending the TDM_UPDATE_ICON message.
            if (isEverReallocated
                || ((element == Constants.TDI_MAIN) && (config.dwFlags & TASKDIALOG_FLAGS.TDF_USE_HICON_MAIN) == 0) // standard current main icon
                || ((element == Constants.TDI_FOOTER) && (config.dwFlags & TASKDIALOG_FLAGS.TDF_USE_HICON_FOOTER) == 0)) // standard current footer icon
            {
                ReallocateDialog();
                return;
            }

            // storing icon handle in config so it can be compared later again
            IntPtr iconHandle;
            if (element == Constants.TDI_MAIN)
                config.hMainIcon = iconHandle = ResetMainIcon(host.Icon != TaskDialogStandardIcons.None ? host.EmulatedStandardMainIcon! : icon)?.Handle ?? IntPtr.Zero;
            else
                config.hFooterIcon = iconHandle = ResetFooterIcon(host.FooterIcon != TaskDialogStandardIcons.None ? host.EmulatedStandardFooterIcon! : icon)?.Handle ?? IntPtr.Zero;

            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_UPDATE_ICON, element, iconHandle);
            if (element == Constants.TDI_MAIN && HasFormIcon && host.FormIcon != null)
            {
                User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_BIG, host.FormIcon.Handle);
                ResetSmallFormIcon(host.FormIcon);
            }
        }

        private void ClearFormIcon()
        {
            User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_BIG, IntPtr.Zero);
            User32.SendMessage(dialogHandle, Constants.WM_SETICON, Constants.ICON_SMALL, IntPtr.Zero);
        }

        private void UpdateProgressBarStyle(TaskDialogProgressBarStyle style)
        {
            bool hasProgressBar = (config.dwFlags & (TASKDIALOG_FLAGS.TDF_SHOW_MARQUEE_PROGRESS_BAR | TASKDIALOG_FLAGS.TDF_SHOW_PROGRESS_BAR)) != 0;
            if (hasProgressBar ^ style != TaskDialogProgressBarStyle.None)
            {
                ReallocateDialog();
                return;
            }

            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_MARQUEE_PROGRESS_BAR, new IntPtr(style == TaskDialogProgressBarStyle.Marquee ? 1 : 0), IntPtr.Zero);
            if (style == TaskDialogProgressBarStyle.Regular)
            {
                UpdateProgressBarState(host.ProgressBarState);
                UpdateProgressBarRange(host.ProgressBarMinimum, host.ProgressBarMaximum);
                UpdateProgressBarValue(host.ProgressBarValue);
            }
            else
                UpdateProgressBarMarqueeAnimationSpeed(host.ProgressBarMarqueeAnimationSpeed);
        }

        private void UpdateProgressBarState(ProgressBarState state)
        {
            if (host.ProgressBarStyle == TaskDialogProgressBarStyle.Marquee)
                UpdateProgressBarMarqueeAnimationSpeed(host.ProgressBarMarqueeAnimationSpeed);
            else
                User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_STATE, (nint)state + 1, IntPtr.Zero);
        }

        private void UpdateProgressBarRange(int minimum, int maximum)
        {
            // actually no real 32-bit int is supported as minimum/maximum natively...
            nint range = (((short)maximum & 0xFFFF) << 16) | ((short)minimum & 0xFFFF);
            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_RANGE, IntPtr.Zero, range);
        }

        private void UpdateProgressBarValue(int value)
        {
            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_POS, new IntPtr(value), IntPtr.Zero);

            // if state is non-normal, value has to be set twice
            if (host.ProgressBarState != ProgressBarState.Normal)
                User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_POS, new IntPtr(value), IntPtr.Zero);
        }

        private void UpdateProgressBarMarqueeAnimationSpeed(int value)
        {
            if (host.ProgressBarStyle != TaskDialogProgressBarStyle.Marquee)
                return;

            IntPtr isRunning = host.ProgressBarState == ProgressBarState.Normal && value > 0 ? new IntPtr(1) : IntPtr.Zero;
            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_SET_PROGRESS_BAR_MARQUEE, isRunning, new IntPtr(value));
        }

        private void DoClose(TaskDialogResult result)
        {
            dialogState = TaskDialogStatus.Closing;
            User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_CLICK_BUTTON, (nint)result, IntPtr.Zero);
        }

        private void ReallocateDialog()
        {
            if (dialogState == TaskDialogStatus.Closed)
                return;

            // If called from callback, deferred until the end of the callback
            if (eventHandlerCount > 0)
            {
                isReallocatePending = true;
                return;
            }

            isReallocatePending = false;
            isEverReallocated = true;

            FreeUpdatedTexts();
            FreeButtons(config.pButtons, config.cButtons);
            FreeButtons(config.pRadioButtons, config.cRadioButtons);

            ResetSettings();

            int size = Marshal.SizeOf(config);
            IntPtr ptrConfig = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(config, ptrConfig, false);
                User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_NAVIGATE_PAGE, IntPtr.Zero, ptrConfig);
            }
            finally
            {
                Marshal.DestroyStructure(ptrConfig, typeof(TASKDIALOGCONFIG));
                Marshal.FreeHGlobal(ptrConfig);
            }
        }

        private void FreeUpdatedTexts()
        {
            if (updatedTexts == null)
                return;

            // freeing pointers of updated texts
            foreach (IntPtr ptrText in updatedTexts.Values)
            {
                if (ptrText != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptrText);
            }

            updatedTexts = null;
        }

        private void Dispose(bool disposing)
        {
            nativeHandler?.DestroyHandle();

            // Happens only when TaskDialog.Dispose was called while showing: forcing close and waiting for being closed
            if (dialogState != TaskDialogStatus.Closed)
            {
                if (isForcedClosing)
                    return;

                isForcedClosing = true;
                DoClose(TaskDialogResult.Close);

                // waiting for being closed
                while (dialogState != TaskDialogStatus.Closed)
                    Thread.Sleep(10);
            }

            // freeing unmanaged resources
            FreeUpdatedTexts();

            if (disposing)
            {
                mainIcon?.Dispose();
                footerIcon?.Dispose();
                smallFormIcon?.Dispose();
            }
        }

        #endregion

        #region Explicitly Implemented Interface Methods

        TaskDialogResult ITaskDialog.Execute(TaskDialog taskDialog, IntPtr owner, out int selectedButtonIndex, out int selectedRadioButtonIndex, out bool checkBoxChecked)
        {
            host = taskDialog;
            ownerHandle = owner;
            ResetSettings();

            try
            {
                // Showing the dialog
                int hResult;

                // Using activation context forces to load Comctl32.dll V6.
                // It works only when V5 has not been loaded yet, otherwise, an EntryPointNotFound exception occurs.
                // IsAvailable checks whether V6 context is accessible, so when using via the TaskDialog class, this using is not really necessary
                using (new ThemingActivationContext(true))
                    hResult = Comctl32.TaskDialogIndirect(ref config, out selectedButtonIndex, out selectedRadioButtonIndex, out checkBoxChecked);

                if (hResult < 0)
                    throw Marshal.GetExceptionForHR(hResult) ?? new Win32Exception(hResult);

                TaskDialogResult result = Enum<TaskDialogResult>.IsDefined(selectedButtonIndex) ? (TaskDialogResult)selectedButtonIndex : TaskDialogResult.Custom;

                // Adjusting out parameters
                if (selectedButtonIndex >= firstButtonId)
                    selectedButtonIndex -= firstButtonId;
                else
                    selectedButtonIndex = -1;

                if (selectedRadioButtonIndex >= firstRadioButtonId)
                    selectedRadioButtonIndex -= firstRadioButtonId;
                else
                    selectedRadioButtonIndex = -1;

                return result;
            }
            finally
            {
                dialogState = TaskDialogStatus.Closed;
                FreeButtons(config.pButtons, config.cButtons);
                FreeButtons(config.pRadioButtons, config.cRadioButtons);

                config = default(TASKDIALOGCONFIG);
            }
        }

        void ITaskDialog.Close(TaskDialogResult result)
        {
            // input is validated when calling this internal interface implementation
            DoClose(result);
        }

        void ITaskDialog.PropertyChanged(string propName)
        {
            if (dialogState == TaskDialogStatus.Initializing || dialogState == TaskDialogStatus.Closed)
                throw new InvalidOperationException("Changing property in invalid state.");

            switch (propName)
            {
                case TaskDialog.PropertyMessage:
                    if (String.IsNullOrEmpty((host.Message)) || String.IsNullOrEmpty(config.pszContent))
                        ReallocateDialog();
                    else
                        UpdateText(TASKDIALOG_ELEMENTS.TDE_CONTENT, config.pszContent = host.Message);
                    return;

                case TaskDialog.PropertyMainInstruction:
                    if (String.IsNullOrEmpty((host.MainInstruction)) || String.IsNullOrEmpty(config.pszMainInstruction))
                        ReallocateDialog();
                    else
                        UpdateText(TASKDIALOG_ELEMENTS.TDE_MAIN_INSTRUCTION, host.MainInstruction);
                    return;

                case TaskDialog.PropertyFooterText:
                    if (String.IsNullOrEmpty((host.FooterText)) || String.IsNullOrEmpty(config.pszFooter))
                        ReallocateDialog();
                    else
                        UpdateText(TASKDIALOG_ELEMENTS.TDE_FOOTER, config.pszFooter = host.FooterText);
                    return;

                case TaskDialog.PropertyDetailsText:
                    if (String.IsNullOrEmpty((host.DetailsText)) || String.IsNullOrEmpty(config.pszExpandedInformation))
                        ReallocateDialog();
                    else
                        UpdateText(TASKDIALOG_ELEMENTS.TDE_EXPANDED_INFORMATION, config.pszExpandedInformation = host.DetailsText);
                    return;

                case TaskDialog.PropertyCaption:
                case TaskDialog.PropertyCheckBoxText:
                case TaskDialog.PropertyShowDetailsText:
                case TaskDialog.PropertyHideDetailsText:
                case TaskDialog.PropertyStandardButtons:
                case TaskDialog.PropertyDefaultStandardButton:
                case TaskDialog.PropertyWidth:
                case TaskDialog.PropertyOptions:
                    ReallocateDialog();
                    return;

                case TaskDialog.PropertyCheckBoxChecked:
                    User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_CLICK_VERIFICATION, new IntPtr(host.CheckBoxChecked ? 1 : 0), IntPtr.Zero);
                    return;

                case TaskDialog.PropertyIcon:
                    UpdateStandardIcon(Constants.TDI_MAIN, host.Icon);
                    return;

                case TaskDialog.PropertyCustomIcon:
                    UpdateCustomIcon(Constants.TDI_MAIN, host.CustomIcon);
                    return;

                case TaskDialog.PropertyFooterIcon:
                    UpdateStandardIcon(Constants.TDI_FOOTER, host.FooterIcon);
                    return;

                case TaskDialog.PropertyCustomFooterIcon:
                    UpdateCustomIcon(Constants.TDI_FOOTER, host.CustomFooterIcon);
                    return;

                case TaskDialog.PropertyProgressBarStyle:
                    UpdateProgressBarStyle(host.ProgressBarStyle);
                    return;

                case TaskDialog.PropertyProgressBarState:
                    UpdateProgressBarState(host.ProgressBarState);
                    return;

                case TaskDialog.PropertyProgressBarMinimum:
                case TaskDialog.PropertyProgressBarMaximum:
                    UpdateProgressBarRange(host.ProgressBarMinimum, host.ProgressBarMaximum);
                    return;

                case TaskDialog.PropertyProgressBarValue:
                    UpdateProgressBarValue(host.ProgressBarValue);
                    return;

                case TaskDialog.PropertyProgressBarMarqueeAnimationSpeed:
                    UpdateProgressBarMarqueeAnimationSpeed(host.ProgressBarMarqueeAnimationSpeed);
                    return;

                default:
                    throw new NotSupportedException("Not supported property: " + propName);
            }
        }

        void ITaskDialog.ControlPropertyChanged(TaskDialogControl control, string propName)
        {
            if (dialogState == TaskDialogStatus.Initializing || dialogState == TaskDialogStatus.Closed)
            {
                throw new InvalidOperationException("Changing property in invalid state.");
            }

            if (control is TaskDialogButton button)
            {
                switch (propName)
                {
                    case TaskDialogButtonBase.PropertyText:
                        ReallocateDialog();
                        return;

                    case TaskDialogButtonBase.PropertyDescription:
                        // updating description only when it has effect
                        if ((config.dwFlags & (TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS | TASKDIALOG_FLAGS.TDF_USE_COMMAND_LINKS_NO_ICON)) != 0)
                            ReallocateDialog();

                        return;

                    case TaskDialogButtonBase.PropertyEnabled:
                        UpdateButtonEnabled(button);
                        return;

                    case TaskDialogButton.PropertyIsElevated:
                        UpdateElevatedStatus(button);
                        return;

                    case TaskDialogButton.PropertyIsDefault:
                        // updating only if the change has effect
                        TaskDialogButton? realDefault = host.Buttons.FirstOrDefault(b => b.IsDefault);

                        if (button.IsDefault)
                        {
                            // IsDefault set: has effect only if there are no defaults before the button
                            if (realDefault == button)
                                ReallocateDialog();
                        }
                        else
                        {
                            // default flag cleared: has only effect if... 
                            if ((realDefault != null && realDefault.Id > button.Id) // real default has larger index
                                || (realDefault == null)) // or, when there is no default custom button anymore (now either a standard button or the first custom button will be the default)
                            {
                                ReallocateDialog();
                            }
                        }

                        return;

                    case TaskDialogButton.PropertyCustomIcon:
                        // not supported: ignoring
                        return;

                    default:
                        throw new NotSupportedException("Not supported button property: " + propName);
                }
            }

            if (control is TaskDialogRadioButton radioButton)
            {
                switch (propName)
                {
                    case TaskDialogButtonBase.PropertyText:
                        ReallocateDialog();
                        return;

                    case TaskDialogButtonBase.PropertyDescription:
                        // not supported: ignoring
                        return;

                    case TaskDialogButtonBase.PropertyEnabled:
                        UpdateRadioButtonEnabled(radioButton);
                        return;

                    case TaskDialogRadioButton.PropertyChecked:
                        if (isCheckedChanging)
                            return;

                        isCheckedChanging = true;
                        try
                        {
                            // unchecking: reallocating so "none checked" status can be reset
                            if (!radioButton.Checked)
                            {
                                ReallocateDialog();
                                return;
                            }

                            // checking: first unchecking others (this may raise unchecking events, which are ignored)
                            foreach (TaskDialogRadioButton rb in host.RadioButtons)
                            {
                                if (rb != radioButton && rb.Checked)
                                    rb.Checked = false;
                            }

                            // if not raised from callback (so not the user actually clicked), but set by Checked property, then checking the actual radio button
                            if (!isRadioButtonClicked)
                                User32.SendMessage(dialogHandle, (int)TASKDIALOG_MESSAGES.TDM_CLICK_RADIO_BUTTON, new IntPtr(radioButton.Id), IntPtr.Zero);
                            return;

                        }
                        finally
                        {
                            isCheckedChanging = false;
                        }

                    default:
                        throw new NotSupportedException("Not supported radio button property: " + propName);
                }
            }

            throw new InvalidOperationException("Invalid control type");
        }

        void ITaskDialog.CustomButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index) => ReallocateDialog();
        void ITaskDialog.RadioButtonsChanged(TaskDialogControlCollectionChangeTypes changeType, int index) => ReallocateDialog();
        void ITaskDialog.TimerChanged(bool enabled) => ReallocateDialog();

        #endregion

        #endregion

        #endregion
    }
}
