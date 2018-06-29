// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Caching strategy for compositions that will be reused frequently.
    /// Weak or Strong indicates the GC reference strength of the composition in the cache.
    /// </summary>
    public enum CacheStrategy
    {
        /// <summary>
        /// Does not cache the <see cref="LottieComposition"/>
        /// </summary>
        None,

        /// <summary>
        /// Holds a weak reference to the <see cref="LottieComposition"/> once it is loaded and deserialized
        /// </summary>
        Weak,

        /// <summary>
        /// Holds a strong reference to the <see cref="LottieComposition"/> once it is loaded and deserialized
        /// </summary>
        Strong
    }
}