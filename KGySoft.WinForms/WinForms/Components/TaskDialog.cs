#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TaskDialog.cs
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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using KGySoft.CoreLibraries;
using KGySoft.Drawing;
using KGySoft.WinForms.Forms;

#endregion

#region Suppressions

#if !NET5_0_OR_GREATER
#pragma warning disable CS1574 // XML comment has cref attribute that could not be resolved - The documentation references types that are not available on all platforms
#endif

#endregion

namespace KGySoft.WinForms.Components
{
    #region Usings

    using Resources = Properties.Resources;

    #endregion

    /// <summary>
    /// Represents a task dialog window that is able to display regular buttons, Vista-like command links,
    /// radio buttons and a progress bar. Can work in compatibility mode so the dialog can be used even with Windows XP or when visual styles are not available.
    /// </summary>
    /// <remarks>
    /// <note type="warning">.NET 5 also introduced task dialogs, so when targeting .NET 5 or later, referencing <see cref="TaskDialog"/>, <see cref="TaskDialogControl"/>,
    /// <see cref="TaskDialogButton"/> and <see cref="TaskDialogRadioButton"/> button classes may require to use fully qualified names or aliases
    /// like <c>using TaskDialog = KGySoft.WinForms.Components.TaskDialog;</c> to avoid ambiguity with the recently added WinForms classes.
    /// Please also note that unlike this class, <see cref="System.Windows.Forms.TaskDialog">System.Windows.Forms.TaskDialog</see> cannot be used on Windows XP, on Linux/Mono, or when visual styles are not enabled.</note>
    /// <note>But you might want to choose the KGy SOFT version even when running on Windows Vista or later with visual styles enabled, because it offers additional features
    /// like custom images on the buttons and command links. If you set <see cref="ForceCompatibilityMode"/> to <see langword="true"/>, then always the alternative implementation
    /// is used, allowing some small improvements to the native version such as tool tips for regular buttons and radio buttons, more detailed info when copying the
    /// content to the clipboard by <c>Ctrl+C</c>, fixing possible color issues in high contrast mode, etc.</note>
    /// </remarks>
    /// <example>TODO</example>
    public sealed class TaskDialog : IWin32Window, IDisposable
    {
        #region Constants

        #region Internal Constants

        internal const int NativeOptionsMask = 0xFFFF;

        #endregion

        #region Private Constants

        private const int isDisposed = 1;
        private const int forceCompatibilityMode = isDisposed << 1;
        private const int isEmulatedStandardMainIcon = forceCompatibilityMode << 1;
        private const int isEmulatedStandardFooterIcon = isEmulatedStandardMainIcon << 1;
        private const int checkBoxChecked = isEmulatedStandardFooterIcon << 1;

        #endregion

        #endregion

        #region Fields

        #region Static Fields

        private static Icon? defaultIcon;
        private static bool? hasImageRes;

        #endregion

        #region Instance Fields

        private BitVector32 flags;
        private ITaskDialog? dialogInstance;
        private string? message;
        private string? mainInstruction;
        private string? caption;
        private string? footerText;
        private string? checkBoxText;
        private string? detailsText;
        private TaskDialogOptions options;
        private string? showDetailsText;
        private string? hideDetailsText;
        private TaskDialogStandardIcon icon;
        private Icon? customIcon;
        private TaskDialogStandardIcon footerIcon;
        private Icon? customFooterIcon;
        private Icon? formIcon;
        private TaskDialogStandardButtons standardButtons;
        private TaskDialogStandardButton defaultStandardButton;
        private TaskDialogControlCollection<TaskDialogButton> buttons;
        private TaskDialogControlCollection<TaskDialogRadioButton> radioButtons;
        private TaskDialogProgressBarStyle progressBarStyle;
        private ProgressBarState progressBarState;
        private int progressBarMinimum;
        private int progressBarMaximum = 100;
        private int progressBarValue;
        private int progressBarMarqueeAnimationSpeed = 20;
        private int selectedButtonIndex = -1;
        private int selectedRadioButtonIndex = -1;
        private TaskDialogResult dialogResult;
        private int width;

