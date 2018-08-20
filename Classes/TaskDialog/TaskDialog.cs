extern alias lang;
#region Used namespaces

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.Libraries;

using Language = lang::KGySoft.Libraries.Language.Language;

#endregion

namespace KGySoft.Controls
{
    /// <summary>
    /// Represents a task dialog window that is able display regular buttons, Vista-like command options,
    /// radio buttons and progress bar. Can work in compatibility mode so dialog can be use even with Windows XP.
    /// <note><see cref="TaskDialog"/> implements <see cref="IDisposable"/>. When task a dialog is disposed, it frees
    /// every event subscriptions so it is not needed to unsubscribe events explicitly.</note>
    /// </summary>
    public sealed class TaskDialog : IWin32Window, IDisposable
    {
        #region Constants

        #region Internal Constants

        internal const string PropertyMessage = "Message";
        internal const string PropertyMainInstruction = "MainInstruction";
        internal const string PropertyCaption = "Caption";
        internal const string PropertyFooterText = "FooterText";
        internal const string PropertyDetailsText = "DetailsText";
        internal const string PropertyCheckBoxText = "CheckBoxText";
        internal const string PropertyCheckBoxChecked = "CheckBoxChecked";
        internal const string PropertyShowDetailsText = "ShowDetailsText";
        internal const string PropertyHideDetailsText = "HideDetailsText";
        internal const string PropertyIcon = "Icon";
        internal const string PropertyCustomIcon = "CustomIcon";
        internal const string PropertyFooterIcon = "FooterIcon";
        internal const string PropertyCustomFooterIcon = "CustomFooterIcon";
        internal const string PropertyStandardButtons = "StandardButtons";
        internal const string PropertyDefaultStandardButton = "DefaultStandardButton";
        internal const string PropertyOptions = "Options";
        internal const string PropertyWidth = "Width";
        internal const string PropertyProgressBarStyle = "ProgressBarStyle";
        internal const string PropertyProgressBarState = "ProgressBarState";
        internal const string PropertyProgressBarMinimum = "ProgressBarMinimum";
        internal const string PropertyProgressBarMaximum = "ProgressBarMaximum";
        internal const string PropertyProgressBarValue = "ProgressBarValue";
        internal const string PropertyProgressBarMarqueeAnimationSpeed = "ProgressBarMarqueeAnimationSpeed";

        #endregion

        #region Private Constants

        private const string propertyChangeNotAllowed = "Changing {0} property is not allowed while dialog is displayed__TaskDialog";
        private const TaskDialogOptions allOptions = TaskDialogOptions.HyperlinksEnabled | TaskDialogOptions.AllowCancel | TaskDialogOptions.UseCommandLinks | TaskDialogOptions.UseCommandLinksNoIcon | TaskDialogOptions.ExpandFooterArea | TaskDialogOptions.DetailsExpanded | TaskDialogOptions.PositionRelativeToWindow | TaskDialogOptions.RightToLeftLayout | TaskDialogOptions.AllowMinimize;// | TaskDialogOptions.SelectableTexts;

        #endregion

        #endregion

        #region Fields

        private bool disposed;
        private ITaskDialog dialogInstance;
        private string message;
        private string mainInstruction;
        private string caption;
        private string footerText;
        private bool forceCompatibilityMode;
        private string checkBoxText;
        private string detailsText;
        private TaskDialogOptions options;
        private string showDetailsText;
        private string hideDetailsText;
        private TaskDialogStandardIcons icon;
        private Icon customIcon;
        private TaskDialogStandardIcons footerIcon;
        private Icon customFooterIcon;
        private bool isEmulatedStandardMainIcon;
        private bool isEmulatedStandardFooterIcon;
        private Icon formIcon;
        private TaskDialogStandardButtonFlags standardButtons;
        private TaskDialogStandardButtons defaultStandardButton;
        private bool checkBoxChecked;
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

