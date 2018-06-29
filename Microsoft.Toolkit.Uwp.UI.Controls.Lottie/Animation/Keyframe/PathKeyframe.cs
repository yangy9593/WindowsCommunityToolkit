// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe
{
    internal class PathKeyframe : Keyframe<Vector2?>
    {
        internal PathKeyframe(LottieComposition composition, Keyframe<Vector2?> keyframe)
            : base(composition, keyframe.StartValue, keyframe.EndValue, keyframe.Interpolator, keyframe.StartFrame, keyframe.EndFrame)
        {
            var equals = EndValue != null && StartValue != null && StartValue.Equals(EndValue.Value);
            if (EndValue != null && !equals)
            {
                Path = Utils.Utils.CreatePath(StartValue.Value, EndValue.Value, keyframe.PathCp1, keyframe.PathCp2);
            }
        }

        /// <summary>
        /// Gets the path of the keyframe. This will be null if the startValue and endValue are the same.
        /// </summary>
        internal Path Path { get; }
    }
}