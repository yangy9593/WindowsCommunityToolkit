using WinCompData.Sn;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    abstract class CompositionClip : CompositionObject
    {
        internal CompositionClip() {  }

        // Default is 0,0.
        public Vector2 CenterPoint { get; set; }

        // Default is 1, 1.
        public Vector2 Scale { get; set; } = new Vector2(1, 1);

    }
}
