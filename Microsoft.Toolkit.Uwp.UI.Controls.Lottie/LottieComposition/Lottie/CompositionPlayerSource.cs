using System;
using System.Threading.Tasks;
using Windows.Foundation.Metadata;
using Windows.UI.Composition;

namespace Lottie
{
    /// <summary>
    /// The type of the Source property of a <see cref="CompositionPlayer"/>.
    /// </summary>
    [CreateFromString(MethodName = "Lottie.LottieComposition.CreateFromString")]
    public class CompositionPlayerSource
    {
        internal CompositionPlayerSource() { }

        internal virtual Task<CompositionLoadResult> TryLoad(Compositor compositor)
        {
            // WinRT does not allow abstract types, so this is the next best thing.
            throw new NotImplementedException();
        }
    }
}
