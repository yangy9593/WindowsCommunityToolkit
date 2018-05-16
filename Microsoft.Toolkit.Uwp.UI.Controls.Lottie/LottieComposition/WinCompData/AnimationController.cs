namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class AnimationController : CompositionObject
    {
        internal AnimationController(CompositionObject targetObject, string targetProperty)
        {
            TargetObject = targetObject;
            TargetProperty = targetProperty;
        }

        public CompositionObject TargetObject { get; }
        public string TargetProperty { get; }
        public void Pause() { }

        public override CompositionObjectType Type => CompositionObjectType.AnimationController;
    }
}
