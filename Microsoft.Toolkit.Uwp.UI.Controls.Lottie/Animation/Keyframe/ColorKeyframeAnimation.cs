// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Utils;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;
using Windows.UI;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe
{
    internal class ColorKeyframeAnimation : KeyframeAnimation<Color?>
    {
        internal ColorKeyframeAnimation(List<Keyframe<Color?>> keyframes)
            : base(keyframes)
        {
        }

        public override Color? GetValue(Keyframe<Color?> keyframe, float keyframeProgress)
        {
            if (keyframe.StartValue == null || keyframe.EndValue == null)
            {
                throw new System.InvalidOperationException("Missing values for keyframe.");
            }

            var startColor = keyframe.StartValue;
            var endColor = keyframe.EndValue;

            if (ValueCallback != null)
            {
                return ValueCallback.GetValueInternal(keyframe.StartFrame.Value, keyframe.EndFrame.Value, startColor, endColor, keyframeProgress, LinearCurrentKeyframeProgress, Progress);
            }

            return GammaEvaluator.Evaluate(keyframeProgress, startColor.Value, endColor.Value);
        }
    }
}