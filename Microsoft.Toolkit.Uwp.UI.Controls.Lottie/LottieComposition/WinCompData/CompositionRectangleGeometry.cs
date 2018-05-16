using WinCompData.Sn;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CompositionRectangleGeometry : CompositionGeometry
    {
        internal CompositionRectangleGeometry() { }

        public Vector2 Size { get; set; }

        public override CompositionObjectType Type => CompositionObjectType.CompositionRectangleGeometry;
    }
}
