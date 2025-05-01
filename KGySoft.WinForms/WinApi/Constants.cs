using System;
using System.Runtime.InteropServices;

namespace KGySoft.WinForms.WinApi
{
    static class Constants
    {
        internal const uint HTERROR = unchecked((uint)-2);
        internal const uint HTTRANSPARENT = unchecked((uint)-1);
        internal const int HTNOWHERE = 0;
        internal const int HTCLIENT = 1;
        internal const int HTCAPTION = 2;
        internal const int HTSYSMENU = 3;
        internal const int HTGROWBOX = 4;
        internal const int HTSIZE = HTGROWBOX;
        internal const int HTMENU = 5;
        internal const int HTHSCROLL = 6;
        internal const int HTVSCROLL = 7;
        internal const int HTMINBUTTON = 8;
        internal const int HTMAXBUTTON = 9;
        internal const int HTLEFT = 10;
        internal const int HTRIGHT = 11;
        internal const int HTTOP = 12;
        internal const int HTTOPLEFT = 13;
        internal const int HTTOPRIGHT = 14;
        internal const int HTBOTTOM = 15;
        internal const int HTBOTTOMLEFT = 16;
        internal const int HTBOTTOMRIGHT = 17;
        internal const int HTBORDER = 18;
        internal const int HTREDUCE = HTMINBUTTON;
        internal const int HTZOOM = HTMAXBUTTON;
        internal const int HTSIZEFIRST = HTLEFT;
        internal const int HTSIZELAST = HTBOTTOMRIGHT;
        internal const int HTOBJECT = 19;

        internal const int SWP_NOSIZE = 0x0001;
        internal const int SWP_NOMOVE = 0x0002;
        internal const int SWP_NOZORDER = 0x0004;
        internal const int SWP_NOREDRAW = 0x0008;
        internal const int SWP_NOACTIVATE = 0x0010;
        internal const int SWP_FRAMECHANGED = 0x0020;  // The frame changed: send
        internal const int SWP_DRAWFRAME = SWP_FRAMECHANGED;
        internal const int SWP_SHOWWINDOW = 0x0040;
        internal const int SWP_HIDEWINDOW = 0x0080;
        internal const int SWP_NOCOPYBITS = 0x0100;
        internal const int SWP_NOOWNERZORDER = 0x0200;  // Don't do owner Z ordering
        internal const int SWP_NOREPOSITION = SWP_NOOWNERZORDER;
        internal const int SWP_NOSENDCHANGING = 0x0400;  // Don't send

        internal const int WM_DESTROY = 0x2;
        internal const int WM_SIZE = 5;
        internal const int WM_SETTEXT = 0xc;
        internal const int WM_GETTEXT = 0xd;
        internal const int WM_GETTEXTLENGTH = 0xe;
        internal const int WM_PAINT = 0x000F;
        internal const int WM_SETCURSOR = 0x0020;
        internal const int WM_SETICON = 0x0080;
        internal const int WM_NCCALCSIZE = 0x0083;
        internal const int WM_NCHITTEST = 0x0084;
        internal const int WM_NCLBUTTONDOWN = 0x00A1;
        internal const int WM_NCLBUTTONUP = 0x00A2;
        internal const int WM_NCMOUSEMOVE = 0x00A0;
        internal const int WM_NCPAINT = 0x0085;
        internal const int WM_LBUTTONDOWN = 0x0201;
        internal const int WM_MOUSEMOVE = 0x0200;
        internal const int WM_INITDIALOG = 0x110;
        internal const int WM_COMMAND = 0x111;
        internal const int WM_TIMER = 0x113;
        internal const int WM_SETFONT = 0x0030;
        internal const int WM_GETFONT = 0x0031;
        internal const int WM_NOTIFY = 0x004E;
        internal const int WM_KEYDOWN = 0x100;
        internal const int WM_CUT = 0x0300;
        internal const int WM_COPY = 0x0301;
        internal const int WM_PASTE = 0x0302;
        internal const int WM_CLEAR = 0x0303;
        internal const int WM_UNDO = 0x0304;
        internal const int WM_USER = 0x0400;
        internal const int WM_CAPTURECHANGED = 0x0215;

        internal const int ICON_SMALL = 0;
        internal const int ICON_BIG = 1;

