#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: COMBOBOXPARTS.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal enum COMBOBOXPARTS
    {
        CP_COMPATIBLEBACKGROUND = 0, // Actually not defined, but works even in XP
        CP_DROPDOWNBUTTON = 1,
        CP_BACKGROUND = 2,
        CP_TRANSPARENTBACKGROUND = 3,
        CP_BORDER = 4,
        CP_READONLY = 5,
        CP_DROPDOWNBUTTONRIGHT = 6,
        CP_DROPDOWNBUTTONLEFT = 7,
        CP_CUEBANNER = 8,
        CP_DROPDOWNITEM = 9,
    };
}