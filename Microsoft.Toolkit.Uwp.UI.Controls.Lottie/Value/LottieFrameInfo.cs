// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// Data class for use with <see cref="LottieValueCallback{T}"/>.
    /// You should* not* hold a reference to the frame info parameter passed to your callback. It will be reused.
    /// </summary>
    /// <typeparam name="T">The type that the value of the LottieFrameInfo refers to.</typeparam>
    public class LottieFrameInfo<T>
    {
        internal LottieFrameInfo<T> Set(
            float startFrame,
            float endFrame,
            T startValue,
            T endValue,
            float linearKeyframeProgress,
            float interpolatedKeyframeProgress,
            float overallProgress)
        {
            StartFrame = startFrame;
            EndFrame = endFrame;
            StartValue = startValue;
            EndValue = endValue;
            LinearKeyframeProgress = linearKeyframeProgress;
            InterpolatedKeyframeProgress = interpolatedKeyframeProgress;
            OverallProgress = overallProgress;
            return this;
        }

        /// <summary>
        /// Gets the start frame of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public float StartFrame { get; private set; }

        /// <summary>
        /// Gets the end frame of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public float EndFrame { get; private set; }

        /// <summary>
        /// Gets the start value of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public T StartValue { get; private set; }

        /// <summary>
        /// Gets the end value of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public T EndValue { get; private set; }

        /// <summary>
        /// Gets the linear keyframe progress of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public float LinearKeyframeProgress { get; private set; }

        /// <summary>
        /// Gets the interpolated keyframe progress of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public float InterpolatedKeyframeProgress { get; private set; }

        /// <summary>
        /// Gets the overall progress of the <see cref="LottieFrameInfo{T}"/>
        /// </summary>
        public float OverallProgress { get; private set; }
    }
}
