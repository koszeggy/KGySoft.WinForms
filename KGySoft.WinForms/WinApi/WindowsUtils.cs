using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using KGySoft.Reflection;

namespace KGySoft.WinForms.WinApi
{
    internal static class WindowsUtils
    {
        private static bool? isVistaOrLater;
        private static bool? isXpOrLater;
        private static bool? isComCtlV6Available;

        internal static bool IsVistaOrLater
        {
            get
            {
                if (isVistaOrLater.HasValue)
                {
                    return isVistaOrLater.Value;
                }

                OperatingSystem os = Environment.OSVersion;
                if (os.Platform != PlatformID.Win32NT)
                {
                    isVistaOrLater = false;
                    return false;
                }

                isVistaOrLater = os.Version >= new Version(6, 0, 5243);
                return isVistaOrLater.Value;
            }
        }

        internal static bool IsWindowsXpOrLater
        {
            get
            {
                if (isXpOrLater.HasValue)
                {
                    return isXpOrLater.Value;
                }

                isXpOrLater = Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version >= new Version(5, 1, 2600);
                return isXpOrLater.Value;
            }
        }

        /// <summary>
        /// Gets whether comctl32.dll V6 is available, without loading it explicitly.
        /// After all tells, whether <see cref="Application.EnableVisualStyles"/> was already called in this current application.
        /// </summary>
        internal static bool IsComCtlV6Available
        {
            get
            {
                if (isComCtlV6Available.HasValue)
                    return isComCtlV6Available.Value;

                // pre-XP: no visual styles
                if (!IsWindowsXpOrLater)
                {
                    isComCtlV6Available = false;
                    return false;
                }

                // visual styles are actually used
                if (Application.RenderWithVisualStyles)
                {
                    isComCtlV6Available = true;
                    return true;
                }

                // Here EnableVisualStyles was either called but classic theme is used (true result) or visual styles were not enabled at all (false result)
                // We could use the Comctl32ActivationContext and get the dll version of comctl32, but then V6 would be loaded accidentaly, causing that controls
                // begin to use visual styles in non-System mode.
                isComCtlV6Available = (bool)Reflector.GetProperty(typeof(Application), "ComCtlSupportsVisualStyles");
                return isComCtlV6Available.Value;
            }
        }
    }
}
