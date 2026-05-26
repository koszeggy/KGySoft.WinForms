#region Copyright

///////////////////////////////////////////////////////////////////////////////
//  File: TimeHelper.cs
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
using System.Diagnostics;

#endregion

namespace KGySoft.WinForms
{
    /// <summary>
    /// A helper class to get a time stamp faster than UtcNow.
    /// NOTE: Do not use if 15 ms accuracy is not enough!
    /// </summary>
    internal static class TimeHelper
    {
        #region Fields

#if !NETCOREAPP3_0_OR_GREATER
        private static readonly long stopwatchTicksPerMillisecond = Stopwatch.Frequency / 1000;
#endif

        #endregion

        #region Methods

        internal static long GetTimeStamp() =>
#if NETCOREAPP3_0_OR_GREATER
            // On .NET 5 this is 8.1 times faster than UtcNow, though it changes in every 15 ms or so.
            Environment.TickCount64;
#else
            // If high resolution is enabled, this can be 3.2 times faster than UtcNow
            Stopwatch.GetTimestamp();
#endif

        internal static long GetInterval(int milliseconds) =>
#if NETCOREAPP3_0_OR_GREATER
            milliseconds;
#else
            milliseconds * stopwatchTicksPerMillisecond;
#endif

        internal static TimeSpan GetTimeSpan(long interval) =>
#if NETCOREAPP3_0_OR_GREATER
            new TimeSpan(interval * TimeSpan.TicksPerMillisecond);
#else
            new TimeSpan(interval * TimeSpan.TicksPerMillisecond / stopwatchTicksPerMillisecond);
#endif

        #endregion
    }
}
