// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// A <see cref="LottieInterpolatedValue{T}"/> with T as Vector2
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public class LottieInterpolatedPointValue : LottieInterpolatedValue<Vector2>
    {
        private Vector2 _point;

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedPointValue"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedPointValue"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedPointValue"/></param>
        public LottieInterpolatedPointValue(Vector2 startValue, Vector2 endValue)
            : base(startValue, endValue)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedPointValue"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedPointValue"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedPointValue"/></param>
        /// <param name="interpolator">The <see cref="IInterpolator"/> that will interpolate the values between the start value and the end value.</param>
        public LottieInterpolatedPointValue(Vector2 startValue, Vector2 endValue, IInterpolator interpolator)
            : base(startValue, endValue, interpolator)
        {
        }

        /// <inheritdoc/>
        protected override Vector2 InterpolateValue(Vector2 startValue, Vector2 endValue, float progress)
        {
            _point.X = MiscUtils.Lerp(startValue.X, endValue.X, progress);
            _point.Y = MiscUtils.Lerp(startValue.Y, endValue.Y, progress);
            return _point;
        }
    }
}
