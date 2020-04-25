using System;
using System.Runtime.InteropServices;

namespace KGySoft.WinForms.WinApi
{
    internal delegate int OFNHookProcDelegate(IntPtr hdlg, uint msg, int wParam, int lParam);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct OPENFILENAME
    {
        public int lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrFilter;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrCustomFilter;
        public int nMaxCustFilter;
        public int nFilterIndex;

        /// <summary>
        /// File name edit box
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]        
        public string lpstrFile;

        public int nMaxFile;

        /// <summary>
        /// The file name and extension (without path information) of the selected file. This member can be NULL.
        /// </summary>
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrFileTitle;
        public int nMaxFileTitle;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrInitialDir;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrTitle;
        public int Flags;
        public short nFileOffset;
        public short nFileExtension;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpstrDefExt;
        public int lCustData;
        public OFNHookProcDelegate lpfnHook;
        [MarshalAs(UnmanagedType.LPTStr)]
        public string lpTemplateName;
        //only if on nt 5.0 or higher
        public int pvReserved;
        public int dwReserved;
        public int FlagsEx;
    }
}
