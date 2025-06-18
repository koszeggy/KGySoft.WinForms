#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: Kernel32.cs
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

#if NETFRAMEWORK
using System.Security.Permissions;
#endif

#region Used Namespaces

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

#endregion

#endregion

namespace KGySoft.WinForms.WinApi
{
    internal static class Kernel32
    {
        #region Nested classes

#if NETFRAMEWORK
        [SecurityPermission(SecurityAction.Demand, UnmanagedCode = true)]
#endif
        internal sealed class ActivationContextSafeHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            #region Constructors

            public ActivationContextSafeHandle()
                : base(true)
            {
            }

            #endregion

            #region Methods

#if NETFRAMEWORK
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
            protected override bool ReleaseHandle()
            {
                ReleaseActCtx(handle);
                return true;
            }

            #endregion
        }

        #endregion

        #region Methods

        /// <summary>
        /// The CreateActCtx function creates an activation context.
        /// </summary>
        /// <param name="actctx">Pointer to an ACTCTX structure that contains information about the activation context to be created.</param>
        /// <returns>If the function succeeds, it returns a handle to the returned activation context. Otherwise, it returns INVALID_HANDLE_VALUE.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern ActivationContextSafeHandle CreateActCtx(ref ACTCTX actctx);

        /// <summary>
        /// The ReleaseActCtx function decrements the reference count of the specified activation context.
        /// </summary>
        /// <param name="hActCtx">Handle to the ACTCTX structure that contains information on the activation context for which the reference count is to be decremented.</param>
        [DllImport("kernel32.dll"), ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
        internal static extern void ReleaseActCtx(IntPtr hActCtx);

        /// <summary>
        /// The ActivateActCtx function activates the specified activation context. It does this by pushing the specified activation context to the top of the activation stack.
        /// The specified activation context is thus associated with the current thread and any appropriate side-by-side API functions.
        /// </summary>
        /// <param name="hActCtx">Handle to an ACTCTX structure that contains information on the activation context that is to be made active.</param>
        /// <param name="lpCookie">Pointer to a ULONG_PTR that functions as a cookie, uniquely identifying a specific, activated activation context.</param>
        /// <returns>If the function succeeds, it returns TRUE. Otherwise, it returns FALSE.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ActivateActCtx(ActivationContextSafeHandle hActCtx, out IntPtr lpCookie);

        /// <summary>
        /// The DeactivateActCtx function deactivates the activation context corresponding to the specified cookie.
        /// </summary>
        /// <param name="dwFlags">Flags that indicate how the deactivation is to occur.</param>
        /// <param name="lpCookie">The ULONG_PTR that was passed into the call to ActivateActCtx. This value is used as a cookie to identify a specific activated activation context.</param>
        /// <returns>If the function succeeds, it returns TRUE. Otherwise, it returns FALSE.</returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DeactivateActCtx(uint dwFlags, IntPtr lpCookie);

        /// <summary>
        /// Retrieves a module handle for the specified module. The module must have been loaded by the calling process.
        /// </summary>
        /// <param name="lpModuleName">The name of the loaded module (either a .dll or .exe file). If the file name extension is omitted, the default library extension .dll is appended.
        /// The file name string can include a trailing point character (.) to indicate that the module name has no extension. The string does not have to specify a path.
        /// When specifying a path, be sure to use backslashes (\), not forward slashes (/). The name is compared (case independently) to the names of modules currently mapped into the
        /// address space of the calling process.</param>
        /// <returns>If the function succeeds, the return value is a handle to the specified module.
        /// If the function fails, the return value is NULL.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr GetModuleHandle(string lpModuleName);

        /// <summary>
        /// Loads the specified module into the address space of the calling process. The specified module may cause other modules to be loaded.
        /// </summary>
        /// <param name="lpFileName">The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file).
        /// If the string specifies a full path, the function searches only that path for the module.
        /// If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module.</param>
        /// <returns>If the function succeeds, the return value is a handle to the module.
        /// If the function fails, the return value is NULL.</returns>
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        internal static extern IntPtr LoadLibrary(string lpFileName);

        /// <summary>
        /// Frees the loaded dynamic-link library (DLL) module and, if necessary, decrements its reference count. When the reference count reaches zero,
        /// the module is unloaded from the address space of the calling process and the handle is no longer valid.
        /// </summary>
        /// <param name="hModule">A handle to the loaded library module. The <see cref="LoadLibrary"/> or <see cref="GetModuleHandle"/> function returns this handle.</param>
        /// <returns>If the function succeeds, the return value is nonzero. If the function fails, the return value is zero.</returns>
        [DllImport("kernel32.dll")]
        internal static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Retrieves the thread identifier of the calling thread.
        /// </summary>
        /// <returns>The return value is the thread identifier of the calling thread.</returns>
        [DllImport("kernel32.dll")]
        internal static extern uint GetCurrentThreadId();

        #endregion
    }
}
