using WinCompData.Tools;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    interface IContainShapes
    {
        ListOfNeverNull<CompositionShape> Shapes { get; }
    }
}
