#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AdvancedSaveFileDialog.cs
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
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using KGySoft.WinForms.WinApi;

#endregion

#region Suppressions

#if !NETCOREAPP3_0_OR_GREATER
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type. - analyzer false alarm for .NET Framework
#pragma warning disable CS8602 // Dereference of a possibly null reference. - analyzer false alarm for .NET Framework
#endif

#endregion

namespace KGySoft.WinForms.Components
{
    // TODO: Use current Windows appearance, see Windows API Code Pack 1.1 CustomCommonFileDialogsDemo
    /// <summary>
    /// Windows save file dialog that can host a custom control and can raise events.
    /// </summary>
    public sealed class AdvancedSaveFileDialog : IDisposable
    {
        #region Fields

        private IntPtr labelHandle;
        //private Screen activeScreen;
        private IntPtr ptrTemplate;
        private string[]? fileTypes;
        private bool isInitialized;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the index of the <see cref="Filter"/> currently selected in the file dialog box as
        /// a one-based index.
        /// The default value is 1.
        /// </summary>
        public int FilterIndex { get; set; }

        /// <summary>
        /// Gets or sets default extension.
        /// The dialog appends this extension to the file name if the user fails to type an extension.
        /// The string should not contain a period (.). If this member is NULL and the user fails to type an extension, no extension is appended.
        /// </summary>
        public string? DefaultExt { get; set; }

        /// <summary>
        /// Gets or sets file types filter.
        /// <example>
        /// Example: "C# files|*.cs|All files|*.*"
        /// </example>
        /// </summary>
        public string? Filter { get; set; }

        /// <summary>
        /// Gets or sets the selected file name
        /// </summary>
        public string? FileName { get; set; }

        /// <summary>
        /// Gets or sets the title of the dialog
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// A custom control that will be placed under the file type combo. It must be allocated and disposed by
        /// the user, <see cref="AdvancedSaveFileDialog"/> adjusts only its size if <see cref="CustomControlAutoSize"/> is true.
        /// </summary>
        public Control? CustomControl { get; set; }

        /// <summary>
        /// Gets or sets whether <see cref="CustomControl"/> should be resized when the file dialog is resized.
        /// </summary>
        public bool CustomControlAutoSize { get; set; }

        /// <summary>
        /// Gets or sets label of the custom control.
        /// </summary>
        public string? CustomControlLabel { get; set; }

        /// <summary>
        /// Gets or sets initial directory
        /// </summary>
        public string? InitialDirectory { get; set; }

        /// <summary>
        /// Gets or sets whether path of given file must exist. Default value is true.
        /// </summary>
        public bool PathMustExist { get; set; }

        /// <summary>
        /// Gets or sets forcing the showing of system and hidden files, thus overriding the user setting to show or not show hidden files.
        /// However, a file that is marked both system and hidden is not shown. Default is false.
        /// </summary>
        public bool ForceShowHiddenFiles { get; set; }

        /// <summary>
        /// Gets or sets whether Save As dialog box to generate a message box if the selected file already exists.
        /// The user must confirm whether to overwrite the file. Default is true;
        /// </summary>
        public bool PromptOverride { get; set; }

        #endregion

        #region Events

        /// <summary>
        /// Occurs when user selects another value in file type combo box.
        /// </summary>
        public event EventHandler<FileTypeChangedEventArgs>? FileTypeChanged;

        /// <summary>
        /// Occurs when user selects a file in the browser.
        /// </summary>
        public event EventHandler<SelectedFileChangedEventArgs>? SelectedFileChanged;

        #endregion

        #region Contruction and Destruction

        /// <summary>
        /// Creates a new instance of <see cref="AdvancedSaveFileDialog"/>.
        /// </summary>
        public AdvancedSaveFileDialog()
        {
            FilterIndex = 1;
            PathMustExist = true;
            PromptOverride = true;
        }

        /// <inheritdoc />
        ~AdvancedSaveFileDialog() => Dispose(false);

        #endregion

        #region Methods

        #region Public Methods

