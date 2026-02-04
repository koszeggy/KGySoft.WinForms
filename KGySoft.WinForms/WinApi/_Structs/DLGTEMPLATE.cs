#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: DLGTEMPLATE.cs
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
using System.Runtime.InteropServices;

#endregion


namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DLGTEMPLATE
    {
        #region Fields

        // DLGTEMPLATE
        internal UInt32 style;
        internal UInt32 extendedStyle;
        internal UInt16 numItems;
        internal Int16 x;
        internal Int16 y;
        internal Int16 cx;
        internal Int16 cy;
        internal Int16 reservedMenu;
        internal Int16 reservedClass;
        internal Int16 reservedTitle;
        
        // DLGITEMTEMPLATE
        internal UInt32 itemStyle;
        internal UInt32 itemExtendedStyle;
        internal Int16 itemX;
        internal Int16 itemY;
        internal Int16 itemCx;
        internal Int16 itemCy;
        internal UInt16 itemId;
     
        // itemdata
        internal UInt16 itemClassHdr;
        internal Int16 itemClass;
        internal Int16 itemText;
        internal Int16 itemData;

        #endregion
    }
}
