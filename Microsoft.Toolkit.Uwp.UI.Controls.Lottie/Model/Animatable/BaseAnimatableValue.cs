// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Text;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable
{
    internal abstract class BaseAnimatableValue<TV, TO> : IAnimatableValue<TV, TO>
    {
        private readonly List<Keyframe<TV>> _keyframes;

        internal List<Keyframe<TV>> Keyframes => _keyframes;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseAnimatableValue{TV, TO}"/> class.
        /// Create a default static animatable path.
        /// </summary>
        internal BaseAnimatableValue(TV value)
            : this(new List<Keyframe<TV>> { new Keyframe<TV>(value) })
        {
        }

        internal BaseAnimatableValue(List<Keyframe<TV>> keyframes)
        {
            _keyframes = keyframes;
        }

        public abstract IBaseKeyframeAnimation<TV, TO> CreateAnimation();

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Keyframes.Count > 0)
            {
                sb.Append("values=").Append("[" + string.Join(",", Keyframes) + "]");
            }

            return sb.ToString();
        }
    }
}