#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedMessageDialog.cs
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
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using KGySoft.Drawing;
using KGySoft.Libraries.Language;

#endregion

#region Suppressions

#if NETCOREAPP3_0 || NETCOREAPP3_1
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type - inconsistent nullability annotations on different platforms
#endif

#endregion

namespace KGySoft.WinForms.Forms
{
    /// <summary>
    /// Provides a dialog for error and other kind of messages.
    /// </summary>
    [Obsolete("Use the Dialogs class or the KGySoft.WinForms.Components.TaskDialog class instead.")]
    public sealed partial class AdvancedMessageDialog : BaseForm
    {
        #region Fields

        private string screenshot = String.Empty; // path of the screenshot
        //private Exception? exception = null; // the exception to be reported (only for the appropriate Execute method)
        private bool detailsVisible;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when the user presses the report sender button, which appears only when this event is subscribed.
        /// </summary>
        public static event EventHandler<ReportSenderEventArgs>? ReportSender;

        /// <summary>
        /// Occurs before the application terminates if user chooses closing the application.
        /// </summary>
        public static event EventHandler? BeforeKillApplication;

        #endregion

        #region Properties

        #region Static Properties

        #region Public Properties
        
        /// <summary>
        /// Gets or sets the log directory for saving logs and screenshots.
        /// </summary>
        public static string? ErrorLogDirectory { get; set; }

        /// <summary>
        /// An optional custom error handler that can be used to handle exceptions in a custom way (e.g. log into a database or file).
        /// </summary>
        public static Action<Exception>? CustomErrorHandler { get; set; }

        #endregion

        #region Private Properties

        private static string TextOk => Res.DialogsOKButtonText;
        private static string TextYes => Res.DialogsYesButtonText;
        private static string TextNo => Res.DialogsNoButtonText;
        private static string TextCancel => Res.DialogsCancelButtonText;
        private static string TextAbort => Res.DialogsAbortButtonText;
        private static string TextRetry => Res.DialogsRetryButtonText;
        private static string TextIgnore => Res.DialogsIgnoreButtonText;

        #endregion

        #endregion

        #region Instance Properties

        /// <summary>
        /// Gets or sets the caption text of the dialog.
        /// </summary>
        [Localizable(false)]
        [AllowNull]
        public override string Text
        {
            get => base.Text;
            set => base.Text = value;
        }

        /// <summary>
        /// Gets or sets the dialog image.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Image? Image
        {
            get => pbImage.Image;
            set => pbImage.Image = value;
        }

        #endregion

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="AdvancedMessageDialog"/> class.
        /// </summary>
        public AdvancedMessageDialog()
        {
            InitializeComponent();
            //btnCloseApp.Image = Images.Delete;
            //btnIgnore.Image = Images.Exit;
            //btnSendReport.Image = Images.Mail;
        }

        #endregion

        #region Methods

        #region Static Methods

        private static void ErrorToFile(string filename, string? logMessage)
        {
            try
            {
                if (!String.IsNullOrEmpty(ErrorLogDirectory) && !Path.IsPathRooted(filename))
                {
                    filename = Path.Combine(ErrorLogDirectory, filename);
                }
                filename += ".log";
                string? dir = Path.GetDirectoryName(Path.GetFullPath(filename));
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                using (StreamWriter sw = new StreamWriter(filename, true))
                {
                    sw.WriteLine(logMessage);
                    //return Path.GetFullPath(filename);
                }
            }
            catch (Exception e) when (!e.IsCritical())
            {
                // suppressing any error
                //return String.Empty;
            }
        }

