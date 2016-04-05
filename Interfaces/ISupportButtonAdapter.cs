namespace KGySoft.Controls
{
    internal interface ISupportButtonAdapter
    {
        ButtonBaseAdapter Adapter { get; }
        bool ShowFocusCues { get; }
        bool ShowKeyboardCues { get; }
    }
}
