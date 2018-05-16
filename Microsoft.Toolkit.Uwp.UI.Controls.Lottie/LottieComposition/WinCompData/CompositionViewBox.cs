using WinCompData.Sn;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CompositionViewBox : CompositionObject
    {
        internal CompositionViewBox() { }
        public Vector2 Size { get; set; }

        public override CompositionObjectType Type => CompositionObjectType.CompositionViewBox;
    }
}
