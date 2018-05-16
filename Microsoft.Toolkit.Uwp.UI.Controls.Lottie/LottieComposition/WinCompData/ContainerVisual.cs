using System.Collections.Generic;
using WinCompData.Tools;

namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    class ContainerVisual : Visual
    {
        internal ContainerVisual() { }

        public ListOfNeverNull<Visual> Children { get; } = new ListOfNeverNull<Visual>();

        public override CompositionObjectType Type => CompositionObjectType.ContainerVisual;

    }
}
