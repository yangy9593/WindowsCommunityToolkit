using System;
using System.Numerics;
using Windows.UI.Composition;

namespace Lottie
{
    /// <summary>
    /// The source of an animated composition.
    /// </summary>
    public interface ICompositionSource
    {
        /// <summary>
        /// Attempts to create an instance of the playable composition.
        /// </summary>
        /// <param name="compositor">The compositor used to instantiate the composition.</param>
        /// <param name="rootVisual">The root visual of the playable composition.</param>
        /// <param name="size">The size of the playable composition.</param>
        /// <param name="progressPropertySet">The <see cref="CompositionPropertySet"/> that holds the "Progress" property.</param>
        /// <param name="duration">The duration of the playable composition.</param>
        /// <param name="diagnostics">An optional object that holds information about the creation
        /// of the instance.</param>
        /// <returns>True if the creation succeeds.</returns>
        bool TryCreateInstance(
            Compositor compositor,
            out Visual rootVisual,
            out Vector2 size,
            out CompositionPropertySet progressPropertySet,
            out TimeSpan duration,
            out object diagnostics);
    }
}