        private EventHandler? created;
        private EventHandler<TaskDialogTickEventArgs>? tick;
        private EventHandler<CancelEventArgs>? closing;
        private EventHandler? closed;
        private EventHandler<HyperlinkClickedEventArgs>? hyperlinkClicked;
        private EventHandler? checkBoxCheckedChanged;
        private EventHandler? helpRequested;
        private EventHandler<TaskDialogDetailsVisibleChangedEventArgs>? detailsVisibleChanged;

        #endregion

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="TaskDialog"/> has been created and is about to be displayed.
        /// </summary>
        public event EventHandler Created
        {
            add
            {
                CheckDisposed();
                created += value;
            }
            remove => created -= value;
        }

        /// <summary>
        /// Occurs in approximately every 200 milliseconds.
        /// Can be used to update progress information.
        /// </summary>
        public event EventHandler<TaskDialogTickEventArgs> Tick
        {
            add
            {
                CheckDisposed();
                if (tick == null)
                {
                    tick = value;
                    if (IsDialogShowing)
                        dialogInstance!.TimerChanged(true);
                    return;
                }

                tick += value;
            }
            remove
            {
                if (tick == null)
                    return;

                tick -= value;
                if (tick == null && IsDialogShowing)
                    dialogInstance!.TimerChanged(false);
            }
        }

        /// <summary>
        /// Occurs when <see cref="TaskDialogOptions.HyperlinksEnabled"/> is set in <see cref="Options"/>,
        /// and the user clicks a hyperlink. If this event is not subscribed or <see cref="HandledEventArgs.Handled"/> is set to <see langword="false"/> in <see cref="HyperlinkClickedEventArgs"/>,
        /// then the system tries to resolve the hyperlink.
        /// </summary>
        /// <remarks>
        /// <note>Resolving the link by the operating system might be blocked so usually the best way is to subscribe this event.</note>
        /// </remarks>
        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked
        {
            add
            {
                CheckDisposed();
                hyperlinkClicked += value;
            }
            remove => hyperlinkClicked -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="CheckBoxChecked"/> property changes.
        /// </summary>
        public event EventHandler CheckBoxCheckedChanged
        {
            add
            {
                CheckDisposed();
                checkBoxCheckedChanged += value;
            }
            remove => checkBoxCheckedChanged -= value;
        }

        /// <summary>
        /// Occurs when the user presses F1 on the <see cref="TaskDialog"/>.
        /// </summary>
        public event EventHandler HelpRequested
        {
            add
            {
                CheckDisposed();
                helpRequested += value;
            }
            remove => helpRequested -= value;
        }

        /// <summary>
        /// Occurs when the visibility of the details text (the state of the expando button) changes.
        /// </summary>
        public event EventHandler<TaskDialogDetailsVisibleChangedEventArgs> DetailsVisibleChanged
        {
            add
            {
                CheckDisposed();
                detailsVisibleChanged += value;
            }
            remove => detailsVisibleChanged -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="TaskDialog"/> about to be closed.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing
        {
            add
            {
                CheckDisposed();
                closing += value;
            }
            remove => closing -= value;
        }

        /// <summary>
        /// Occurs when the <see cref="TaskDialog"/> has been closed.
        /// </summary>
        public event EventHandler Closed
        {
            add
            {
                CheckDisposed();
                closed += value;
            }
            remove => closed -= value;
        }

        #endregion

        #region Properties

        #region Static Properties
        
        #region Internal Properties

        internal static Icon DefaultIcon => defaultIcon ??= OSHelper.IsWindowsVistaOrLater && HasImageRes
            ? Icons.FromFile("imageres", 116)
            : Resources.TaskDialogIcon;

        #endregion

        #region Private Properties

        private static bool HasImageRes => hasImageRes ??= File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "imageres.dll"));

        #endregion

        #endregion

