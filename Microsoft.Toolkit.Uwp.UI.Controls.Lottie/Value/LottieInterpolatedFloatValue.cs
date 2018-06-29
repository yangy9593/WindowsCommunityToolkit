// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// A <see cref="LottieInterpolatedValue{T}"/> with T as float
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class LottieInterpolatedFloatValue : LottieInterpolatedValue<float>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedFloatValue"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedFloatValue"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedFloatValue"/></param>
        public LottieInterpolatedFloatValue(float startValue, float endValue)
            : base(startValue, endValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedFloatValue"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedFloatValue"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedFloatValue"/></param>
        /// <param name="interpolator">The <see cref="IInterpolator"/> that will interpolate the values between the start value and the end value.</param>
        public LottieInterpolatedFloatValue(float startValue, float endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        /// <inheritdoc/>
        protected override float InterpolateValue(float startValue, float endValue, float progress)
        {
            return MiscUtils.Lerp(startValue, endValue, progress);
        }
    }
}
