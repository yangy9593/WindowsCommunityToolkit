// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value
{
    /// <summary>
    /// <see cref="Value.LottieValueCallback{T}"/> that provides a value offset from the original animation
    ///  rather than an absolute value.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class LottieRelativePointValueCallback : LottieValueCallback<Vector2?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LottieRelativePointValueCallback"/> class.
        /// </summary>
        public LottieRelativePointValueCallback()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LottieRelativePointValueCallback"/> class.
        /// </summary>
        /// <param name="staticValue">A static <see cref="Vector2"/> to be used as a static value.</param>
        public LottieRelativePointValueCallback(Vector2 staticValue)
            : base(staticValue)
        {
        }

        /// <inheritdoc/>
        public override Vector2? GetValue(LottieFrameInfo<Vector2?> frameInfo)
        {
            var point = new Vector2(
                MiscUtils.Lerp(
                    frameInfo.StartValue.Value.X,
                    frameInfo.EndValue.Value.X,
                    frameInfo.InterpolatedKeyframeProgress),
                MiscUtils.Lerp(
                    frameInfo.StartValue.Value.Y,
                    frameInfo.EndValue.Value.Y,
                    frameInfo.InterpolatedKeyframeProgress));

            var offset = GetOffset(frameInfo);
            point.X += offset.X;
            point.Y += offset.Y;
            return point;
        }

        /// <summary>
        /// Gets the offset for a specified <see cref="LottieFrameInfo{T}"/>
        /// Override this to provide your own offset on every frame.
        /// </summary>
        /// <param name="frameInfo">The <see cref="LottieFrameInfo{T}"/> that the offset should get the value from</param>
        /// <returns>Returns the value of the provided <see cref="LottieFrameInfo{T}"/></returns>
        public Vector2 GetOffset(LottieFrameInfo<Vector2?> frameInfo)
        {
            if (Value == null)
            {
                throw new ArgumentException("You must provide a static value in the constructor " +
                                                    ", call setValue, or override getValue.");
            }

            return Value.Value;
        }
    }
}