        /// <summary>
        /// Shows the file dialog
        /// </summary>
        /// <returns></returns>
        public DialogResult ShowDialog()
        {
            //set up the struct and populate it

            var ofn = new OPENFILENAME();
            ofn.lStructSize = MarshalHelper.SizeOf<OPENFILENAME>();
            if ((Environment.OSVersion.Platform != PlatformID.Win32NT) || (Environment.OSVersion.Version.Major < 5))
            {
                ofn.lStructSize = 0x4c;
            }

            string? filter = Filter;
            if (String.IsNullOrEmpty(filter))
                filter = " |*.*";
            ofn.lpstrFilter = filter.Replace('|', '\0') + '\0';
            fileTypes = filter.Split('|');
            ofn.nFilterIndex = FilterIndex;

            //filename in editor
            char[] fileNameChars = new String(' ', 0x1000).ToCharArray();
            ofn.nMaxFile = 0x2000;
            string fileName = String.Empty;
            if (!String.IsNullOrEmpty(FileName))
                fileName = FileName;
            if (fileName.Length > 0)
                fileName.CopyTo(0, fileNameChars, 0, fileName.Length);
            else
                fileNameChars[0] = '\0';

            ofn.lpstrFile = new String(fileNameChars);
            if (!String.IsNullOrEmpty(DefaultExt))
                ofn.lpstrDefExt = DefaultExt;
            ofn.lpstrFileTitle = null;
            ofn.nMaxFileTitle = 260;
            ofn.lpstrTitle = Title;
            ofn.lpstrDefExt = DefaultExt;
            ofn.lpstrInitialDir = InitialDirectory;

            //position the dialog above the active window
            if (Form.ActiveForm != null)
                ofn.hwndOwner = Form.ActiveForm.Handle;

            // Create an in-memory Win32 dialog template; this will be a "child" window inside the FileOpenDialog
            // We have no use for this child window, except that its presence allows us to capture events when
            // the user interacts with the FileOpenDialog
            ptrTemplate = BuildDialogTemplate();
            ofn.hInstance = ptrTemplate;

            //activeScreen = null;
            //if (Form.ActiveForm != null)
            //    activeScreen = Screen.FromControl(Form.ActiveForm);
            //activeScreen = Screen.PrimaryScreen;

            //set up some sensible flags
            int flags = Constants.OFN_EXPLORER | Constants.OFN_NOTESTFILECREATE | Constants.OFN_ENABLETAMPLATEHANDLE | Constants.OFN_ENABLEHOOK | Constants.OFN_HIDEREADONLY | Constants.OFN_ENABLESIZING;
            if (PathMustExist)
                flags |= Constants.OFN_PATHMUSTEXIST;
            if (ForceShowHiddenFiles)
                flags |= Constants.OFN_FORCESHOWHIDDEN;
            if (PromptOverride)
                flags |= Constants.OFN_OVERWRITEPROMPT;
            ofn.Flags = flags;

            //this is where the hook is set. Note that we can use a C# delegate in place of a C function pointer
            ofn.lpfnHook = HookProc;

            //if we're running on Windows 98/ME then the struct is smaller
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                ofn.lStructSize -= 12;
            }

            //ofn.FlagsEx = 1;

            //showing the dialog
            if (!Comdlg32.GetSaveFileName(ref ofn))
            {
                int ret = Comdlg32.CommDlgExtendedError();

                if (ret != 0)
                {
                    throw new InvalidOperationException("Couldn't show file dialog - " + ret);
                }

                return DialogResult.Cancel;
            }

            FileName = ofn.lpstrFile;

            return DialogResult.OK;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Builds a dummy in-memory Win32 dialog template just for enabling capture of dialog events.
        /// </summary>
        /// <returns>A pointer to an unmanaged memory buffer containing the dialog template</returns>
        private static IntPtr BuildDialogTemplate()
        {
            // We must place this child window inside the standard FileOpenDialog in order to get any
            // notifications sent to our hook procedure.  Also, this child window must contain at least
            // one control.  We make no direct use of the child window, or its control.

            // Set up the contents of the DLGTEMPLATE
            DLGTEMPLATE template = new DLGTEMPLATE();
            template.style = Constants.DS_3DLOOK | Constants.DS_CONTROL | Constants.WS_CHILD | Constants.WS_CLIPSIBLINGS | Constants.SS_NOTIFY;
            template.extendedStyle = Constants.WS_EX_CONTROLPARENT;
            template.numItems = 1;
            template.itemStyle = Constants.WS_CHILD;
            template.itemExtendedStyle = Constants.WS_EX_NOPARENTNOTIFY;
            template.itemClassHdr = 0xffff;
            template.itemClass = 0x0082;

            // Allocate some unmanaged memory for the template structure, and copy it in
            IntPtr ptrTemplate = Marshal.AllocCoTaskMem(MarshalHelper.SizeOf<DLGTEMPLATE>());
            Marshal.StructureToPtr(template, ptrTemplate, true);
            return ptrTemplate;
        }

