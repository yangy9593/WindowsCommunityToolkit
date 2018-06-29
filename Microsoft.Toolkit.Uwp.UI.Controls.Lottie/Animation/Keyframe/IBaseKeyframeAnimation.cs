// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe
{
    /// <summary>
    /// Internal interface that indicates basic keyframe animations properties
    /// </summary>
    public interface IBaseKeyframeAnimation
    {
        /// <summary>
        /// Gets or sets the current progress of this animation
        /// </summary>
        float Progress { get; set; }

        /// <summary>
        /// This event is invoked whenever a value of this animation changed
        /// </summary>
        event EventHandler ValueChanged;

        /// <summary>
        /// Method that should invoke the ValueChanged event, whenever a value of this animation changes
        /// </summary>
        void OnValueChanged();
    }

    internal interface IBaseKeyframeAnimation<out TK, TA> : IBaseKeyframeAnimation
    {
        TA Value { get; }

        void SetValueCallback(ILottieValueCallback<TA> valueCallback);
    }
}