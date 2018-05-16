namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CompositionColorBrush : CompositionBrush
    {
        internal CompositionColorBrush(Wui.Color color) { Color = color; }
        public Wui.Color Color { get; set; }

        public override CompositionObjectType Type => CompositionObjectType.CompositionColorBrush;
    }
}
