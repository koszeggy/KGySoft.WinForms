#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ISupportButtonAdapter.cs
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

using KGySoft.WinForms.Controls;

#endregion

namespace KGySoft.WinForms
{
    internal interface ISupportButtonAdapter
    {
        #region Properties

        ButtonBaseAdapter Adapter { get; }
        bool ShowFocusCues { get; }
        bool ShowKeyboardCues { get; }

        #endregion
    }
}