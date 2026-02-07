#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: EXPANDOBUTTONSTATES.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

using System.Diagnostics.CodeAnalysis;

namespace KGySoft.WinForms.WinApi
{
    [SuppressMessage("ReSharper", "InconsistentNaming", Justification = "WinAPI")]
    internal enum EXPANDOBUTTONSTATES
    {
        TDLGEBS_NORMAL = 1,
        TDLGEBS_HOVER = 2,
        TDLGEBS_PRESSED = 3,
        TDLGEBS_EXPANDEDNORMAL = 4,
        TDLGEBS_EXPANDEDHOVER = 5,
        TDLGEBS_EXPANDEDPRESSED = 6,
    };
}
