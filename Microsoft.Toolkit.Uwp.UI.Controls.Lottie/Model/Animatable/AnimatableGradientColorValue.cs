// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable
{
    internal class AnimatableGradientColorValue : BaseAnimatableValue<GradientColor, GradientColor>
    {
        public AnimatableGradientColorValue(List<Keyframe<GradientColor>> keyframes)
            : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<GradientColor, GradientColor> CreateAnimation()
        {
            return new GradientColorKeyframeAnimation(Keyframes);
        }
    }
}