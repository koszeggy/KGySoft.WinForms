using KGySoft.WinForms.Controls;

namespace KGySoft.WinForms
{
    internal interface ISupportButtonAdapter
    {
        ButtonBaseAdapter Adapter { get; }
        bool ShowFocusCues { get; }
        bool ShowKeyboardCues { get; }
    }
}
