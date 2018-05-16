namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ColorKeyFrameAnimation : KeyFrameAnimation<Wui.Color>
    {
        internal ColorKeyFrameAnimation() : base(null) { }
        ColorKeyFrameAnimation(ColorKeyFrameAnimation other) : base(other) { }

        public override CompositionObjectType Type => CompositionObjectType.ColorKeyFrameAnimation;

        internal override CompositionAnimation Clone() => new ColorKeyFrameAnimation(this);
    }
}