        internal const int EM_GETSEL = 0x00B0,
            EM_SETSEL = 0x00B1,
            EM_SCROLL = 0x00B5,
            EM_SCROLLCARET = 0x00B7,
            EM_GETMODIFY = 0x00B8,
            EM_SETMODIFY = 0x00B9,
            EM_GETLINECOUNT = 0x00BA,
            EM_REPLACESEL = 0x00C2,
            EM_GETLINE = 0x00C4,
            EM_LIMITTEXT = 0x00C5,
            EM_CANUNDO = 0x00C6,
            EM_UNDO = 0x00C7,
            EM_SETPASSWORDCHAR = 0x00CC,
            EM_GETPASSWORDCHAR = 0x00D2,
            EM_EMPTYUNDOBUFFER = 0x00CD,
            EM_SETREADONLY = 0x00CF,
            EM_SETMARGINS = 0x00D3,
            EM_POSFROMCHAR = 0x00D6,
            EM_CHARFROMPOS = 0x00D7,
            EM_LINEFROMCHAR = 0x00C9,
            EM_GETFIRSTVISIBLELINE = 0x00CE,
            EM_LINEINDEX = 0x00BB;

        internal const uint DS_3DLOOK = 4;
        internal const uint DS_ABSALIGN = 1;
        internal const uint DS_CENTER = 0x800;
        internal const uint DS_CENTERMOUSE = 4096;
        internal const uint DS_CONTEXTHELP = 0x2000;
        internal const uint DS_CONTROL = 0x400;
        internal const uint DS_FIXEDSYS = 8;
        internal const uint DS_LOCALEDIT = 32;
        internal const uint DS_MODALFRAME = 128;
        internal const uint DS_NOFAILCREATE = 16;
        internal const uint DS_NOIDLEMSG = 256;
        internal const uint DS_SETFONT = 64;
        internal const uint DS_SETFOREGROUND = 512;
        internal const uint DS_SYSMODAL = 2;

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
        internal const int OFN_FILEMUSTEXIST = 0x00001000;
        internal const int OFN_HIDEREADONLY = 0x00000004;
        internal const int OFN_CREATEPROMPT = 0x00002000;
        internal const int OFN_NOTESTFILECREATE = 0x00010000;
        internal const int OFN_OVERWRITEPROMPT = 0x00000002;
        internal const int OFN_PATHMUSTEXIST = 0x00000800;
        internal const int OFN_ENABLESIZING = 0x00800000;
        internal const int OFN_FORCESHOWHIDDEN = 0x10000000;

        internal const int CBS_DROPDOWNLIST = 0x0003;
        internal const int CBS_HASSTRINGS = 0x0200;
        internal const int CB_ADDSTRING = 0x0143;
        internal const int CB_SETCURSEL = 0x014E;
        internal const int CB_GETCURSEL = 0x0147;

        internal const int CDN_FIRST = -601;
        internal const int CDN_INITDONE = CDN_FIRST;
        internal const int CDN_SELCHANGE = CDN_FIRST - 1;
        internal const int CDN_FOLDERCHANGE = CDN_FIRST - 2;
        internal const int CDN_SHAREVIOLATION = CDN_FIRST - 3;
        internal const int CDN_HELP = CDN_FIRST - 4;
        internal const int CDN_FILEOK = CDN_FIRST - 5;
        internal const int CDN_TYPECHANGE = CDN_FIRST - 6;

        internal const int CDM_FIRST = WM_USER + 100;
        internal const int CDM_GETFILEPATH = CDM_FIRST + 0x0001;

        //internal const int IDOK = 1;
        //internal const int IDCANCEL = 2;
        //internal const int IDABORT = 3;
        //internal const int IDRETRY = 4;
        //internal const int IDIGNORE = 5;
        //internal const int IDYES = 6;
        //internal const int IDNO = 7;
        //internal const int IDCLOSE = 8;

        internal const int ACTCTX_FLAG_ASSEMBLY_RESOURCE_NAME_VALID = 0x008;

        internal const int TDI_MAIN = 0;
        internal const int TDI_FOOTER = 1;

        //Button styles
        public const int BS_COMMANDLINK = 0x0000000E;
        public const int BS_SPLITBUTTON = 0x0000000C;

        private const int BCM_FIRST = 0x1600;
        internal const int BCM_GETIDEALSIZE = (BCM_FIRST + 0x0001);
        internal const int BCM_SETNOTE = (BCM_FIRST + 0x0009);
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C);

        internal const int BM_SETIMAGE = 0x00F7;

        internal const int IDC_HAND = 32649;
        internal const int TMT_TRANSITIONDURATIONS = 6000;

        internal const int TDLG_EXPANDOBUTTON = 13;
        internal const int TDLG_MAININSTRUCTIONPANE = 2;

        internal const int TEXT_MAININSTRUCTION = 1;

        internal const int TMT_FONT = 210;

        internal const int PBM_SETVALUE = WM_USER + 2;
        internal const int PBM_SETMARQUEE = WM_USER + 10;
        internal const int PBM_SETSTATE = WM_USER + 16;
        internal const int PBM_GETSTATE = WM_USER + 17;
    }
}
