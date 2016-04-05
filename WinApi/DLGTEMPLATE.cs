using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace KGySoft.Controls.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DLGTEMPLATE
    {
        // DLGTEMPLATE
        public UInt32 style;
        public UInt32 extendedStyle;
        public UInt16 numItems;
        public Int16 x;
        public Int16 y;
        public Int16 cx;
        public Int16 cy;
        public Int16 reservedMenu;
        public Int16 reservedClass;
        public Int16 reservedTitle;
        // DLGITEMTEMPLATE
        public UInt32 itemStyle;
        public UInt32 itemExtendedStyle;
        public Int16 itemX;
        public Int16 itemY;
        public Int16 itemCx;
        public Int16 itemCy;
        public UInt16 itemId;
        // itemdata
        public UInt16 itemClassHdr;
        public Int16 itemClass;
        public Int16 itemText;
        public Int16 itemData;
    };
}
