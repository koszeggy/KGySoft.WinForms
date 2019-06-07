#region Used namespaces

using System;
using System.IO;
using System.Runtime.InteropServices;

#endregion

namespace KGySoft.Controls.WinApi
{
    /// <summary>
    /// This class makes sure that the correct version of Comctl32.dll will be loaded.
    /// Needed for <see cref="TaskDialog"/> used on 64-bit machines, where P/Invoke from comctl32
    /// may cause an <see cref="EntryPointNotFoundException"/> without using this context.
    /// </summary>
    sealed class Comctl32ActivationContext: IDisposable
    {
        #region Fields

        #region Static Fields

        private static ACTCTX enableThemingActivationContext;
        private static Kernel32.ActivationContextSafeHandle activationContext;
        private static bool contextCreationSucceeded;
        private static readonly object syncLock = new object();
        #endregion

        #region Instance Fields

        private IntPtr cookie;

        #endregion

        #endregion

        #region Construction and Destruction

        #region Constructors

        public Comctl32ActivationContext(bool enable)
        {
            if (enable && WindowsUtils.IsWindowsXpOrLater)
            {
                if (EnsureActivateContextCreated())
                {
                    if (!Kernel32.ActivateActCtx(activationContext, out cookie))
                    {
                        // Be sure cookie always zero if activation failed
                        cookie = IntPtr.Zero;
                    }
                }
            }
        }

        #endregion

        #region Destructor

        ~Comctl32ActivationContext()
        {
            Dispose(false);
        }

        #endregion

        #region Explicit Disposing

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (cookie != IntPtr.Zero)
            {
                if (Kernel32.DeactivateActCtx(0, cookie))
                {
                    // deactivation succeeded...
                    cookie = IntPtr.Zero;
                }
            }
        }

        #endregion

        #endregion

        #region Methods

        private static bool EnsureActivateContextCreated()
        {
            lock (syncLock)
            {
                if (!contextCreationSucceeded)
                {
                    // Pull manifest from the .NET Framework install
                    // directory

                    string assemblyLoc = typeof(object).Assembly.Location;

                    string installDir = Path.GetDirectoryName(assemblyLoc);
                    string manifestLoc = null;
                    if (installDir != null)
                    {
                        const string manifestName = "XPThemes.manifest";
                        manifestLoc = Path.Combine(installDir, manifestName);
                    }

                    if (manifestLoc != null)
                    {
                        enableThemingActivationContext = new ACTCTX();
                        enableThemingActivationContext.cbSize = Marshal.SizeOf(typeof(ACTCTX));
                        enableThemingActivationContext.lpSource = manifestLoc;

                        // Set the lpAssemblyDirectory to the install
                        // directory to prevent Win32 Side by Side from
                        // looking for comctl32 in the application
                        // directory, which could cause a bogus dll to be
                        // placed there and open a security hole.
                        enableThemingActivationContext.lpAssemblyDirectory = installDir;
                        enableThemingActivationContext.dwFlags = Constants.ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID;

                        // Note this will fail gracefully if file specified
                        // by manifestLoc doesn't exist.
                        activationContext = Kernel32.CreateActCtx(ref enableThemingActivationContext);
                        contextCreationSucceeded = !activationContext.IsInvalid;
                    }
                }

                // If we return false, we'll try again on the next call into
                // EnsureActivateContextCreated(), which is fine.
                return contextCreationSucceeded;
            }
        }

        #endregion
    }
}
