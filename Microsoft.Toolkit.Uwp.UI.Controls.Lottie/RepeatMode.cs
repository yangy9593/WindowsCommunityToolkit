// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Indicated if the animation should be restarted from the begining or if it should reverse, when reaching it's end.
    /// </summary>
    public enum RepeatMode
    {
        /// <summary>
        /// When the animation reaches the end and <see cref="LottieDrawable.RepeatCount"/> is INFINITE
        /// or a positive value, the animation restarts from the beginning.
        /// </summary>
        Restart = 1,

        /// <summary>
        /// When the animation reaches the end and <see cref="LottieDrawable.RepeatCount"/> is INFINITE
        /// or a positive value, the animation reverses direction on every iteration.
        /// </summary>
        Reverse = 2
    }
}
