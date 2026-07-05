#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: MemoryHelper.cs
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
#if NETFRAMEWORK
using System.Security;
#endif

using KGySoft.WinForms.WinApi;

#endregion

namespace KGySoft.WinForms
{
    internal static class MemoryHelper
    {
        #region Constants

        private const int maxArrayLength = 0x7FEF_FFFF;
        private const int minFreeMemory = 1_048_576;

        #endregion

        #region Fields

#if NETFRAMEWORK
        private static long? maxMemoryForGC;
#endif

        #endregion

        #region Properties

        private static long MaxMemoryForGC
#if NETFRAMEWORK
        {
            [SecuritySafeCritical]
            get
            {
                maxMemoryForGC ??= Math.Min(
                    IntPtr.Size == 4 ? 1_600_000_000 : Int64.MaxValue,
                    OSHelper.IsWindows ? GetTotalMemory() : Int64.MaxValue);
                return maxMemoryForGC.Value;
            }
        }
#else
            => GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
#endif

        #endregion

        #region Methods
  
        #region Internal Methods

        /// <summary>
        /// Gets an educated guess whether an array of specified size can be allocated.
        /// It does not consider virtual memory and does not guarantee that out of memory can be avoided (especially in pre .NET 4.0 versions).
        /// We also ignore gcAllowVeryLargeObjects.
        /// </summary>
        internal static bool IsAvailableManaged(long arraySize)
        {
            if (arraySize > maxArrayLength)
                return false;

            var maxMem = MaxMemoryForGC;
            if (maxMem == Int64.MaxValue)
                return true;

            // Using the total physical available memory (or 1.6GB on 32-bit systems, whichever is smaller) to determine free memory.
            // Virtual memory is ignored, even if it can be used to avoid slowing down the system very much.
            if (maxMem - GC.GetTotalMemory(false) > arraySize)
                return true;

            // trying again with a forced garbage collection
            return maxMem - GC.GetTotalMemory(true) - minFreeMemory > arraySize;
        }

        /// <summary>
        /// Gets whether the specified size of (unmanaged) system memory is quickly available for allocation.
        /// </summary>
        internal static bool IsAvailableUnmanaged(long size)
        {
            // not guessing on non-Windows systems, going for the hard way
            if (!OSHelper.IsWindows)
                return true;

            return GetAvailableMemory() >= size;
        }

        #endregion

        #region Private Methods

#if NETFRAMEWORK
        private static long GetTotalMemory()
        {
            Debug.Assert(OSHelper.IsWindows);
            var status = new MEMORYSTATUSEX { dwLength = (uint)MarshalHelper.SizeOf<MEMORYSTATUSEX>() };
            if (!Kernel32.GlobalMemoryStatusEx(ref status))
                return Int64.MaxValue;
            return (long)status.ullTotalPhys;
        }
#endif

        private static long GetAvailableMemory()
        {
            Debug.Assert(OSHelper.IsWindows);
            var status = new MEMORYSTATUSEX { dwLength = (uint)MarshalHelper.SizeOf<MEMORYSTATUSEX>() };
            if (!Kernel32.GlobalMemoryStatusEx(ref status))
                return Int64.MaxValue;
            return (long)status.ullAvailPhys;
        }

        #endregion

        #endregion
    }
}
