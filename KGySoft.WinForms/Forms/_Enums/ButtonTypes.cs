#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ButtonTypes.cs
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

namespace KGySoft.WinForms.Forms
{
    [Obsolete("This type is used by the obsoleted AdvancedMessageDialog")]
    public enum ButtonTypes
    {
        // Standard types with DialogResult return
        OK,
        YesNo,
        YesNoCancel,
        OKCancel,
        RetryCancel,
        AbortRetryIgnore,

        // Special types with DialogResult.None return
        Closewin,                   // Close button with door icon
        ClosewinSendreport,         // like above + error log sending button
        ClosewinSendreportCloseapp  // like above + close application button
    }
}