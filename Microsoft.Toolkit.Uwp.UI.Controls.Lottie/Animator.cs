// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// Base abstract class that provides basic animation functionality
    /// </summary>
    public abstract class Animator
    {
        /// <summary>
        /// Gets or sets the total duration os the animation
        /// </summary>
        public virtual long Duration { get; set; }

        /// <summary>
        /// Gets a value indicating whether the animation is currently running
        /// </summary>
        public abstract bool IsRunning { get; }

        /// <summary>
        /// Cancels the animation that is being executed
        /// </summary>
        public virtual void Cancel()
        {
            AnimationCanceled();
        }

        /// <summary>
        /// Invoked whenever the animation is canceled
        /// </summary>
        protected virtual void AnimationCanceled()
        {
        }
    }
}