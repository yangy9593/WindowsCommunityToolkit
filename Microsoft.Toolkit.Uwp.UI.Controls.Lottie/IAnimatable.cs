// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Represents an animatable.
    /// </summary>
    public interface IAnimatable
    {
        /// <summary>
        /// Gets a value indicating whether the animation is currently running
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Starts the animation
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the animation
        /// </summary>
        void Stop();
    }
}
