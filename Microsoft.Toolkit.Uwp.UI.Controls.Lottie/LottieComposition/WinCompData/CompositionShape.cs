using WinCompData.Sn;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    abstract class CompositionShape : CompositionObject
    {
        internal CompositionShape() { }
        public Vector2? CenterPoint { get; set; }
        public Vector2? Offset { get; set; }
        public float? RotationAngleInDegrees { get; set; }
        public Vector2? Scale { get; set; }
    }
}