        /// <summary>
        /// Screenshot saving into file. Does not throw exception, on error returns empty string.
        /// </summary>
        /// <param name="filename">Filename without extension.</param>
        /// <returns>Path or empty string if there was no save.</returns>
        private static string Screenshot(string filename)
        {
            try
            {
                if (!String.IsNullOrEmpty(ErrorLogDirectory) && !Path.IsPathRooted(filename))
                {
                    filename = Path.Combine(ErrorLogDirectory, filename);
                }
                filename += ".png";
                string? dir = Path.GetDirectoryName(Path.GetFullPath(filename));
                if (dir != null && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                Application.DoEvents();
                Thread.Sleep(100);
                Application.DoEvents();
                using (Image screenshot = WinForms.Screenshot.CaptureScreenshot())
                {
                    screenshot.Save(filename, ImageFormat.Png);

                    return Path.GetFullPath(filename);
                }
            }
            catch (Exception e) when (!e.IsCritical())
            {
                return String.Empty;
            }
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        /// <summary>
        /// Shows a message dialog for an exception.
        /// </summary>
        /// <param name="e">The exception to show in the dialog.</param>
        /// <param name="caption">The caption text of the dialog.</param>
        /// <param name="logNamePrefix">The prefix of the log file name to save. Path can be set in <see cref="ErrorLogDirectory"/>.
        /// Can be <see langword="null"/> to prevent saving the log.</param>
        public void Execute(Exception? e, string caption, string logNamePrefix)
        {
            ResetDetails(true);
            Text = caption;
            txtDetails.Text = e?.ToString();
            //exception = e;
            txtMessage.Text = e != null ? e.Message : Language.Translate("Unknown error__Dialogs");

            using (Icon icon = Icons.Shield)
            {
                using var resizedIcon = icon.Resize(pbImage.Size);
                pbImage.Image = resizedIcon.ExtractBitmap(0);
            }

            btnSendReport.Visible = ReportSender != null;
            pnlStandardButtons.Visible = false;
            try
            {
                Cursor.Current = Cursors.WaitCursor;
                try
                {
                    if (e != null)
                        CustomErrorHandler?.Invoke(e);
                    if (!String.IsNullOrEmpty(logNamePrefix))
                    {
                        string filename = logNamePrefix + DateTime.Now.ToString("yyyyMMddhhmmssffff", CultureInfo.InvariantCulture);
                        screenshot = Screenshot(filename);
                        ErrorToFile(filename, txtDetails.Text);
                    }
                }
                finally
                {
                    Cursor.Current = Cursors.Default;
                }
                ShowDialog();
            }
            catch (Exception ex) when (!ex.IsCritical())
            {
                CustomErrorHandler?.Invoke(ex);
            }
        }

        /// <summary>
        /// Shows a message dialog for an exception with a default caption.
        /// Saves log and screenshot if directory is set in <see cref="ErrorLogDirectory"/>.
        /// </summary>
        /// <param name="e">The exception to show in the dialog.</param>
        public void Execute(Exception e)
        {
            Execute(e, Language.Translate("Unhandled error caught__Dialogs"), "fatalerror");
        }

        /// <summary>
        /// Shows a message dialog for any kind of message.
        /// </summary>
        /// <returns>The <see cref="DialogResult"/> returned by the dialog.</returns>
        /// <param name="message">The message to show.</param>
        /// <param name="details">The details text (if <see langword="null"/> or empty, the Details button will be hidden)</param>
        /// <param name="caption">The caption text of the dialog.</param>
        /// <param name="dialogType">Specifies the dialog icon.</param>
        /// <param name="buttons">Specifies the buttons to show.</param>
        /// <param name="saveLog"><see langword="true"/> to save the logs (see also <see cref="ErrorLogDirectory"/>)</param>
        /// <param name="saveScreenshot"><see langword="true"/> to save a screenshot (see also <see cref="ErrorLogDirectory"/>)</param>
        /// <param name="logNamePrefix">The prefix of the log file name to save.</param>
        public DialogResult Execute(string? message, string? details, string? caption, AdvancedDialogTypes dialogType,
            ButtonTypes buttons, bool saveLog, bool saveScreenshot, string? logNamePrefix)
        {
            try
            {
                ResetDetails(!String.IsNullOrEmpty(details));
                Text = caption;
                txtMessage.Text = message ?? String.Empty;
                txtDetails.Text = details ?? String.Empty;
                btnDetails.Visible = txtDetails.Text.Length > 0;
                Icon? icon = null;

                switch (dialogType)
                {
                    case AdvancedDialogTypes.Information:
                        icon = Icons.Information;
                        break;
                    case AdvancedDialogTypes.Confirmation:
                        icon = Icons.Question;
                        break;
                    case AdvancedDialogTypes.Warning:
                        icon = Icons.Warning;
                        break;
                    case AdvancedDialogTypes.Error:
                        icon = Icons.Error;
                        break;
                    case AdvancedDialogTypes.Exception:
                        icon = Icons.Shield;
                        break;
                    case AdvancedDialogTypes.CustomImage:
                        // the image can be set before calling Execute
                        break;
                }

                if (icon != null)
                {
                    using var resizedIcon = icon.Resize(pbImage.Size); // 128x128 on 100% DPI
                    pbImage.Image = resizedIcon.ExtractBitmap(0);
                    icon.Dispose();
                }

                SetButtons(buttons);

                if (saveScreenshot || saveLog)
                {
                    Cursor.Current = Cursors.WaitCursor;
                    try
                    {
                        string filename = (logNamePrefix ?? String.Empty) + DateTime.Now.ToString("yyyyMMddhhmmssffff", CultureInfo.InvariantCulture);
                        if (saveScreenshot)
                            screenshot = Screenshot(filename);
                        if (saveLog)
                            ErrorToFile(filename, details);
                    }
                    finally
                    {
                        Cursor.Current = Cursors.Default;
                    }
                }
                //exception = null;
                return ShowDialog();
            }
            catch (Exception ex) when (!ex.IsCritical())
            {
                CustomErrorHandler?.Invoke(ex);
                return DialogResult.None;
            }
        }

        /// <summary>
        /// Shows a message dialog for any kind of message.
        /// </summary>
        /// <returns>The <see cref="DialogResult"/> returned by the dialog.</returns>
        /// <param name="message">The message to show.</param>
        /// <param name="details">The details text (if <see langword="null"/> or empty, the Details button will be hidden)</param>
        /// <param name="caption">The caption text of the dialog.</param>
        /// <param name="dialogType">Specifies the dialog icon</param>
        /// <param name="buttons">Specifies the buttons to show.</param>
        public DialogResult Execute(string message, string? details, string? caption, AdvancedDialogTypes dialogType, ButtonTypes buttons)
        {
            return Execute(message, details, caption, dialogType, buttons, false, false, null);
        }

        /// <summary>
        /// Shows a message dialog for any kind of message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="caption">The caption text of the dialog.</param>
        /// <param name="dialogType">Affects the dialog icon and the buttons of the dialog.</param>
        /// <returns>The <see cref="DialogResult"/> returned by the dialog.</returns>
        /// <remarks>
        /// <para>Details will be shown if <paramref name="dialogType"/> is <see cref="AdvancedDialogTypes.Exception"/>.</para>
        /// <para>Log and screenshot will be saved if <paramref name="dialogType"/> is <see cref="AdvancedDialogTypes.Exception"/> and <see cref="ReportSender"/> is assigned.</para>
        /// <para>Buttons are controlled by the <paramref name="dialogType"/> parameter.</para>
        /// </remarks>
        public DialogResult Execute(string message, string caption, AdvancedDialogTypes dialogType)
        {
            bool showDetails = dialogType == AdvancedDialogTypes.Exception;
            ButtonTypes btn;

            switch (dialogType)
            {
                //case AdvancedDialogTypes.Error:
                //    btn = ButtonTypes.ClosewinSendreport;
                //    break;
                case AdvancedDialogTypes.Exception:
                    btn = ButtonTypes.ClosewinSendreportCloseapp;
                    break;
                case AdvancedDialogTypes.Confirmation:
                    btn = ButtonTypes.YesNo;
                    break;
                default:
                    btn = ButtonTypes.OK;
                    break;
            }

            // TODO: SysInfo?
            //string details = showDetails ? (Language.Translate("Message: {0}{1}{2}{1}{3}__Dialogs", message, Environment.NewLine,
            //    Language.Translate("System information:"), DiagnosticTools.SysInfoToString())) : String.Empty;
            return Execute(message, /*details*/null, caption, dialogType,
                btn,                                           // buttons
                showDetails && ReportSender != null,           // saveLog
                showDetails && ReportSender != null,           // saveScreenshot
                "error");                                      // logNamePrefix
        }

        /// <summary>
        /// Shows a message dialog for any kind of message.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <param name="dialogType">Affects the dialog icon and the buttons of the dialog.</param>
        /// <returns>The <see cref="DialogResult"/> returned by the dialog.</returns>
        /// <remarks>
        /// <para>Details will be shown if <paramref name="dialogType"/> is <see cref="AdvancedDialogTypes.Exception"/>.</para>
        /// <para>Log and screenshot will be saved if <paramref name="dialogType"/> is <see cref="AdvancedDialogTypes.Exception"/> and <see cref="ReportSender"/> is assigned.</para>
        /// <para>Buttons are controlled by the <paramref name="dialogType"/> parameter.</para>
        /// </remarks>
        public DialogResult Execute(string message, AdvancedDialogTypes dialogType)
        {
            string title = dialogType.ToString() + Language.DistinctionSeparator + "Dialogs";
            if (dialogType == AdvancedDialogTypes.Exception)
                title = "Unhandled error caught" + Language.DistinctionSeparator + "Dialogs";
            return Execute(message, Language.Translate(title), dialogType);
        }

        /// <summary>
        /// Shows an information dialog.
        /// </summary>
        /// <param name="message">The message to show.</param>
        /// <returns>The <see cref="DialogResult"/> returned by the dialog.</returns>
        public DialogResult Execute(string message)
        {
            return Execute(message, AdvancedDialogTypes.Information);
        }

        #endregion

        #region Private Methods

        private void ResetDetails(bool enableDetails)
        {
            pnlMessageHeader.Visible = enableDetails;
            pnlMessage.Dock = DockStyle.Fill;
            splitter.Visible = false;
            pnlDetailsHeader.Visible = false;
            pnlDetails.Visible = false;
            detailsVisible = false;
        }

        private void ShowDetails(bool visible)
        {
            btnDetails.Text = visible ? Language.Translate("Hide Details__Dialogs") : Language.Translate("Show Details__Dialogs");
            pnlMessage.Dock = visible ? DockStyle.Top : DockStyle.Fill;
            splitter.Visible = visible;
            splitter.BringToFront();
            pnlDetailsHeader.Visible = visible;
            pnlDetailsHeader.BringToFront();
            pnlDetails.Visible = visible;
            pnlDetails.BringToFront();
            detailsVisible = visible;
        }

        private void SetButtons(ButtonTypes buttons)
        {
            if (buttons < ButtonTypes.Closewin)
            {
                pnlErrorButtons.Visible = false;
                pnlStandardButtons.Controls.Clear();
            }
            else
                pnlStandardButtons.Visible = false;

            Button? b;
            switch (buttons)
            {
                case ButtonTypes.OK:
                    b = new Button();
                    b.Text = TextOk;
                    b.DialogResult = DialogResult.OK;
                    //b.Image = Images.Accept;
                    AcceptButton = b;
                    CancelButton = b;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    break;
                case ButtonTypes.YesNo:
                    pnlStandardButtons.ColumnCount = 2;
                    b = new Button();
                    b.Text = TextYes;
                    b.DialogResult = DialogResult.Yes;
                    //b.Image = Images.Accept;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    b = new Button();
                    b.Text = TextNo;
                    b.DialogResult = DialogResult.No;
                    //b.Image = Images.Refuse;
                    pnlStandardButtons.Controls.Add(b, 1, 0);
                    break;
                case ButtonTypes.YesNoCancel:
                    pnlStandardButtons.ColumnCount = 3;
                    b = new Button();
                    b.Text = TextYes;
                    b.DialogResult = DialogResult.Yes;
                    //b.Image = Images.Accept;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    b = new Button();
                    b.Text = TextNo;
                    b.DialogResult = DialogResult.No;
                    //b.Image = Images.Refuse;
                    pnlStandardButtons.Controls.Add(b, 1, 0);
                    b = new Button();
                    b.Text = TextCancel;
                    b.DialogResult = DialogResult.Cancel;
                    //b.Image = Images.Delete;
                    CancelButton = b;
                    pnlStandardButtons.Controls.Add(b, 2, 0);
                    break;
                case ButtonTypes.OKCancel:
                    pnlStandardButtons.ColumnCount = 2;
                    b = new Button();
                    b.Text = TextOk;
                    b.DialogResult = DialogResult.OK;
                    //b.Image = Images.Accept;
                    AcceptButton = b;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    b = new Button();
                    b.Text = TextCancel;
                    b.DialogResult = DialogResult.Cancel;
                    //b.Image = Images.Delete;
                    CancelButton = b;
                    pnlStandardButtons.Controls.Add(b, 1, 0);
                    break;
                case ButtonTypes.RetryCancel:
                    pnlStandardButtons.ColumnCount = 2;
                    b = new Button();
                    b.Text = TextRetry;
                    b.DialogResult = DialogResult.Retry;
                    //b.Image = Images.Redo;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    b = new Button();
                    b.Text = TextCancel;
                    b.DialogResult = DialogResult.Cancel;
                    //b.Image = Images.Delete;
                    CancelButton = b;
                    pnlStandardButtons.Controls.Add(b, 1, 0);
                    break;
                case ButtonTypes.AbortRetryIgnore:
                    pnlStandardButtons.ColumnCount = 3;
                    b = new Button();
                    b.Text = TextAbort;
                    b.DialogResult = DialogResult.Abort;
                    //b.Image = Images.Refuse;
                    pnlStandardButtons.Controls.Add(b, 0, 0);
                    b = new Button();
                    b.Text = TextRetry;
                    b.DialogResult = DialogResult.Retry;
                    //b.Image = Images.Redo;
                    pnlStandardButtons.Controls.Add(b, 1, 0);
                    b = new Button();
                    b.Text = TextIgnore;
                    b.DialogResult = DialogResult.Ignore;
                    //b.Image = Images.None;
                    CancelButton = b;
                    pnlStandardButtons.Controls.Add(b, 2, 0);
                    break;
                case ButtonTypes.Closewin:
                    btnSendReport.Visible = false;
                    btnCloseApp.Visible = false;
                    break;
                case ButtonTypes.ClosewinSendreport:
                    btnSendReport.Visible = ReportSender != null;
                    btnCloseApp.Visible = false;
                    break;
                case ButtonTypes.ClosewinSendreportCloseapp:
                    btnSendReport.Visible = ReportSender != null;
                    break;
            }

            if (buttons < ButtonTypes.Closewin)
            {
                foreach (Control c in pnlStandardButtons.Controls)
                {
                    b = c as Button;
                    if (b != null)
                    {
                        b.Size = new Size(160, 28);
                        b.Anchor = AnchorStyles.None;
                        //b.TextImageRelation = TextImageRelation.ImageBeforeText;
                        //b.TextAlign = ContentAlignment.MiddleRight;
                    }
                }
                for (int i = 0; i < pnlStandardButtons.ColumnStyles.Count; i++)
                {
                    pnlStandardButtons.ColumnStyles[i].SizeType = SizeType.Percent;
                    pnlStandardButtons.ColumnStyles[i].Width = 100f / pnlStandardButtons.ColumnStyles.Count;
                }
            }
        }

        private void OnSendReport(ReportSenderEventArgs e) => ReportSender?.Invoke(this, e);

        #endregion

        #region Event handlers
#pragma warning disable IDE1006 // Naming Styles

        private void btnDetails_Click(object? sender, EventArgs e)
        {
            ShowDetails(!detailsVisible);
        }

        private void btnIgnore_Click(object? sender, EventArgs e)
        {
            Close();
        }

        private void btnCloseApp_Click(object? sender, EventArgs e)
        {
            if (Dialogs.ConfirmMessage("Are you sure to terminate application? All unsaved work will be lost!"))
            {
                BeforeKillApplication?.Invoke(null, EventArgs.Empty);
                Process.GetCurrentProcess().Kill();
                Application.Exit();
            }
        }

        private void btnSendReport_Click(object? sender, EventArgs e)
        {
            if (ReportSender != null)
            {
                ReportSenderEventArgs args = new ReportSenderEventArgs(txtMessage.Text, txtDetails.Text, screenshot);
                Hide();
                try
                {
                    OnSendReport(args);
                    if (args.CloseMessageDialog)
                        Close();

                }
                finally
                {
                    Show();
                }
            }
        }

#pragma warning restore IDE1006 // Naming Styles
        #endregion

        #endregion

        #endregion
    }
}