        // hiding event backing fields as they were simple auto events
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler created;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<TaskDialogTickEventArgs> tick;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<CancelEventArgs> closing;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler closed;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<HyperlinkClickedEventArgs> hyperlinkClicked;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler checkBoxCheckedChanged;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler helpRequested;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private EventHandler<TaskDialogDetailsVisibleChangedEventArgs> detailsVisibleChanged;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the <see cref="TaskDialog"/> has been created and is before displayed.
        /// </summary>
        public event EventHandler Created
        {
            add
            {
                CheckDisposed();
                created += value;
            }
            remove { created -= value; }
        }

        /// <summary>
        /// Occurs approximately every 200 milliseconds.
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
                        dialogInstance.TimerChanged(true);
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
                    dialogInstance.TimerChanged(false);
            }
        }

        /// <summary>
        /// Occurs when <see cref="TaskDialogOptions.HyperlinksEnabled"/> is set in <see cref="Options"/>
        /// and user clicks a hyperlink. If this event is not subscribed or <see cref="HandledEventArgs.Handled"/> is set to <see langword="false"/> in <see cref="HyperlinkClickedEventArgs"/>,
        /// then system tries to resolve the hyperlink.
        /// <note>Resolving the link by the operating system might be blocked so usually the best way is to subscribe this event.</note>
        /// </summary>
        public event EventHandler<HyperlinkClickedEventArgs> HyperlinkClicked
        {
            add
            {
                CheckDisposed();
                hyperlinkClicked += value;
            }
            remove { hyperlinkClicked -= value; }
        }

        /// <summary>
        /// Occurs when <see cref="CheckBoxChecked"/> property changes.
        /// </summary>
        public event EventHandler CheckBoxCheckedChanged
        {
            add
            {
                CheckDisposed();
                checkBoxCheckedChanged += value;
            }
            remove { checkBoxCheckedChanged -= value; }
        }

        /// <summary>
        /// Occurs when user presses F1 on the dialog.
        /// </summary>
        public event EventHandler HelpRequested
        {
            add
            {
                CheckDisposed();
                helpRequested += value;
            }
            remove { helpRequested -= value; }
        }

        /// <summary>
        /// Occurs when visibility of the Show/Hide Details expando button changes.
        /// </summary>
        public event EventHandler<TaskDialogDetailsVisibleChangedEventArgs> DetailsVisibleChanged
        {
            add
            {
                CheckDisposed();
                detailsVisibleChanged += value;
            }
            remove { detailsVisibleChanged -= value; }
        }

        /// <summary>
        /// Occurs when the dialog about to be closed.
        /// </summary>
        public event EventHandler<CancelEventArgs> Closing
        {
            add
            {
                CheckDisposed();
                closing += value;
            }
            remove { closing -= value; }
        }

        #endregion

        #region Properties

        #region Public Properties

        /// <summary>
        /// Gets or sets the message text.
        /// </summary>
        public string Message
        {
            get { return message; }
            set
            {
                if (message == value)
                    return;

                CheckCanChangeProperty();
                message = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyMessage);
            }
        }

        /// <summary>
        /// Gets or sets the main instruction text.
        /// </summary>
        public string MainInstruction
        {
            get { return mainInstruction; }
            set
            {
                if (mainInstruction == value)
                    return;

                CheckCanChangeProperty();
                mainInstruction = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyMainInstruction);
            }
        }

        /// <summary>
        /// Gets or sets the caption of the task dialog. If caption is <see langword="null"/>, the filename of the executable program is used.
        /// </summary>
        public string Caption
        {
            get { return caption; }
            set
            {
                if (caption == value)
                    return;

                CheckCanChangeProperty();
                caption = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyCaption);
            }
        }

        /// <summary>
        /// Gets or sets the footer text.
        /// </summary>
        public string FooterText
        {
            get { return footerText; }
            set
            {
                if (footerText == value)
                    return;

                CheckCanChangeProperty();
                footerText = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyFooterText);
            }
        }

        /// <summary>
        /// Gets or sets the verification text of the checkbox of the task dialog. The checkbox will be visible when this property is not empty when the dialog is shown.
        /// </summary>
        public string CheckBoxText
        {
            get { return checkBoxText; }
            set
            {
                if (checkBoxText == value)
                    return;

                CheckCanChangeProperty();
                checkBoxText = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyCheckBoxText);
            }
        }

        /// <summary>
        /// Gets or sets whether the verification check box is checked. This property is ignored when <see cref="CheckBoxText"/> is empty.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxText"/>
        public bool CheckBoxChecked
        {
            get { return checkBoxChecked; }
            set
            {
                if (checkBoxChecked == value)
                    return;

                CheckCanChangeProperty();
                checkBoxChecked = value;

                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyCheckBoxChecked);
                }

                OnCheckBoxCheckedChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets the details text. See/Hide details button will be visible only when this property is not empty.
        /// </summary>
        public string DetailsText
        {
            get { return detailsText; }
            set
            {
                if (detailsText == value)
                    return;

                CheckCanChangeProperty();
                detailsText = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyDetailsText);
            }
        }

        /// <summary>
        /// Gest or sets options of the task dialog.
        /// </summary>
        public TaskDialogOptions Options
        {
            get { return options; }
            set
            {
                if (options == value)
                    return;

                CheckCanChangeProperty();
                if ((value | allOptions) != allOptions)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                options = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyOptions);
                }

                //if (dialogInstance != null)
                //{
                //    throw new InvalidOperationException(Language.Translate(propertyChangeNotAllowed, "Options"));
                //}

                //if ((value | allOptions) != allOptions)
                //{
                //    throw new ArgumentOutOfRangeException("value");
                //}

                //options = value;
            }
        }

        /// <summary>
        /// Gets or sets the text of the Show Details button. The Show Details button is visible only when <see cref="DetailsText"/> is not empty.
        /// </summary>
        /// <remarks>
        /// When both <see cref="ShowDetailsText"/> and <see cref="HideDetailsText"/> properties are empty, a default text will be displayed. When one
        /// of these properties is empty while the other is set, the non-empty value will be displayed in both expanded and collapsed states.
        /// </remarks>
        public string ShowDetailsText
        {
            get { return showDetailsText; }
            set
            {
                if (showDetailsText == value)
                    return;

                CheckCanChangeProperty();
                showDetailsText = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyShowDetailsText);
            }
        }

        /// <summary>
        /// Gets or sets the text of the Hide Details button. The Hide Details button is visible only when <see cref="DetailsText"/> is not empty.
        /// </summary>
        /// <remarks>
        /// When both <see cref="ShowDetailsText"/> and <see cref="HideDetailsText"/> properties are empty, a default text will be displayed. When one
        /// of these properties is empty while the other is set, the non-empty value will be displayed in both expanded and collapsed states.
        /// </remarks>
        public string HideDetailsText
        {
            get { return hideDetailsText; }
            set
            {
                if (hideDetailsText == value)
                    return;

                CheckCanChangeProperty();
                hideDetailsText = value;

                if (IsDialogShowing)
                    dialogInstance.PropertyChanged(PropertyHideDetailsText);
            }
        }

        /// <summary>
        /// Gets or sets one of the standard icons as the main icon for the dialog.
        /// Setting this property clears <see cref="CustomIcon"/> and vice-versa.
        /// </summary>
        public TaskDialogStandardIcons Icon
        {
            get { return icon; }
            set
            {
                if (icon == value && customIcon == null)
                    return;

                CheckCanChangeProperty();

                if (!Enum<TaskDialogStandardIcons>.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                isEmulatedStandardMainIcon = false;
                ReplaceIcon(ref customIcon, null, 0);
                ReplaceIcon(ref formIcon, null, 0);
                icon = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyIcon);
                }
            }
        }

        /// <summary>
        /// Gets or sets a custom <see cref="System.Drawing.Icon"/> as the main icon for the dialog.
        /// Setting this property clears <see cref="Icon"/> and vice-versa.
        /// </summary>
        public Icon CustomIcon
        {
            get { return isEmulatedStandardMainIcon ? null : customIcon; }
            set
            {
                if (customIcon == value)
                    return;

                CheckCanChangeProperty();
                isEmulatedStandardMainIcon = false;
                icon = TaskDialogStandardIcons.None;
                ReplaceIcon(ref customIcon, value, 32);
                //ReplaceIcon(ref formIcon, value, 16);
                formIcon = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyCustomIcon);
                }
            }
        }

        /// <summary>
        /// Gets or sets one of the standard icons as the footer icon for the dialog.
        /// Setting this property clears <see cref="CustomFooterIcon"/> and vice-versa.
        /// </summary>
        public TaskDialogStandardIcons FooterIcon
        {
            get { return footerIcon; }
            set
            {
                if (footerIcon == value && customFooterIcon == null)
                    return;

                CheckCanChangeProperty();

                if (!Enum<TaskDialogStandardIcons>.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                isEmulatedStandardFooterIcon = false;
                ReplaceIcon(ref customFooterIcon, null, 0);
                footerIcon = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyFooterIcon);
                }
            }
        }

        /// <summary>
        /// Gets or sets a custom <see cref="System.Drawing.Icon"/> as the footer icon for the dialog.
        /// Setting this property clears <see cref="FooterIcon"/> and vice-versa.
        /// </summary>
        public Icon CustomFooterIcon
        {
            get { return isEmulatedStandardFooterIcon ? null : customFooterIcon; }
            set
            {
                if (customFooterIcon == value)
                    return;

                CheckCanChangeProperty();
                isEmulatedStandardFooterIcon = false;
                footerIcon = TaskDialogStandardIcons.None;
                ReplaceIcon(ref customFooterIcon, value, 16);
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyCustomFooterIcon);
                }
            }
        }

        /// <summary>
        /// Gets or sets standard buttons if the dialog. When <see cref="Buttons"/> are not empty, this property is ignored.
        /// If neither standard nor custom buttons are specified, the task dialog will contain the OK button by default.
        /// By default, these buttons will use the current windows language, unless <see cref="TaskDialogOptions.TranslateStandardButtons"/> is set
        /// in <see cref="Options"/>.
        /// </summary>
        public TaskDialogStandardButtonFlags StandardButtons
        {
            get { return standardButtons; }
            set
            {
                if (standardButtons == value)
                    return;

                CheckCanChangeProperty();
                if (((int)value | 0x3F) != 0x3F)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                standardButtons = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyStandardButtons);
                }
            }
        }

        /// <summary>
        /// Gets or sets the default standard buttons. When <see cref="Buttons"/> are not empty and one of them is specified as default button
        /// by <see cref="TaskDialogButton.IsDefault"/>, then this property is ignored.
        /// If neither standard nor custom buttons are specified as default, the first button will be the default one.
        /// </summary>
        public TaskDialogStandardButtons DefaultStandardButton
        {
            get { return defaultStandardButton; }
            set
            {
                if (defaultStandardButton == value)
                    return;

                CheckCanChangeProperty();
                if (!Enum<TaskDialogStandardButtons>.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                defaultStandardButton = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyDefaultStandardButton);
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the <see cref="TaskDialog"/> in dialog units.
        /// The dialog is always at least 100 DLU wide.
        /// Zero value means that size is auto-calculated.
        /// Re-assigning zero value adjusts the automatic width of the dialog on demand.
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                if (width == value && value != 0)
                    return;

                CheckCanChangeProperty();
                if (width < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                width = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyWidth);
                }
            }
        }

        /// <summary>
        /// Gets a value that contains the TaskDialog controls.
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
        /// Gets a value that contains the TaskDialog controls.
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
        /// </summary>
        public TaskDialogProgressBarStyle ProgressBarStyle
        {
            get { return progressBarStyle; }
            set
            {
                if (progressBarStyle == value)
                    return;

                CheckCanChangeProperty();

                if (!Enum<TaskDialogProgressBarStyle>.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                progressBarStyle = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarStyle);
                }
            }
        }

        /// <summary>
        /// Gets or sets the state of the progress bar.
        /// </summary>
        public ProgressBarState ProgressBarState
        {
            get { return progressBarState; }
            set
            {
                if (progressBarState == value)
                    return;

                CheckCanChangeProperty();

                if (!Enum<ProgressBarState>.IsDefined(value))
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                progressBarState = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarState);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// </summary>
        public int ProgressBarMinimum
        {
            get { return progressBarMinimum; }
            set
            {
                if (progressBarMinimum == value)
                    return;

                CheckCanChangeProperty();

                if (value < 0 || value >= progressBarMaximum)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (progressBarValue < value)
                {
                    ProgressBarValue = value;
                }

                progressBarMinimum = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarMinimum);
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// </summary>
        public int ProgressBarMaximum
        {
            get { return progressBarMaximum; }
            set
            {
                if (progressBarMaximum == value)
                    return;

                CheckCanChangeProperty();

                if (value < progressBarMinimum)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                if (progressBarValue > value)
                {
                    ProgressBarValue = value;
                }

                progressBarMaximum = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarMaximum);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Regular"/>.
        /// </summary>
        public int ProgressBarValue
        {
            get { return progressBarValue; }
            set
            {
                if (progressBarValue == value)
                    return;

                CheckCanChangeProperty();

                if (value < progressBarMinimum || value > progressBarMaximum)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                progressBarValue = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarValue);
                }
            }
        }

        /// <summary>
        /// Gets or sets the current animation speed value of the progress bar.
        /// This property is used only when <see cref="ProgressBarStyle"/> is <see cref="TaskDialogProgressBarStyle.Marquee"/>.
        /// Zero value stops the animation.
        /// </summary>
        public int ProgressBarMarqueeAnimationSpeed
        {
            get { return progressBarMarqueeAnimationSpeed; }
            set
            {
                if (progressBarMarqueeAnimationSpeed == value)
                    return;

                CheckCanChangeProperty();

                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value");
                }

                progressBarMarqueeAnimationSpeed = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyProgressBarMarqueeAnimationSpeed);
                }
            }
        }

        /// <summary>
        /// Gest or sets whether the <see cref="TaskDialog"/> is to be forced to operate in compatibility mode
        /// even if current operating system supports native task dialogs.
        /// </summary>
        public bool ForceCompatibilityMode
        {
            get { return forceCompatibilityMode; }
            set
            {
                if (dialogInstance != null)
                {
                    throw new InvalidOperationException(Language.Translate(propertyChangeNotAllowed, "ForceCompatibilityMode"));
                }

                forceCompatibilityMode = value;
            }
        }

        /// <summary>
        /// Gets whether <see cref="TaskDialog"/> is displayed in compatibility mode.
        /// When dialog is not displayed, returns <see langword="false"/>.
        /// </summary>
        public bool IsInCompatibilityMode
        {
            get { return IsDialogShowing && dialogInstance is Form; }
        }

        /// <summary>
        /// After the dialog is closed, gets the index of the clicked custom button defined in <see cref="Buttons"/> collection; otherwise, returns <c>-1</c>.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public int SelectedButtonIndex
        {
            get { return selectedButtonIndex; }
        }

        /// <summary>
        /// After the dialog is closed, gets the index of the selected radio button defined in <see cref="RadioButtons"/> collection; otherwise, returns <c>-1</c>.
        /// </summary>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public int SelectedRadioButtonIndex
        {
            get { return selectedRadioButtonIndex; }
        }

        /// <summary>
        /// <para>When read, gets the last result of a closed <see cref="TaskDialog"/> (if the dialog was closed by one of the <see cref="StandardButtons"/>).</para>
        /// <para>When set and there is an opened <see cref="TaskDialog"/>, forces to close the dialog with the specified result.</para>
        /// </summary>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult DialogResult
        {
            get { return dialogResult; }
            set
            {
                CheckDisposed();

                if (value != dialogResult)
                {
                    if (!Enum<TaskDialogResult>.IsDefined(value))
                    {
                        throw new ArgumentOutOfRangeException("value");
                    }

                    if (IsDialogShowing)
                    {
                        dialogInstance.Close(value);
                    }
                    else
                    {
                        dialogResult = value;
                    }
                }
            }
        }

        #endregion

        #region Internal Properties

        internal bool IsDialogShowing
        {
            get
            {
                return (dialogInstance != null)
                    && (dialogInstance.ShowState == TaskDialogStates.Showing
                    || dialogInstance.ShowState == TaskDialogStates.Closing);
            }
        }

        internal bool IsTickAssigned
        {
            get { return tick != null; }
        }

        /// <summary>
        /// When an <see cref="ITaskDialog"/> implementation does not support one of the <see cref="TaskDialogStandardIcons"/>, it can set this property
        /// to handle a standard icon as a custom one.
        /// </summary>
        internal Icon EmulatedStandardMainIcon
        {
            get { return isEmulatedStandardMainIcon ? customIcon : null; }
            set
            {
                isEmulatedStandardMainIcon = true;
                ReplaceIcon(ref customIcon, value, 32);
                formIcon = value;
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyCustomIcon);
                }
            }
        }

        /// <summary>
        /// When an <see cref="ITaskDialog"/> implementation does not support one of the <see cref="TaskDialogStandardIcons"/>, it can set this property
        /// to handle a standard icon as a custom one.
        /// </summary>
        internal Icon EmulatedStandardFooterIcon
        {
            get { return isEmulatedStandardFooterIcon ? customFooterIcon : null; }
            set
            {
                isEmulatedStandardFooterIcon = true;
                ReplaceIcon(ref customFooterIcon, value, 16);
                if (IsDialogShowing)
                {
                    dialogInstance.PropertyChanged(PropertyCustomFooterIcon);
                }
            }
        }

        /// <summary>
        /// Gets the form icon. It is set by setting <see cref="CustomIcon"/> or <see cref="EmulatedStandardMainIcon"/>.
        /// </summary>
        internal Icon FormIcon
        {
            get { return formIcon; }
        }

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

        ~TaskDialog()
        {
            Dispose(false);
        }

        #endregion

        #region Explicit Disposing

        private void Dispose(bool disposing)
        {
            disposed = true;

            // clearing events in all circumstances
            created = null;
            tick = null;
            closing = null;
            closed = null;
            hyperlinkClicked = null;
            checkBoxCheckedChanged = null;
            helpRequested = null;
            detailsVisibleChanged = null;

            // disposing some objects regardless of disposing parameter because they can hold
            // either unmanged resources or event subscriptions
            if (dialogInstance != null)
            {
                dialogInstance.Dispose();
                dialogInstance = null;
            }

            ((IDisposable)buttons).Dispose();
            ((IDisposable)radioButtons).Dispose();

            // on explicit disposing nullifying references
            if (disposing)
            {
                message = null;
                mainInstruction = null;
                caption = null;
                footerText = null;
                checkBoxText = null;
                detailsText = null;
                showDetailsText = null;
                hideDetailsText = null;
                ReplaceIcon(ref customIcon, null, 0);
                ReplaceIcon(ref customFooterIcon, null, 0);
                ReplaceIcon(ref formIcon, null, 0);
                buttons = null;
                radioButtons = null;
            }
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        internal static void ReplaceIcon(ref Icon value, Icon newValue, int requiredSize)
        {
            // same instances
            if (value == newValue)
            {
                return;
            }

            // disposing original instance
            if (value != null)
            {
                value.Dispose();
                value = null;
            }

            if (newValue == null)
            {
                return;
            }

            // Creating a local copy. Note that Icon.Size may report anything so trying to extract the requested size first.
            value = newValue.ExtractNearestIcon(new Size(requiredSize, requiredSize), PixelFormat.Format32bppArgb);
            if (value.Width == requiredSize)
                return;

            // in case no success, resizing the nearest icon
            using (Bitmap bmp = value.ToAlphaBitmap())
            {
                value.Dispose();
                value = IconTools.IconFromImage(bmp, requiredSize, true);
            }
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// <para>Result is stored also in <see cref="DialogResult"/> property.</para>
        /// </returns>
        /// <param name="owner">Handle of the parent window. Can be empty (<see cref="IntPtr.Zero"/>).</param>
        /// <param name="customButtonIndex">Returns the index of the clicked custom button specified in <see cref="Buttons"/> collection if the button closed the dialog.
        /// <para>Result is stored also in <see cref="SelectedButtonIndex"/> property.</para>
        /// </param>
        /// <param name="radioButtonIndex">Returns the index of the selected radio button specified in <see cref="RadioButtons"/> collection.
        /// <para>Result is stored also in <see cref="SelectedRadioButtonIndex"/> property.</para>
        /// </param>
        /// <param name="verificationTextChecked">Returns whether verification checkbox was checked when the dialog was closed.
        /// <para>Result is stored also in <see cref="CheckBoxChecked"/> property.</para>
        /// </param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IntPtr owner, out int customButtonIndex, out int radioButtonIndex, out bool verificationTextChecked)
        {
            return ShowInternal(owner, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);
        }

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// <para>Result is stored also in <see cref="DialogResult"/> property.</para>
        /// </returns>
        /// <param name="owner">Handle of the parent window. Can be <see langword="null"/>.</param>
        /// <param name="customButtonIndex">Returns the index of the clicked custom button specified in <see cref="Buttons"/> collection if the button closed the dialog.
        /// <para>Result is stored also in <see cref="SelectedButtonIndex"/> property.</para>
        /// </param>
        /// <param name="radioButtonIndex">Returns the index of the selected radio button specified in <see cref="RadioButtons"/> collection.
        /// <para>Result is stored also in <see cref="SelectedRadioButtonIndex"/> property.</para>
        /// </param>
        /// <param name="verificationTextChecked">Returns whether verification checkbox was checked when the dialog was closed.
        /// <para>Result is stored also in <see cref="CheckBoxChecked"/> property.</para>
        /// </param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IWin32Window owner, out int customButtonIndex, out int radioButtonIndex, out bool verificationTextChecked)
        {
            return Show(owner == null ? IntPtr.Zero : owner.Handle, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);
        }

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// <para>Result is stored also in <see cref="DialogResult"/> property.</para>
        /// </returns>
        /// <param name="owner">Handle of the parent window. Can be empty (<see cref="IntPtr.Zero"/>).</param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IntPtr owner)
        {
            int customButtonIndex;
            int radioButtonIndex;
            bool verificationTextChecked;
            return Show(owner, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);
        }

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// <para>Result is stored also in <see cref="DialogResult"/> property.</para>
        /// </returns>
        /// <param name="owner">Handle of the parent window. Can be <see langword="null"/>.</param>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show(IWin32Window owner)
        {
            int customButtonIndex;
            int radioButtonIndex;
            bool verificationTextChecked;
            return Show(owner == null ? IntPtr.Zero : owner.Handle, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);
        }

        /// <summary>
        /// Shows the <see cref="TaskDialog"/> using the current configuration.
        /// </summary>
        /// <returns>A <see cref="TaskDialogResult"/> value that indicates one of the pressed standard buttons specified by <see cref="StandardButtons"/> property.
        /// If return value is <see cref="TaskDialogResult.None"/>, then the dialog might have been closed by a custom button, which can be identified by <see cref="SelectedButtonIndex"/>.
        /// <para>Result is stored also in <see cref="DialogResult"/> property.</para>
        /// </returns>
        /// <seealso cref="DialogResult"/>
        /// <seealso cref="SelectedButtonIndex"/>
        /// <seealso cref="SelectedRadioButtonIndex"/>
        /// <seealso cref="CheckBoxChecked"/>
        public TaskDialogResult Show()
        {
            int customButtonIndex;
            int radioButtonIndex;
            bool verificationTextChecked;
            return Show(IntPtr.Zero, out customButtonIndex, out radioButtonIndex, out verificationTextChecked);
        }

        /// <summary>
        /// Forces to close the dialog. This causes that <see cref="DialogResult"/> will be <see cref="TaskDialogResult.Close"/>
        /// even if there was no Close button on the dialog.
        /// </summary>
        public void Close()
        {
            DialogResult = TaskDialogResult.Close;
        }

        #endregion

        #region Internal Methods

        internal void ControlPropertyChanged(TaskDialogControl control, string propName)
        {
            CheckCanChangeProperty();

            if (dialogInstance != null)
            {
                dialogInstance.ControlPropertyChanged(control, propName);
            }
        }

        internal void ControlCollectionChanged(IList collection, TaskDialogControlCollectionChangeTypes changeType, int index)
        {
            CheckCanChangeProperty();

            if (dialogInstance != null)
            {
                if (collection == buttons)
                {
                    dialogInstance.CustomButtonsChanged(changeType, index);
                }
                else if (collection == radioButtons)
                {
                    dialogInstance.RadioButtonsChanged(changeType, index);
                }
            }
        }

        internal void CheckCanChangeProperty()
        {
            CheckDisposed();

            if (dialogInstance == null
                || dialogInstance.ShowState != TaskDialogStates.Initializing)
            {
                return;
            }

            throw new InvalidOperationException("Cannot change property while TaskDialog is being initialized.");
        }

        internal void OnCreated()
        {
            if (created != null)
            {
                created.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnTick(TaskDialogTickEventArgs e)
        {
            if (tick != null)
            {
                tick.Invoke(this, e);
            }
        }

        internal void OnHyperlinkClicked(HyperlinkClickedEventArgs e)
        {
            if (hyperlinkClicked != null)
            {
                hyperlinkClicked.Invoke(this, e);
            }
            else
            {
                e.Handled = false;
            }
        }

        /// <summary>
        /// This is a callback notification of the dialog instance but also raises an event
        /// </summary>
        internal void OnCheckBoxCheckedChanged(bool isChecked)
        {
            checkBoxChecked = isChecked;
            if (checkBoxCheckedChanged != null)
            {
                checkBoxCheckedChanged.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnHelpRequested()
        {
            if (helpRequested != null)
            {
                helpRequested.Invoke(this, EventArgs.Empty);
            }
        }

        internal void OnDetailsVisibleChanged(TaskDialogDetailsVisibleChangedEventArgs e)
        {
            if (e.DetailsVisible)
            {
                options |= TaskDialogOptions.DetailsExpanded;
            }
            else
            {
                options &= ~TaskDialogOptions.DetailsExpanded;
            }

            if (detailsVisibleChanged != null)
            {
                detailsVisibleChanged.Invoke(this, e);
            }
        }

        internal void OnClosing(CancelEventArgs e)
        {
            if (closing != null)
            {
                closing.Invoke(this, e);
            }
        }

        internal void OnClosed()
        {
            if (closed != null)
                closed.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Private Methods

        private void CheckDisposed()
        {
            if (disposed)
            {
                throw new ObjectDisposedException("TaskDialog");
            }
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
                dialogResult = dialogInstance.Execute(this, owner, out selectedButtonIndex, out selectedRadioButtonIndex, out checkBoxChecked);
                customButtonIndex = selectedButtonIndex;
                radioButtonIndex = selectedRadioButtonIndex;
                verificationTextChecked = checkBoxChecked;
                return dialogResult;
            }
            finally
            {
                dialogInstance.Dispose();
                dialogInstance = null;
            }
        }

        private void CreateDialogInstance()
        {
            // compatibility mode
            if (forceCompatibilityMode || IsNonNativeFeatureRequired() || !NativeTaskDialog.IsAvailable)
            {
                dialogInstance = new TaskDialogForm();
            }
            else
            {
                dialogInstance = new NativeTaskDialog();
            }
        }

        /// <summary>
        /// Gets whether compatibility mode is required because of the chosen settings.
        /// </summary>
        private bool IsNonNativeFeatureRequired()
        {
            return ((int)options | 0xFFFF) > 0xFFFF // non-native options
                || icon == TaskDialogStandardIcons.SecurityQuestion // security question icon (due to blue background)
                || buttons.Any(b => b.CustomIcon != null); // custom button icons
        }

        #endregion

        #endregion

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region IWin32Window Members

        /// <summary>
        /// Gets the handle of the dialog.
        /// </summary>
        public IntPtr Handle { get; internal set; }

        #endregion
    }
}
