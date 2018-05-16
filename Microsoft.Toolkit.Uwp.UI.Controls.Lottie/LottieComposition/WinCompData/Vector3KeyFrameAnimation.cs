using WinCompData.Sn;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class Vector3KeyFrameAnimation : KeyFrameAnimation<Vector3>
    {
        internal Vector3KeyFrameAnimation() : base(null) { }
        Vector3KeyFrameAnimation(Vector3KeyFrameAnimation other) : base(other) { }

        public override CompositionObjectType Type => CompositionObjectType.Vector3KeyFrameAnimation;

        internal override CompositionAnimation Clone() => new Vector3KeyFrameAnimation(this);
    }
}
