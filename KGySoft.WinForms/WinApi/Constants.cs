#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Constants.cs
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

#endregion


namespace KGySoft.WinForms.WinApi
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    [SuppressMessage("ReSharper", "IdentifierTypo", Justification = "WinAPI")]
    internal static class Constants
    {
        #region Constants

        internal const string ThemeClassButton = "BUTTON";
        internal const string ThemeClassTaskDialog = "TASKDIALOG";

        internal const int S_OK = 0;

        internal const uint HTERROR = unchecked((uint)-2);
        internal const int HTGROWBOX = 4;
        internal const int HTSIZE = HTGROWBOX;

        internal const int SWP_NOSIZE = 0x0001;
        internal const int SWP_NOMOVE = 0x0002;
        internal const int SWP_NOZORDER = 0x0004;
        internal const int SWP_NOACTIVATE = 0x0010;
        internal const int SWP_FRAMECHANGED = 0x0020;  // The frame changed: send
        internal const int SWP_DRAWFRAME = SWP_FRAMECHANGED;

        internal const int WM_DESTROY = 0x2;
        internal const int WM_SIZE = 5;
        internal const int WM_SETTEXT = 0xc;
        internal const int WM_PAINT = 0x000F;
        internal const int WM_SETICON = 0x0080;
        internal const int WM_NCCALCSIZE = 0x0083;
        internal const int WM_NCHITTEST = 0x0084;
        internal const int WM_NCPAINT = 0x0085;
        internal const int WM_INITDIALOG = 0x110;
        internal const int WM_TIMER = 0x113;
        internal const int WM_SETFONT = 0x0030;
        internal const int WM_GETFONT = 0x0031;
        internal const int WM_NOTIFY = 0x004E;
        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_LBUTTONDBLCLK = 0x0203;
        internal const int WM_CUT = 0x0300;
        internal const int WM_PASTE = 0x0302;
        internal const int WM_CLEAR = 0x0303;
        internal const int WM_UNDO = 0x0304;
        internal const int WM_USER = 0x0400;
        internal const int WM_DPICHANGED_BEFOREPARENT = 0x02E2;
        internal const int WM_DPICHANGED_AFTERPARENT = 0x02E3;

        internal const int ICON_SMALL = 0;
        internal const int ICON_BIG = 1;

        internal const int EM_GETFIRSTVISIBLELINE = 0x00CE;

        internal const uint DS_3DLOOK = 4;
        internal const uint DS_CONTROL = 0x400;

        internal const uint WS_CLIPSIBLINGS = 0x04000000;
        internal const uint WS_VISIBLE = 0x10000000;
        internal const uint WS_CHILD = 0x40000000;
        internal const uint WS_TABSTOP = 0x00010000;

        internal const uint WS_EX_CONTROLPARENT = 0x10000;
        internal const uint WS_EX_NOPARENTNOTIFY = 4;

        internal const uint SS_NOTIFY = 256;

        internal const int OFN_ENABLETAMPLATEHANDLE = 0x00000080;
        internal const int OFN_ENABLEHOOK = 0x00000020;
        internal const int OFN_EXPLORER = 0x00080000;
        internal const int OFN_HIDEREADONLY = 0x00000004;
        internal const int OFN_NOTESTFILECREATE = 0x00010000;
        internal const int OFN_OVERWRITEPROMPT = 0x00000002;
        internal const int OFN_PATHMUSTEXIST = 0x00000800;
        internal const int OFN_ENABLESIZING = 0x00800000;
        internal const int OFN_FORCESHOWHIDDEN = 0x10000000;

        internal const int CB_GETCURSEL = 0x0147;

        internal const int CDN_FIRST = -601;
        internal const int CDN_SELCHANGE = CDN_FIRST - 1;
        internal const int CDN_FILEOK = CDN_FIRST - 5;
        internal const int CDN_TYPECHANGE = CDN_FIRST - 6;

        internal const int CDM_FIRST = WM_USER + 100;
        internal const int CDM_GETFILEPATH = CDM_FIRST + 0x0001;

        internal const int ACTCTX_FLAG_ASSEMBLY_RESOURCE_NAME_VALID = 0x008;

        internal const int TDI_MAIN = 0;
        internal const int TDI_FOOTER = 1;

        internal const int BS_COMMANDLINK = 0x0000000E;

        internal const int BCM_FIRST = 0x1600;
        internal const int BCM_GETIDEALSIZE = (BCM_FIRST + 0x0001);
        internal const int BCM_SETNOTE = (BCM_FIRST + 0x0009);
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C);

        internal const int BM_SETIMAGE = 0x00F7;

        internal const int IDC_HAND = 32649;
        internal const int TMT_TRANSITIONDURATIONS = 6000;

        internal const int TDLG_EXPANDOBUTTON = 13;
        internal const int TDLG_MAININSTRUCTIONPANE = 2;

        internal const int TMT_FONT = 210;
        internal const int TMT_COLOR = 3803;

        internal const int PBM_SETMARQUEE = WM_USER + 10;
        internal const int PBM_SETSTATE = WM_USER + 16;

        internal const nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = -3;
        internal const nint DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = -4;

        internal const uint MONITOR_DEFAULTTONEAREST = 2;

        #endregion
    }
}
