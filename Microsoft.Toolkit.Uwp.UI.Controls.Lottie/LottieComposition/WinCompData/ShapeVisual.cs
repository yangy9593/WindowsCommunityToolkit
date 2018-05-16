using WinCompData.Tools;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ShapeVisual : ContainerVisual, IContainShapes
    {
        internal ShapeVisual() { }
        public ListOfNeverNull<CompositionShape> Shapes { get; } = new ListOfNeverNull<CompositionShape>();

        public CompositionViewBox ViewBox { get; set; }

        public override CompositionObjectType Type => CompositionObjectType.ShapeVisual;
    }
}