        #region Instance Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string? Message
        {
            get => message;
            set
            {
                if (message == value)
                    return;

                CheckCanChangeProperty();
                message = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the main instruction text.
        /// </summary>
        public string? MainInstruction
        {
            get => mainInstruction;
            set
            {
                if (mainInstruction == value)
                    return;

                CheckCanChangeProperty();
                mainInstruction = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the caption of the task dialog. If caption is <see langword="null"/>, the filename of the executable program is displayed.
        /// </summary>
        public string? Caption
        {
            get => caption;
            set
            {
                if (caption == value)
                    return;

                CheckCanChangeProperty();
                caption = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the footer text.
        /// </summary>
        public string? FooterText
        {
            get => footerText;
            set
            {
                if (footerText == value)
                    return;

                CheckCanChangeProperty();
                footerText = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the verification checkbox of the <see cref="TaskDialog"/>.
        /// The checkbox will be visible when this property is not empty when the dialog is shown.
        /// </summary>
        public string? CheckBoxText
        {
            get => checkBoxText;
            set
            {
                if (checkBoxText == value)
                    return;

                CheckCanChangeProperty();
                checkBoxText = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the verification check box is checked. This property is ignored when <see cref="CheckBoxText"/> is empty.
        /// <br/>Default value: <see langword="false"/>.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxText"/>
        public bool CheckBoxChecked
        {
            get => flags[checkBoxChecked];
            set
            {
                if (flags[checkBoxChecked] == value)
                    return;

                CheckCanChangeProperty();
                flags[checkBoxChecked] = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();

                OnCheckBoxCheckedChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets the details text. The expando button with <see cref="ShowDetailsText"/> or <see cref="HideDetailsText"/>
        /// will be visible only when this property is not empty.
        /// </summary>
        public string? DetailsText
        {
            get => detailsText;
            set
            {
                if (detailsText == value)
                    return;

                CheckCanChangeProperty();
                detailsText = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the options of the <see cref="TaskDialog"/>.
        /// </summary>
        public TaskDialogOptions Options
        {
            get => options;
            set
            {
                if (options == value)
                    return;

                CheckCanChangeProperty();
                if (!value.AllFlagsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.FlagsEnumOutOfRange(value));

                options = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the expando button is collapsed state. The expando button is visible only when <see cref="DetailsText"/> is not empty.
        /// </summary>
        /// <remarks>
        /// When both <see cref="ShowDetailsText"/> and <see cref="HideDetailsText"/> properties are empty, a default text will be displayed. When one
        /// of these properties is empty while the other is set, the non-empty value will be displayed in both expanded and collapsed states.
        /// </remarks>
        public string? ShowDetailsText
        {
            get => showDetailsText;
            set
            {
                if (showDetailsText == value)
                    return;

                CheckCanChangeProperty();
                showDetailsText = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the text of the expando button in expanded state. The expando button is visible only when <see cref="DetailsText"/> is not empty.
        /// </summary>
        /// <remarks>
        /// When both <see cref="ShowDetailsText"/> and <see cref="HideDetailsText"/> properties are empty, a default text will be displayed. When one
        /// of these properties is empty while the other is set, the non-empty value will be displayed in both expanded and collapsed states.
        /// </remarks>
        public string? HideDetailsText
        {
            get => hideDetailsText;
            set
            {
                if (hideDetailsText == value)
                    return;

                CheckCanChangeProperty();
                hideDetailsText = value;

                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets one of the standard icons as the main icon for this <see cref="TaskDialog"/>.
        /// Setting this property clears <see cref="CustomIcon"/> and vice versa.
        /// <br/>Default value: <see cref="TaskDialogStandardIcon.None"/>.
        /// </summary>
        public TaskDialogStandardIcon Icon
        {
            get => icon;
            set
            {
                if (icon == value && customIcon == null)
                    return;

                CheckCanChangeProperty();

                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                flags[isEmulatedStandardMainIcon] = false;
                customIcon = null;
                formIcon = null;
                icon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a custom <see cref="System.Drawing.Icon"/> as the main icon for this <see cref="TaskDialog"/>.
        /// Setting this property clears <see cref="Icon"/> and vice versa.
        /// </summary>
        public Icon? CustomIcon
        {
            get => flags[isEmulatedStandardMainIcon] ? null : customIcon;
            set
            {
                if (customIcon == value)
                    return;

                CheckCanChangeProperty();
                flags[isEmulatedStandardMainIcon] = false;
                icon = TaskDialogStandardIcon.None;
                customIcon = value;
                formIcon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets one of the standard icons as the footer icon for this <see cref="TaskDialog"/>.
        /// Setting this property clears <see cref="CustomFooterIcon"/> and vice versa.
        /// <br/>Default value: <see cref="TaskDialogStandardIcon.None"/>.
        /// </summary>
        public TaskDialogStandardIcon FooterIcon
        {
            get => footerIcon;
            set
            {
                if (footerIcon == value && customFooterIcon == null)
                    return;

                CheckCanChangeProperty();

                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                flags[isEmulatedStandardFooterIcon] = false;
                customFooterIcon = null;
                footerIcon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets a custom <see cref="System.Drawing.Icon"/> as the footer icon for this <see cref="TaskDialog"/>.
        /// Setting this property clears <see cref="FooterIcon"/> and vice versa.
        /// </summary>
        public Icon? CustomFooterIcon
        {
            get => flags[isEmulatedStandardFooterIcon] ? null : customFooterIcon;
            set
            {
                if (customFooterIcon == value)
                    return;

                CheckCanChangeProperty();
                flags[isEmulatedStandardFooterIcon] = false;
                footerIcon = TaskDialogStandardIcon.None;
                customFooterIcon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the standard buttons of this <see cref="TaskDialog"/>.
        /// If neither standard nor custom buttons (see <see cref="Buttons"/>) are specified, the task dialog will have an OK button by default.
        /// </summary>
        /// <remarks>
        /// The standard button texts are localized by using the native Windows resources by default.
        /// To localize them using the dynamic resources of this library, set <see cref="TaskDialogOptions.TranslateStandardButtons"/> in <see cref="Options"/>.
        /// </remarks>
        public TaskDialogStandardButtons StandardButtons
        {
            get => standardButtons;
            set
            {
                if (standardButtons == value)
                    return;

                CheckCanChangeProperty();
                if (!value.AllFlagsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.FlagsEnumOutOfRange(value));

                standardButtons = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the default standard button. When <see cref="Buttons"/> are not empty and one of them is specified as default button
        /// by <see cref="TaskDialogButton.IsDefault"/>, then this property is ignored.
        /// If neither standard nor custom buttons are specified as default, the first button will be the default one.
        /// </summary>
        public TaskDialogStandardButton DefaultStandardButton
        {
            get => defaultStandardButton;
            set
            {
                if (defaultStandardButton == value)
                    return;

                CheckCanChangeProperty();
                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                defaultStandardButton = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the width of the <see cref="TaskDialog"/> in dialog units (1 DLU is about 2 pixels, but may vary based on the default dialog font).
        /// The dialog is always at least 100 DLU wide. Zero value means that size is auto-calculated.
        /// Re-assigning zero value adjusts the automatic width of the dialog on demand.
        /// <br/>Default value: 0.
        /// </summary>
        public int Width
        {
            get => width;
            set
            {
                if (width == value && value != 0)
                    return;

                CheckCanChangeProperty();
                if (width < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

                width = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets a <see cref="TaskDialogControlCollection{T}"/> containing <see cref="TaskDialogButton"/> elements,
        /// representing the custom buttons of this <see cref="TaskDialog"/>.
        /// </summary>
        public TaskDialogControlCollection<TaskDialogButton> Buttons
        {
            get
            {
                CheckDisposed();
                return buttons;
            }
        }

        /// <summary>
        /// Gets a <see cref="TaskDialogControlCollection{T}"/> containing <see cref="TaskDialogRadioButton"/> elements,
        /// representing the radio buttons of this <see cref="TaskDialog"/>.
        /// </summary>
        public TaskDialogControlCollection<TaskDialogRadioButton> RadioButtons
        {
            get
            {
                CheckDisposed();
                return radioButtons;
            }
        }

        /// <summary>
        /// Gets or sets the style of the progress bar.
        /// <br/>Default value: <see cref="TaskDialogProgressBarStyle.None"/>.
        /// </summary>
        public TaskDialogProgressBarStyle ProgressBarStyle
        {
            get => progressBarStyle;
            set
            {
                if (progressBarStyle == value)
                    return;

                CheckCanChangeProperty();

                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                progressBarStyle = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the state of the progress bar.
        /// <br/>Default value: <see cref="ProgressBarState.Normal"/>.
        /// </summary>
        public ProgressBarState ProgressBarState
        {
            get => progressBarState;
            set
            {
                if (progressBarState == value)
                    return;

                CheckCanChangeProperty();

                if (!value.IsDefined())
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                progressBarState = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// <br/>Default value: 0.
        /// </summary>
        public int ProgressBarMinimum
        {
            get => progressBarMinimum;
            set
            {
                if (progressBarMinimum == value)
                    return;

                CheckCanChangeProperty();

                if (value < 0 || value >= progressBarMaximum)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentOutOfRange);
                if (progressBarValue < value)
                    ProgressBarValue = value;

                progressBarMinimum = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// <br/>Default value: 100.
        /// </summary>
        public int ProgressBarMaximum
        {
            get => progressBarMaximum;
            set
            {
                if (progressBarMaximum == value)
                    return;

                CheckCanChangeProperty();

                if (value < progressBarMinimum)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentOutOfRange);

                if (progressBarValue > value)
                    ProgressBarValue = value;

                progressBarMaximum = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// <br/>Default value: 0.
        /// </summary>
        public int ProgressBarValue
        {
            get => progressBarValue;
            set
            {
                if (progressBarValue == value)
                    return;

                CheckCanChangeProperty();

                if (value < progressBarMinimum || value > progressBarMaximum)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentOutOfRange);

                progressBarValue = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the current animation speed value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Marquee"/>.
        /// Zero value stops the animation.
        /// <br/>Default value: 20.
        /// </summary>
        public int ProgressBarMarqueeAnimationSpeed
        {
            get => progressBarMarqueeAnimationSpeed;
            set
            {
                if (progressBarMarqueeAnimationSpeed == value)
                    return;

                CheckCanChangeProperty();

                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), PublicResources.ArgumentMustBeGreaterThanOrEqualTo(0));

                progressBarMarqueeAnimationSpeed = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="TaskDialog"/> is to be forced to operate in compatibility mode
        /// even if the current operating system supports native task dialogs.
        /// </summary>
        /// <remarks>
        /// <para>Compatibility mode is automatically used in the following cases:
        /// <list type="bullet">
        /// <item>The operating system is not Windows Vista or later.</item>
        /// <item><see cref="Application.EnableVisualStyles">Application.EnableVisualStyles</see> was not called on launching the application.</item>
        /// <item><see cref="TaskDialogOptions.TranslateStandardButtons"/> is set in <see cref="Options"/>.</item>
        /// <item><see cref="Icon"/> is <see cref="TaskDialogStandardIcon.SecurityQuestion"/> (so the special header colors can be applied).</item>
        /// <item>There is at least one button in <see cref="Buttons"/> that has a custom icon (<see cref="TaskDialogButton.CustomIcon"/> is set).</item>
        /// </list></para>
        /// <para>When this property is set to <see langword="true"/>, the following improvements can be observed:
        /// <list type="bullet">
        /// <item>If the custom buttons have <see cref="TaskDialogButtonBase.Description"/>, and buttons are displayed as standard buttons
        /// rather than command links, then the descriptions are displayed as tool tips when the buttons are hovered by the mouse.</item>
        /// <item>The <see cref="TaskDialogButtonBase.Description"/> of radio buttons are displayed as tool tips.</item>
        /// <item><c>Ctrl+C</c> copies more information than the standard version (e.g. includes the texts of the radio buttons, indicates elevated icons, etc.)</item>
        /// <item>In high contrast mode it is ensured that the content remains visible if Control and Window colors are the inverse of each other.</item>
        /// <item>Better support of Right-to-Left mode.</item>
        /// </list></para>
        /// </remarks>
        public bool ForceCompatibilityMode
        {
            get => flags[forceCompatibilityMode];
            set
            {
                if (dialogInstance != null)
                    throw new InvalidOperationException(Res.TaskDialogPropertyChange(nameof(ForceCompatibilityMode)));

                flags[forceCompatibilityMode] = value;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="TaskDialog"/> is displayed in compatibility mode.
        /// When the dialog is not displayed, returns <see langword="false"/>.
        /// </summary>
        public bool IsInCompatibilityMode => IsDialogShowing && dialogInstance is TaskDialogForm;

        /// <summary>
        /// After the dialog is closed, gets the index of the clicked custom button defined in the <see cref="Buttons"/> collection.
        /// If the <see cref="TaskDialog"/> has been closed by a standard button or is not closed yet, this property returns -1.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public int SelectedButtonIndex => selectedButtonIndex;

        /// <summary>
        /// After the dialog is closed, gets the index of the selected radio button defined in the <see cref="RadioButtons"/> collection.
        /// If no radio button was selected, this property returns -1.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public int SelectedRadioButtonIndex => selectedRadioButtonIndex;

        /// <summary>
        /// <para>When read, gets the last result of a closed <see cref="TaskDialog"/> (if the dialog was closed by one of the <see cref="StandardButtons"/>).</para>
        /// <para>When set and there is an opened <see cref="TaskDialog"/>, forces to close the dialog with the specified result.</para>
        /// </summary>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult DialogResult
        {
            get => dialogResult;
            set
            {
                CheckDisposed();
                if (value != dialogResult)
                {
                    if (!value.IsDefined())
                        throw new ArgumentOutOfRangeException(nameof(value), PublicResources.EnumOutOfRange(value));

                    if (IsDialogShowing)
                        dialogInstance!.Close(value);
                    else
                        dialogResult = value;
                }
            }
        }

        /// <summary>
        /// Gets the native Win32 handle of the dialog.
        /// </summary>
        public IntPtr Handle { get; internal set; }

        #endregion

        #region Internal Properties

        internal bool IsDialogShowing
            => (dialogInstance != null) && (dialogInstance.ShowState == TaskDialogStatus.Showing || dialogInstance.ShowState == TaskDialogStatus.Closing);

        internal bool IsTickAssigned => tick != null;
        internal bool IsHelpRequestedAssigned => helpRequested != null;
        internal bool IsNativeDialog => dialogInstance is NativeTaskDialog;

        /// <summary>
        /// When an <see cref="ITaskDialog"/> implementation does not support one of the <see cref="TaskDialogStandardIcon"/>, it can set this property
        /// to handle a standard icon as a custom one.
        /// </summary>
        internal Icon? EmulatedStandardMainIcon
        {
            get => flags[isEmulatedStandardMainIcon] ? customIcon : null;
            set
            {
                flags[isEmulatedStandardMainIcon] = true;
                customIcon = value;
                formIcon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged(nameof(CustomIcon));
            }
        }

        /// <summary>
        /// When an <see cref="ITaskDialog"/> implementation does not support one of the <see cref="TaskDialogStandardIcon"/>, it can set this property
        /// to handle a standard icon as a custom one.
        /// </summary>
        internal Icon? EmulatedStandardFooterIcon
        {
            get => flags[isEmulatedStandardFooterIcon] ? customFooterIcon : null;
            set
            {
                flags[isEmulatedStandardFooterIcon] = true;
                customFooterIcon = value;
                if (IsDialogShowing)
                    dialogInstance!.PropertyChanged(nameof(CustomFooterIcon));
            }
        }

        /// <summary>
        /// Gets the form icon. It is set also by setting <see cref="CustomIcon"/> or <see cref="EmulatedStandardMainIcon"/>.
        /// </summary>
        internal Icon? FormIcon
        {
            get => formIcon;
            set => formIcon = value;
        }

        #endregion

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        /// <summary>
        /// Creates a new instance of <see cref="TaskDialog"/> class.
        /// </summary>
        public TaskDialog()
        {
            buttons = new TaskDialogControlCollection<TaskDialogButton>(this);
            radioButtons = new TaskDialogControlCollection<TaskDialogRadioButton>(this);
        }

        #endregion

        #region Destructor

        /// <inheritdoc />
        ~TaskDialog() => Dispose(false);

        #endregion

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If the return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// The result is stored also in the <see cref="DialogResult"/> property.
        /// </returns>
        /// <param name="owner">The handle of the owner window. If <see cref="IntPtr.Zero">IntPtr.Zero</see>, the dialog is shown as a non-modal window.</param>
        /// <param name="customButtonIndex">When this method returns, contains the index of the clicked custom button specified in <see cref="Buttons"/> collection if the button closed the dialog.
        /// <para>Its value is stored also in the <see cref="SelectedButtonIndex"/> property.</para>
        /// </param>
        /// <param name="radioButtonIndex">When this method returns, contains the index of the selected radio button specified in <see cref="RadioButtons"/> collection.
        /// <para>Its value is stored also in the <see cref="SelectedRadioButtonIndex"/> property.</para>
        /// </param>
        /// <param name="verificationTextChecked">When this method returns, contains whether the verification checkbox was checked when the dialog was closed.
        /// <para>Its value is stored also in the <see cref="CheckBoxChecked"/> property.</para>
        /// </param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IntPtr owner, out int customButtonIndex, out int radioButtonIndex, out bool verificationTextChecked)
            => ShowInternal(owner, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If the return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// The result is stored also in the <see cref="DialogResult"/> property.
        /// </returns>
        /// <param name="owner">The owner of the dialog. If <see langword="null"/>, the dialog is shown as a non-modal window.</param>
        /// <param name="customButtonIndex">When this method returns, contains the index of the clicked custom button specified in <see cref="Buttons"/> collection if the button closed the dialog.
        /// <para>Its value is stored also in the <see cref="SelectedButtonIndex"/> property.</para>
        /// </param>
        /// <param name="radioButtonIndex">Returns the index of the selected radio button specified in <see cref="RadioButtons"/> collection.
        /// <para>Its value is stored also in the <see cref="SelectedRadioButtonIndex"/> property.</para>
        /// </param>
        /// <param name="verificationTextChecked">Returns whether verification checkbox was checked when the dialog was closed.
        /// <para>Its value is stored also in the <see cref="CheckBoxChecked"/> property.</para>
        /// </param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IWin32Window? owner, out int customButtonIndex, out int radioButtonIndex, out bool verificationTextChecked)
            => Show(owner?.Handle ?? IntPtr.Zero, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// The result is stored also in the <see cref="DialogResult"/> property.
        /// </returns>
        /// <param name="owner">Handle of the parent window. If <see cref="IntPtr.Zero">IntPtr.Zero</see>, the dialog is shown as a non-modal window.</param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IntPtr owner) => Show(owner, out int _, out int _, out bool _);

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// The result is stored also in the <see cref="DialogResult"/> property.
        /// </returns>
        /// <param name="owner">Handle of the parent window. If <see langword="null"/>, the dialog is shown as a non-modal window.</param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IWin32Window? owner) => Show(owner?.Handle ?? IntPtr.Zero, out int _, out int _, out bool _);

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> as a non-modal window, using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// The result is stored also in the <see cref="DialogResult"/> property.
        /// </returns>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show() => Show(IntPtr.Zero, out int _, out int _, out bool _);

        /// <summary>
        /// Forces to close the <see cref="TaskDialog"/>. This causes that <see cref="DialogResult"/> will be <see cref="TaskDialogResult.Close"/>
        /// even if there was no Close button on the dialog.
        /// </summary>
        public void Close() => DialogResult = TaskDialogResult.Close;

        /// <inheritdoc />
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Internal Methods

        internal void ControlPropertyChanged(TaskDialogControl control, string propName)
        {
            CheckCanChangeProperty();
            dialogInstance?.ControlPropertyChanged(control, propName);
        }

        internal void ControlCollectionChanged(IList collection, TaskDialogControlCollectionChangeTypes changeType, int index)
        {
            CheckCanChangeProperty();

            if (dialogInstance != null)
            {
                if (ReferenceEquals(collection, buttons))
                    dialogInstance.CustomButtonsChanged(changeType, index);
                else if (ReferenceEquals(collection, radioButtons))
                    dialogInstance.RadioButtonsChanged(changeType, index);
            }
        }

        internal void CheckCanChangeProperty()
        {
            CheckDisposed();

            if (dialogInstance == null || dialogInstance.ShowState != TaskDialogStatus.Initializing)
                return;

            throw new InvalidOperationException(Res.TaskDialogInitializing);
        }

        internal void OnCreated() => created?.Invoke(this, EventArgs.Empty);
        internal void OnTick(TaskDialogTickEventArgs e) => tick?.Invoke(this, e);

        internal void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
        {
            if (hyperlinkClicked is { } handler)
                handler.Invoke(this, e);
            else
                e.Handled = false;
        }

        /// <summary>
        /// This is a callback notification of the dialog instance but also raises an event
        /// </summary>
        internal void OnCheckBoxCheckedChanged(bool isChecked)
        {
            flags[checkBoxChecked] = isChecked;
            checkBoxCheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        internal void OnHelpRequested() => helpRequested?.Invoke(this, EventArgs.Empty);

        internal void OnDetailsVisibleChanged(TaskDialogDetailsVisibleChangedEventArgs e)
        {
            if (e.DetailsVisible)
                options |= TaskDialogOptions.DetailsExpanded;
            else
                options &= ~TaskDialogOptions.DetailsExpanded;

            detailsVisibleChanged?.Invoke(this, e);
        }

        internal void OnClosing(CancelEventArgs e) => closing?.Invoke(this, e);
        internal void OnClosed() => closed?.Invoke(this, EventArgs.Empty);

        #endregion

        #region Private Methods

        private void CheckDisposed()
        {
            if (flags[isDisposed])
                throw new ObjectDisposedException(nameof(TaskDialog), PublicResources.ObjectDisposed);
        }

        private TaskDialogResult ShowInternal(IntPtr owner, out int customButtonIndex, out int radioButtonIndex, out bool verificationTextChecked)
        {
            CheckDisposed();

            // cleaning up possible last results
            dialogResult = TaskDialogResult.None;
            selectedButtonIndex = -1;
            selectedRadioButtonIndex = -1;

            CreateDialogInstance();
            try
            {
                dialogResult = dialogInstance!.Execute(this, owner, out selectedButtonIndex, out selectedRadioButtonIndex, out bool isChecked);
                flags[checkBoxChecked] = isChecked;
                customButtonIndex = selectedButtonIndex;
                radioButtonIndex = selectedRadioButtonIndex;
                verificationTextChecked = isChecked;
                return dialogResult;
            }
            finally
            {
                dialogInstance!.Dispose();
                dialogInstance = null;
            }
        }

        private void CreateDialogInstance()
        {
            dialogInstance = ForceCompatibilityMode || IsNonNativeFeatureRequired() || !NativeTaskDialog.IsAvailable
                ? new TaskDialogForm()
                : new NativeTaskDialog();
        }

        /// <summary>
        /// Gets whether compatibility mode is required because of the chosen settings.
        /// </summary>
        private bool IsNonNativeFeatureRequired()
        {
            return ((options & TaskDialogOptions.TranslateStandardButtons) != 0) // options not supported natively
                || icon == TaskDialogStandardIcon.SecurityQuestion // security question icon (due to blue background)
                || buttons.Any(b => b.CustomIcon != null); // custom button icons
        }

        private void Dispose(bool disposing)
        {
            flags[isDisposed] = true;

            // clearing events in all circumstances
            created = null;
            tick = null;
            closing = null;
            closed = null;
            hyperlinkClicked = null;
            checkBoxCheckedChanged = null;
            helpRequested = null;
            detailsVisibleChanged = null;

            if (disposing)
            {
                dialogInstance?.Dispose();
                ((IDisposable)buttons).Dispose();
                ((IDisposable)radioButtons).Dispose();
                dialogInstance = null;
                message = null;
                mainInstruction = null;
                caption = null;
                footerText = null;
                checkBoxText = null;
                detailsText = null;
                showDetailsText = null;
                hideDetailsText = null;
                buttons = null!;
                radioButtons = null!;
            }
        }

        #endregion

        #endregion
    }
}
