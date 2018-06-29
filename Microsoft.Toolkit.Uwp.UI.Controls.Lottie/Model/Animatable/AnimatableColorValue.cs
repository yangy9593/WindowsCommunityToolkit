// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;
using Windows.UI;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable
{
    internal class AnimatableColorValue : BaseAnimatableValue<Color?, Color?>
    {
        public AnimatableColorValue(List<Keyframe<Color?>> keyframes)
            : base(keyframes)
        {
        }

        public override IBaseKeyframeAnimation<Color?, Color?> CreateAnimation()
        {
            return new ColorKeyframeAnimation(Keyframes);
        }
    }
}