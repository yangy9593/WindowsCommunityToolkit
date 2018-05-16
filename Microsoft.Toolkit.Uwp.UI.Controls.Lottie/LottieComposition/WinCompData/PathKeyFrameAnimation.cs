namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class PathKeyFrameAnimation : KeyFrameAnimation<CompositionPath>
    {
        internal PathKeyFrameAnimation() : base(null) { }
        PathKeyFrameAnimation(PathKeyFrameAnimation other) : base(other) { }

        public override CompositionObjectType Type => CompositionObjectType.PathKeyFrameAnimation;

        internal override CompositionAnimation Clone() => new PathKeyFrameAnimation(this);

    }
}