        private int HookProc(IntPtr hdlg, uint msg, int wParam, int lParam)
        {
            switch (msg)
            {
                case Constants.WM_INITDIALOG:
                    if (CustomControl != null)
                    {
                        IntPtr parent = User32.GetParent(hdlg);

                        //Rectangle sr = activeScreen.Bounds;
                        RECT cr = new RECT();
                        User32.GetWindowRect(parent, ref cr);

                        //int x = (sr.Right + sr.Left - (cr.Right - cr.Left)) / 2;
                        //int y = (sr.Bottom + sr.Top - (cr.Bottom - cr.Top)) / 2;

                        // resizing parent window to fit self control (but don't relocates)
                        User32.SetWindowPos(parent, IntPtr.Zero, 0, 0, cr.Right - cr.Left, cr.Bottom - cr.Top + CustomControl.Height + 12, Constants.SWP_NOZORDER | Constants.SWP_NOMOVE);

                        //we need to find the label to position our new label under
                        IntPtr fileTypeWindow = User32.GetDlgItem(parent, 0x441);
                        IntPtr fontHandle = User32.SendMessage(fileTypeWindow, Constants.WM_GETFONT, IntPtr.Zero, IntPtr.Zero);

                        RECT fileTypeRect = new RECT();
                        User32.GetWindowRect(fileTypeWindow, ref fileTypeRect);

                        //now convert the label's screen co-ordinates to client co-ordinates
                        POINT point = new POINT();
                        point.x = fileTypeRect.Left;
                        point.y = fileTypeRect.Bottom;

                        User32.ScreenToClient(parent, ref point);

                        //create the label
                        if (!String.IsNullOrEmpty(CustomControlLabel))
                        {
                            labelHandle = User32.CreateWindowEx(0, "STATIC", "customControlLabel", Constants.WS_VISIBLE | Constants.WS_CHILD | Constants.WS_TABSTOP, point.x, point.y + 16, 80, 100, parent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                            User32.SetWindowText(labelHandle, CustomControlLabel);

                            User32.SendMessage(labelHandle, Constants.WM_SETFONT, fontHandle, IntPtr.Zero);
                        }
                        else
                            labelHandle = IntPtr.Zero;

                        //we now need to find the combo-box to position the new control
                        IntPtr fileComboWindow = User32.GetDlgItem(parent, 0x470);
                        fileTypeRect = new RECT();
                        User32.GetWindowRect(fileComboWindow, ref fileTypeRect);

                        point = new POINT();
                        point.x = fileTypeRect.Left;
                        point.y = fileTypeRect.Bottom;
                        User32.ScreenToClient(parent, ref point);

                        POINT rightPoint = new POINT();
                        rightPoint.x = fileTypeRect.Right;
                        rightPoint.y = fileTypeRect.Top;

                        User32.ScreenToClient(parent, ref rightPoint);

                        // winapi combo:
                        //we create the new combobox
                        //IntPtr comboHandle = User32.CreateWindowEx(0, "ComboBox", "mycombobox", Constants.WS_VISIBLE | Constants.WS_CHILD | Constants.CBS_HASSTRINGS | Constants.CBS_DROPDOWNLIST | Constants.WS_TABSTOP, point.X, point.Y + 8, rightPoint.X - point.X, 100, parent, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                        //User32.SendMessage(comboHandle, Constants.WM_SETFONT, fontHandle, IntPtr.Zero);

                        ////and add the encodings we want to offer
                        //User32.SendMessage(comboHandle, Constants.CB_ADDSTRING, IntPtr.Zero, "UTF-8");
                        //User32.SendMessage(comboHandle, Constants.CB_ADDSTRING, IntPtr.Zero, "UTF-8 with preamble");
                        //User32.SendMessage(comboHandle, Constants.CB_ADDSTRING, IntPtr.Zero, "Unicode");
                        //User32.SendMessage(comboHandle, Constants.CB_ADDSTRING, IntPtr.Zero, "ANSI");
                        //// selecting default item
                        //User32.SendMessage(comboHandle, Constants.CB_SETCURSEL, new IntPtr(0), IntPtr.Zero);

                        CustomControl.Location = new Point(point.x, point.y + 8);
                        if (CustomControlAutoSize)
                            CustomControl.Width = fileTypeRect.Right - fileTypeRect.Left;
                        User32.SetParent(CustomControl.Handle, parent);
                    }
                    else
                    {
                        ptrTemplate = IntPtr.Zero;
                        labelHandle = IntPtr.Zero;
                    }
                    isInitialized = true;
                    break;
                case Constants.WM_DESTROY:
                    DestroyHandles();
                    break;
                case Constants.WM_NOTIFY:

                    //we need to intercept the CDN_FILEOK message
                    //which is sent when the user selects a filename

                    NMHDR nmhdr = MarshalHelper.PtrToStructure<NMHDR>(new IntPtr(lParam));

                    // OK pressed
                    if (nmhdr.Code == Constants.CDN_FILEOK)
                    {
                        //a file has been selected
                        //we need to get the encoding
                        // winapi combo:
                        //EncodingType = (int)User32.SendMessage(m_ComboHandle, Constants.CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero);

                        // WinForms combo:
                        //m_EncodingType = (EncodingType)cmbEncoding.SelectedIndex;
                    }
                    // file type changed
                    else if (nmhdr.Code == Constants.CDN_TYPECHANGE)
                    {
                        IntPtr parent = User32.GetParent(hdlg);
                        IntPtr fileComboWindow = User32.GetDlgItem(parent, 0x470);
                        int selectedIndex = User32.SendMessage(fileComboWindow, Constants.CB_GETCURSEL, IntPtr.Zero, IntPtr.Zero).ToInt32();
                        string? extension = null;
                        if (fileTypes?.Length >= selectedIndex * 2)
                            extension = fileTypes[selectedIndex * 2 + 1];
                        FilterIndex = selectedIndex + 1;
                        OnFileTypeChanged(new FileTypeChangedEventArgs(selectedIndex, extension));
                    }
                    // selected file changed
                    else if (nmhdr.Code == Constants.CDN_SELCHANGE)
                    {
                        IntPtr ptrFileName = Marshal.AllocHGlobal(0x2000);
                        try
                        {
                            IntPtr parent = User32.GetParent(hdlg);
                            User32.SendMessage(parent, Constants.CDM_GETFILEPATH, new IntPtr(0x2000), ptrFileName);
                            string fileName = Marshal.PtrToStringAuto(ptrFileName)!;
                            OnSelectedFileChanged(new SelectedFileChangedEventArgs(fileName));
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptrFileName);
                        }
                    }

                    break;
                case Constants.WM_SIZE:
                    if (CustomControl != null && CustomControlAutoSize)
                    {
                        IntPtr parent = User32.GetParent(hdlg);
                        IntPtr fileComboWindow = User32.GetDlgItem(parent, 0x470);
                        RECT fileComboRect = new RECT();
                        User32.GetWindowRect(fileComboWindow, ref fileComboRect);

                        //// winapi control:
                        //FindScreenToClient(parent, ref aboveRect);
                        //User32.SetWindowPos(m_ComboHandle, IntPtr.Zero, 0, 0, aboveRect.Right - aboveRect.Left, aboveRect.Bottom - aboveRect.Top, Constants.SWP_NOMOVE);

                        // WinForms control:
                        CustomControl.Width = fileComboRect.Right - fileComboRect.Left;
                    }
                    break;
            }
            return 0;
        }

