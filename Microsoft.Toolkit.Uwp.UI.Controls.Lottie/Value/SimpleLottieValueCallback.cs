// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// Delegate interface for <see cref="LottieValueCallback{T}"/>. This is helpful for the Kotlin API because you can use a SAM conversion to write the
    /// callback as a single abstract method block like this:
    /// animationView.AddValueCallback(keyPath, LottieProperty.TransformOpacity) { 50 }
    /// </summary>
    /// <typeparam name="T">The type that the callback will act on.</typeparam>
    /// <param name="frameInfo">The information of this frame, which this callback wants to change</param>
    /// <returns>Return the appropriate value that it wants to change.</returns>
    public delegate T SimpleLottieValueCallback<T>(LottieFrameInfo<T> frameInfo);
}
