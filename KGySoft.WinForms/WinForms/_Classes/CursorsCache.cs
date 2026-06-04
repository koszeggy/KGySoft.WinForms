#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: CursorsCache.cs
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

using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

using KGySoft.Collections;
using KGySoft.Drawing;

#endregion

namespace KGySoft.WinForms
{
    internal static class CursorsCache
    {
        #region Nested classes

        private class CursorInfo
        {
            #region Constants

            private const int referenceSize = 20;

            #endregion

            #region Fields


            private readonly Icon icon;

            // Not a cache, because items are never dropped. CursorHandle is part of the value to prevent disposing it.
            private readonly ThreadSafeDictionary<int, (CursorHandle Handle, Cursor Cursor)> createdCursors = new();

            #endregion

            #region Constructors

            internal CursorInfo(Icon icon) => this.icon = icon;

            #endregion

            #region Methods
            
            #region Internal Methods

            internal Cursor Get(Control control)
            {
                int size = control.ScaleWidth(referenceSize);
                return createdCursors.GetOrAdd(size, Create).Cursor;
            }

            #endregion

            #region Private Methods

            (CursorHandle Handle, Cursor Cursor) Create(int size)
            {
                using Icon image = icon.ExtractNearestIcon(new Size(size, size), PixelFormat.Format32bppArgb);
                CursorHandle handle = image.ToCursorHandle(new Point(image.Width >> 1, image.Height >> 1));
                return (handle, new Cursor(handle));
            }

            #endregion

            #endregion
        }

        #endregion

        #region Fields

        private static CursorInfo? handOpen;
        private static CursorInfo? handGrab;

        #endregion

        #region Methods

        #region Internal Methods

        internal static Cursor HandOpen(Control control) => (handOpen ??= GetCreateCursorInfo())?.Get(control) ?? Cursors.Hand;
        internal static Cursor HandGrab(Control control) => (handGrab ??= GetCreateCursorInfo())?.Get(control) ?? Cursors.NoMove2D;

        #endregion

        #region Private Methods

        private static CursorInfo? GetCreateCursorInfo([CallerMemberName]string resourceName = null!) => OSHelper.IsWindows
            ? new CursorInfo((Icon)Properties.Resources.ResourceManager.GetObject(resourceName, CultureInfo.InvariantCulture)!)
            : null;

        #endregion

        #endregion
    }
}