        private void DestroyHandles()
        {
            //destroy the handles we have created
            // winapi combo:
            //if (m_ComboHandle != IntPtr.Zero)
            //{
            //    User32.DestroyWindow(m_ComboHandle);
            //}

            if (ptrTemplate != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(ptrTemplate);
                ptrTemplate = IntPtr.Zero;
            }
            //if (CustomControl != null)
            //    CustomControl.Parent = null;

            if (labelHandle != IntPtr.Zero)
            {
                User32.DestroyWindow(labelHandle);
                labelHandle = IntPtr.Zero;
            }
            isInitialized = false;
        }

        [SuppressMessage("ReSharper", "UnusedParameter.Local", Justification = "Dispose pattern")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Dispose pattern")]
        private void Dispose(bool disposing)
        {
            if (isInitialized)
                DestroyHandles();
        }

        private void OnFileTypeChanged(FileTypeChangedEventArgs e) => FileTypeChanged?.Invoke(this, e);

        private void OnSelectedFileChanged(SelectedFileChangedEventArgs e) => SelectedFileChanged?.Invoke(this, e);

        //private void FindScreenToClient(IntPtr parent, ref RECT rect)
        //{
        //    POINT topLeft;
        //    POINT bottomRight;
        //    topLeft.x = rect.Left;
        //    topLeft.y = rect.Top;
        //    User32.ScreenToClient(parent, ref topLeft);
        //    bottomRight.x = rect.Right;
        //    bottomRight.y = rect.Bottom;
        //    User32.ScreenToClient(parent, ref bottomRight);
        //    rect.Top = topLeft.y;
        //    rect.Left = topLeft.x;
        //    rect.Bottom = bottomRight.y;
        //    rect.Right = bottomRight.x;
        //}

        #endregion

        #endregion
    }
}
