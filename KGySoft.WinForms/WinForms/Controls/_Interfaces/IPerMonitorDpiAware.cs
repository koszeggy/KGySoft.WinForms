#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IPerMonitorDpiAware.cs
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
    internal interface IPerMonitorDpiAware
    {
        #region Methods

        void ParentFormDpiChanging();
        void ParentFormDpiChanged();

        #endregion
    }
}