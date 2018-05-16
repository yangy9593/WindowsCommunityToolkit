namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class LinearEasingFunction : CompositionEasingFunction
    {
        internal LinearEasingFunction() { }

        public override CompositionObjectType Type => CompositionObjectType.LinearEasingFunction;
    }
}
