using System;

namespace KGySoft.WinForms.WinApi
{
    internal struct ACTCTX
    {
        public int cbSize;
        public uint dwFlags;
        public string lpSource;
        public ushort wProcessorArchitecture;
        public ushort wLangId;
        public string lpAssemblyDirectory;
        public IntPtr lpResourceName;
        public string lpApplicationName;
    }
}
