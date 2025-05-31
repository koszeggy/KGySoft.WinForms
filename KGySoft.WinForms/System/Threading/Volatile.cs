#if NET35 || NET40

// ReSharper disable once CheckNamespace
namespace System.Threading
{
    internal static class Volatile
    {
        #region Methods

        internal static void Write<T>(ref T location, T value)
        {
            Thread.MemoryBarrier();
            location = value;
        }

        #endregion
    }
}

#endif