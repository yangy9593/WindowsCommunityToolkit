// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// A <see cref="LottieValueCallback{T}"/> that interpolates values based on a <see cref="IInterpolator"/>
    /// </summary>
    /// <typeparam name="T">The type that the callback should return.</typeparam>
    public abstract class LottieInterpolatedValue<T> : LottieValueCallback<T>
    {
        private readonly T _startValue;
        private readonly T _endValue;
        private readonly IInterpolator _interpolator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedValue{T}"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedValue{T}"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedValue{T}"/></param>
        protected LottieInterpolatedValue(T startValue, T endValue)
            : this(startValue, endValue, new LinearInterpolator())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieInterpolatedValue{T}"/> class.
        /// </summary>
        /// <param name="startValue">The starting value of the <see cref="LottieInterpolatedValue{T}"/></param>
        /// <param name="endValue">The ending value of the <see cref="LottieInterpolatedValue{T}"/></param>
        /// <param name="interpolator">The <see cref="IInterpolator"/> that will interpolate the values between the start value and the end value.</param>
        protected LottieInterpolatedValue(T startValue, T endValue, IInterpolator interpolator)
        {
            _startValue = startValue;
            _endValue = endValue;
            _interpolator = interpolator;
        }

        /// <inheritdoc/>
        public override T GetValue(LottieFrameInfo<T> frameInfo)
        {
            float progress = _interpolator.GetInterpolation(frameInfo.OverallProgress);
            return InterpolateValue(_startValue, _endValue, progress);
        }

        /// <summary>
        /// A method that interpolates between the startValue and the endValue, using the current interpolator
        /// </summary>
        /// <param name="startValue">The startValue of the interpolation that the progress will be based on</param>
        /// <param name="endValue">The endValue of the interpolation that the progress will be based on</param>
        /// <param name="progress">The progress that the interpolation will be calculated</param>
        /// <returns>Returns the interpolation between the startValue and the endValue, using the current interpolator, at the current progress</returns>
        protected abstract T InterpolateValue(T startValue, T endValue, float progress);
    }
}
