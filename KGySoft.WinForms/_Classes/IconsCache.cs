#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: IconsCache.cs
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
using System.Drawing;

using KGySoft.Collections;
using KGySoft.CoreLibraries;
using KGySoft.Drawing;

#endregion

namespace KGySoft.WinForms
{
    internal static class IconsCache
    {
        #region Fields

        // Notes:
        // - Not using the loader delegate because the key is calculated in the GetCachedBitmap method.
        // - Size is turned into ulong to avoid slow value compare in .NET Framework where Size does not implement IEquatable.
        // - Not enabling DisposeDroppedValues because an image may be used after dropping it from the cache
        //   (because unlike in VisualStyleHelper or ColorExtensions, the returned cache items may be stored by the caller).
        //   So relying on the finalizer to dispose the images, which is actually not worse than using non-disposable objects with finalizers like WPF images or weak references.
        private static readonly Cache<(string, ulong), Bitmap> imagesCache = new Cache<(string, ulong), Bitmap>(8);

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