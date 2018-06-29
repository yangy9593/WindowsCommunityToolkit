// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    /// <summary>
    /// This view will load, deserialize, and display an After Effects animation exported with
    /// bodymovin (https://github.com/bodymovin/bodymovin).
    /// <para>
    /// You may set the animation in one of two ways:
    /// 1) Attrs: <seealso cref="SourceProperty"/>
    /// 2) Programatically: <seealso cref="SetAnimationAsync(string)"/>, <seealso cref="Composition"/>,
    /// or <seealso cref="SetAnimationAsync(TextReader)"/>.
    /// </para>
    /// <para>
    /// You can set a default cache strategy with <seealso cref="CacheStrategy.None"/>.
    /// </para>
    /// <para>
    /// You can manually set the progress of the animation with <seealso cref="Progress"/> or
    /// <seealso cref="Progress"/>
    /// </para>
    /// </summary>
    public partial class LottieAnimationView
    {
        /// <summary>
        /// This event will be invoked whenever the frame of the animation changes.
        /// </summary>
        public event EventHandler<ValueAnimator.ValueAnimatorUpdateEventArgs> AnimatorUpdate
        {
            add => _lottieDrawable.AnimatorUpdate += value;
            remove => _lottieDrawable.AnimatorUpdate -= value;
        }

        /// <summary>
        /// Clears the <seealso cref="AnimatorUpdate"/> event handler.
        /// </summary>
        public void RemoveAllUpdateListeners()
        {
            _lottieDrawable.RemoveAllUpdateListeners();
        }

        /// <summary>
        /// This event will be invoked whenever the internal animator is executed.
        /// </summary>
        public event EventHandler ValueChanged
        {
            add => _lottieDrawable.ValueChanged += value;
            remove => _lottieDrawable.ValueChanged -= value;
        }

        /// <summary>
        /// Clears the <seealso cref="ValueChanged"/> event handler.
        /// </summary>
        public void RemoveAllAnimatorListeners()
        {
            _lottieDrawable.RemoveAllAnimatorListeners();
        }
    }
}
