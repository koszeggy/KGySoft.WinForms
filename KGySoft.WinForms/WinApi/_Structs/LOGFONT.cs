#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: LOGFONT.cs
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

using System.Runtime.InteropServices;

#endregion

namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct LOGFONT
    {
        #region Constants

        private const int LF_FACESIZE = 32;

        #endregion

        #region Fields

        internal int lfHeight;
        internal int lfWidth;
        internal int lfEscapement;
        internal int lfOrientation;
        internal int lfWeight;
        internal byte lfItalic;
        internal byte lfUnderline;
        internal byte lfStrikeOut;
        internal byte lfCharSet;
        internal byte lfOutPrecision;
        internal byte lfClipPrecision;
        internal byte lfQuality;
        internal byte lfPitchAndFamily;

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = LF_FACESIZE)]
        internal string lfFaceName;

        #endregion
    }
}