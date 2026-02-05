#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: ThemingActivationContext.cs
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

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

#endregion

namespace KGySoft.WinForms.WinApi
{
    /// <summary>
    /// This class makes sure that the correct version of Comctl32.dll will be loaded.
    /// Needed for <see cref="Components.TaskDialog"/> used on 64-bit machines, where P/Invoke from comctl32
    /// may cause an <see cref="EntryPointNotFoundException"/> without using this context.
    /// </summary>
    internal sealed class ThemingActivationContext : IDisposable
    {
        #region Fields

        #region Static Fields

        private static readonly IntPtr themingManifestId =
#if NETFRAMEWORK
            new IntPtr(101);
#elif NETCOREAPP
            new IntPtr(2);
#endif


        private static ACTCTX enableThemingActivationContext;
        private static Kernel32.ActivationContextSafeHandle activationContext = null!;
        private static bool? themingContextCreated;

        #endregion

        #region Instance Fields

        private IntPtr cookie;

        #endregion

        #endregion

        #region Properties

        internal static bool IsThemingAvailable => themingContextCreated ??= TryCreateThemingActivationContext();

        #endregion

        #region Construction and Destruction

        #region Constructors

        public ThemingActivationContext(bool enable)
        {
            if (!enable || !OSHelper.IsWindowsXpOrLater || !IsThemingAvailable)
                return;

            if (!Kernel32.ActivateActCtx(activationContext, out cookie))
            {
                // Be sure cookie always zero if activation failed
                cookie = IntPtr.Zero;
            }
        }

        #endregion

        #region Destructor

        ~ThemingActivationContext()
        {
            Dispose(false);
        }

        #endregion

        #endregion

        #region Methods

        #region Static Methods

        private static bool TryCreateThemingActivationContext()
        {
            // it is safe to lock on the internal type
            lock (typeof(ThemingActivationContext))
            {
                enableThemingActivationContext = new ACTCTX();
                enableThemingActivationContext.cbSize = MarshalHelper.SizeOf<ACTCTX>();
                enableThemingActivationContext.lpSource = typeof(Application).Assembly.Location;
                enableThemingActivationContext.lpResourceName = themingManifestId;
                enableThemingActivationContext.dwFlags = Constants.ACTCTX_FLAG_ASSEMBLY_RESOURCE_NAME_VALID;

                // Note this will fail gracefully if themingManifestId does not exist in System.Windows.Forms.dll
                activationContext = Kernel32.CreateActCtx(ref enableThemingActivationContext);
                Debug.Assert(!activationContext.IsInvalid, "Theming activation context could not be created. The checks should prevent we reach this point with failure.");
                return !activationContext.IsInvalid;
            }
        }

        #endregion

        #region Instance Methods

        #region Public Methods

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private Methods

        private void Dispose(bool _)
        {
            if (cookie != IntPtr.Zero)
            {
                try
                {
                    if (Kernel32.DeactivateActCtx(0, cookie))
                    {
                        // deactivation succeeded...
                        cookie = IntPtr.Zero;
                    }
                }
                catch (Exception e) when (!e.IsCritical())
                {
                    // sometimes throws a System.Runtime.InteropServices.SEHException
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}
