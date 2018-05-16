using System;
using System.Collections.Generic;
using System.Text;

namespace WinCompData.Mgc
{
#if !WINDOWS_UWP
    public
#endif
    sealed class CanvasDevice : IDisposable
    {
        public static CanvasDevice GetSharedDevice() => new CanvasDevice();

        public void Dispose()
        {
        }
    }
}
