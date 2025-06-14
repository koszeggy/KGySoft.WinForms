#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconsHelper.cs
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

using System.Drawing;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;

#endregion

namespace KGySoft.WinForms
{
    internal static class IconsHelper
    {
        #region Fields

        #region Internal Fields

        internal static readonly Size LargeIconReferenceSize = new Size(32, 32);
        internal static readonly Size SmallIconReferenceSize = new Size(16, 16);

        #endregion

        #region Private Fields

        // Notes:
        // - Not using Cache<,> with a loader delegate because the key is calculated in the GetCachedBitmap method.
        // - Size is turned into ulong to avoid slow value compare in .NET Framework where Size does not implement IEquatable.
        private static readonly ThreadSafeDictionary<(string, ulong), Bitmap> imagesCache = new();
        
        #endregion

        #endregion

        #region Methods

        internal static Bitmap GetCachedBitmap(this Icon icon, string name, Size size)
        {
            var key = (name, (ulong)(uint)size.Width << 32 | (uint)size.Height);
            if (imagesCache.TryGetValue(key, out Bitmap? bitmap))
                return bitmap;

            using var resizedIcon = icon.Resize(size);
            bitmap = resizedIcon.ExtractBitmap(0)!;
            imagesCache[key] = bitmap;
            return bitmap;
        }

        internal static Bitmap GetCachedBitmap(this Icon icon, string name, Size size, ScalingMode scalingMode)
        {
            var key = ($"{name}_{Enum<ScalingMode>.ToString(scalingMode)}", (ulong)(uint)size.Width << 32 | (uint)size.Height);
            if (imagesCache.TryGetValue(key, out Bitmap? bitmap))
                return bitmap;

            using var resizedIcon = icon.Resize(size);
            bitmap = resizedIcon.ExtractBitmap(0)!;
            imagesCache[key] = bitmap;
            return bitmap;
        }

        #endregion
    }
}