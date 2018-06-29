// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable
{
    internal class AnimatablePathValue : IAnimatableValue<Vector2?, Vector2?>
    {
        private readonly List<Keyframe<Vector2?>> _keyframes;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnimatablePathValue"/> class.
        /// Create a default static animatable path.
        /// </summary>
        public AnimatablePathValue()
        {
            _keyframes = new List<Keyframe<Vector2?>> { new Keyframe<Vector2?>(new Vector2(0, 0)) };
        }

        public AnimatablePathValue(List<Keyframe<Vector2?>> keyframes)
        {
            _keyframes = keyframes;
        }

        public IBaseKeyframeAnimation<Vector2?, Vector2?> CreateAnimation()
        {
            if (_keyframes[0].Static)
            {
                return new PointKeyframeAnimation(_keyframes);
            }

            return new PathKeyframeAnimation(_keyframes.ToList());
        }
    }
}