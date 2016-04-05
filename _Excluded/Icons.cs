using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using KGySoft.Controls.WinApi;
using KGySoft.Libraries;

namespace KGySoft.Controls
{
    /// <summary>
    /// Provides general icons in multi resolution (32x32 and 16x16). Unlike <see cref="SystemIcons"/>, these icons should be disposed when not used.
    /// </summary>
    public static class Icons
    {
        private enum IconId
        {
            Application = 2,
            Question = 23,
            Shield = 77,
            Warning = 78,
            Information = 79,
            Error = 80,
            SecurityQuestion = -1,
            SecurityWarning = -2,
            SecurityError = -3,
            SecuritySuccess = -4,
        }

        private static readonly Dictionary<IconId, RawIcon> iconCache = new Dictionary<IconId, RawIcon>(EnumComparer<IconId>.Comparer);

        /// <summary>
        /// Tries to get the system icon by id. When there is no icon defined for provided <paramref name="id"/>,
        /// or Windows version is below Vista, this method returns <see langword="null"/>.
        /// On Windows XP use the predefined property members to retrieve system icons.
        /// </summary>
        /// <param name="id">Id of the icon to retrieve.</param>
        /// <returns>An <see cref="Icon"/> instance containing a small and large icon when an icon belongs to <paramref name="id"/>, or <see langword="null"/>,
        /// when no icon found, or Windows version is below Vista.</returns>
        public static Icon TryGetSystemIconById(int id)
        {
            if (id < 0 || !WindowsUtils.IsVistaOrLater)
                return null;

            RawIcon rawIcon;
            if (iconCache.TryGetValue((IconId)id, out rawIcon))
                return rawIcon.ToIcon();

            SHSTOCKICONINFO iconInfo = new SHSTOCKICONINFO();
            iconInfo.cbSize = (uint)Marshal.SizeOf(typeof(SHSTOCKICONINFO));

            SHGSI flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_LARGEICON;
            if (Shell32.SHGetStockIconInfo(id, flags, ref iconInfo) != 0)
                return null;

            Icon icon = Icon.FromHandle(iconInfo.hIcon);
            rawIcon = new RawIcon(icon);
            User32.DestroyIcon(iconInfo.hIcon);

            flags = SHGSI.SHGSI_ICON | SHGSI.SHGSI_SMALLICON;
            if (Shell32.SHGetStockIconInfo(id, flags, ref iconInfo) != 0)
            {
                iconCache.Add((IconId)id, rawIcon);
                return rawIcon.ToIcon();
            }

            icon = Icon.FromHandle(iconInfo.hIcon);
            rawIcon.Add(icon);
            User32.DestroyIcon(iconInfo.hIcon);
            iconCache.Add((IconId)id, rawIcon);
            return rawIcon.ToIcon();
        }

        /// <summary>
        /// Gets an <see cref="Icon"/> that contains a large and a small
        /// Information icon. The returned instance should be disposed when not used anymore.
        /// </summary>
        public static Icon Information
        {
            get
            {
                IconId id = IconId.Information;
                RawIcon icon;
                if (iconCache.TryGetValue(id, out icon))
                    return icon.ToIcon();

                Icon result = TryGetSystemIconById((int)id);
                if (result != null)
                    return result;

                return RetrieveSystemIcon(id, SystemIcons.Information);
            }
        }

        private static Icon RetrieveSystemIcon(IconId id, Icon icon)
        {
            Bitmap imageLarge = icon.ToAlphaBitmap();
            Bitmap imageSmall = imageLarge.Resize(new Size(16, 16));
            RawIcon cacheItem = new RawIcon();
            cacheItem.Add(imageLarge);
            cacheItem.Add(imageSmall);
            iconCache[id] = cacheItem;
            imageLarge.Dispose();
            imageSmall.Dispose();
            return cacheItem.ToIcon();
        }
    }
}
