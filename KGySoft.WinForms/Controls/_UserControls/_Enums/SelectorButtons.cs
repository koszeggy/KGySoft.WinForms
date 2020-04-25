using System;

namespace KGySoft.WinForms.Controls
{
    /// <summary>
    /// Buttons that can be appeared in a <see cref="ucCustomSelector"/>
    /// </summary>
    [Flags]
    public enum SelectorButtons
    {
        None = 0,
        ClearSelection = 1 << 0,
        SelectAll = 1 << 1,
        SelectNone = 1 << 2,
        Browse = 1 << 3,
        Editor = 1 << 4,
        New = 1 << 5
    }
}