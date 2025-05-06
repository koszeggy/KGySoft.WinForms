#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: BP_ANIMATIONSTYLE.cs
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
    [Flags]
    internal enum BP_ANIMATIONSTYLE
    {
        BPAS_NONE = 0,
        BPAS_LINEAR = 1,
        BPAS_CUBIC = 2,
        BPAS_SINE = 3
    }
}