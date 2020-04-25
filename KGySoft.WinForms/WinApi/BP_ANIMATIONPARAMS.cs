using System.Runtime.InteropServices;

namespace KGySoft.WinForms.WinApi
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BP_ANIMATIONPARAMS
    {
        internal int cbSize;
        internal int dwFlags;
        internal BP_ANIMATIONSTYLE style;
        internal int dwDuration;
    }
}
