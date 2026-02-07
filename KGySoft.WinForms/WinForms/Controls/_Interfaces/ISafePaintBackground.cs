#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISafePaintBackground.cs
///////////////////////////////////////////////////////////////////////////////
//  Copyright (C) KGy SOFT, 2005-2026 - All Rights Reserved
//
//  You should have received a copy of the LICENSE file at the top-level
//  directory of this distribution.
//
//  Please refer to the LICENSE file if you want to use this source code.
///////////////////////////////////////////////////////////////////////////////

#endregion

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Just to indicate that a control uses the workaround for the background image painting issue of .NET Core - see https://github.com/dotnet/winforms/issues/13784
    /// </summary>
    internal interface ISafePaintBackground;
}
