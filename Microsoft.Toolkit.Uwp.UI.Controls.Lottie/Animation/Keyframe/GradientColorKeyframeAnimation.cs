// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;
using Windows.UI;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe
{
    internal class GradientColorKeyframeAnimation : KeyframeAnimation<GradientColor>
    {
        private readonly GradientColor _gradientColor;

        internal GradientColorKeyframeAnimation(List<Keyframe<GradientColor>> keyframes)
            : base(keyframes)
        {
            var startValue = keyframes[0].StartValue;
            var size = startValue?.Size ?? 0;
            _gradientColor = new GradientColor(new float[size], new Color[size]);
        }

        public override GradientColor GetValue(Keyframe<GradientColor> keyframe, float keyframeProgress)
        {
            _gradientColor.Lerp(keyframe.StartValue, keyframe.EndValue, keyframeProgress);
            return _gradientColor;
        }
    }
}