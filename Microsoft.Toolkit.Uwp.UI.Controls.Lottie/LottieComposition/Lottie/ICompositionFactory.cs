using System;
using System.Numerics;
using WinCompData;

namespace Lottie
{
    interface ICompositionFactory
    {
        /// <summary>
        /// Creates an animated composition.
        /// </summary>
        /// <param name="compositor">The compositor used to instantiate composition object.</param>
        /// <param name="rootVisual">The resulting root of the visual tree.</param>
        /// <param name="size">The size of the result.</param>
        /// <param name="progressPropertySet">A property set that contains a scalar property called Progress that can be used
        /// to set the progress of the composition animation.</param>
        /// <param name="duration">The duration of the composition animation.</param>
        void CreateInstance(
            Compositor compositor,
            out Visual rootVisual,
            out Vector2 size,
            out CompositionPropertySet progressPropertySet,
            out TimeSpan duration);
    }
}
