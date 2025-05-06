#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: NMHDR.cs
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

using System;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// Contains information about a notification message.
    /// </summary>
    internal struct NMHDR
    {
        #region Fields

        public IntPtr HwndFrom;

        public IntPtr IdFrom;

        public int Code;

        #endregion
    }
}