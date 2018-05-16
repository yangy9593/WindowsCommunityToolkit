using System;
using System.Numerics;
using Windows.UI.Composition;

namespace Lottie
{
    public interface ICompositionSink
    {
        void SetContent(
            Visual rootVisual,
            Vector2 size,
            CompositionPropertySet progressPropertySet,
            string progressPropertyName,
            TimeSpan duration,
            object diagnostics);
    }
}
