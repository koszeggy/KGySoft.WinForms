#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: OFNHookProcDelegate.cs
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

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal delegate int OFNHookProcDelegate(IntPtr hdlg, uint msg, int wParam, int lParam);
}