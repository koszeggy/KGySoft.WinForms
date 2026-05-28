#if NET35 || NET40

// ReSharper disable once CheckNamespace
namespace System.Threading
{
    internal static class Volatile
    {
        #region Methods

        internal static void Write<T>(ref T location, T value) where T : class?
        {
            Thread.MemoryBarrier();
            location = value;
        }

        internal static long Read(ref long location) => Thread.VolatileRead(ref location);
        internal static void Write(ref long location, long value) => Thread.VolatileWrite(ref location, value);

        #endregion
    }
}

#endif