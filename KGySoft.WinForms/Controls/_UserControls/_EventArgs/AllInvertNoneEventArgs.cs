#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: AllInvertNoneEventArgs.cs
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

namespace KGySoft.WinForms.Controls
{
    [Obsolete("This class belongs to the obsoleted ucAllInvertNone class")]
    public class AllInvertNoneEventArgs : EventArgs
    {
        #region Fields

        InvertButtonTypes buttonType;

        #endregion

        #region Properties

        public InvertButtonTypes ButtonType
        {
            get { return buttonType; }
        }

        #endregion

        #region Constructors

        public AllInvertNoneEventArgs(InvertButtonTypes buttonType)
        {
            this.buttonType = buttonType;
        }

        #endregion
    }
}