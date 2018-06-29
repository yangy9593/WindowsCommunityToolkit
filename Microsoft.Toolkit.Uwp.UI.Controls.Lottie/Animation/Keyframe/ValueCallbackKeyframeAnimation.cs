// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe
{
    internal class ValueCallbackKeyframeAnimation<TK, TA> : BaseKeyframeAnimation<TK, TA>
    {
        private readonly LottieFrameInfo<TA> _frameInfo = new LottieFrameInfo<TA>();

        internal ValueCallbackKeyframeAnimation(ILottieValueCallback<TA> valueCallback)
            : base(new List<Keyframe<TK>>())
        {
            SetValueCallback(valueCallback);
        }

        /// <summary>
        /// Gets the end progress of the animation. If this doesn't return 1, then <see cref="BaseKeyframeAnimation{TK, TA}.Progress"/> will always clamp the progress
        /// to 0.
        /// </summary>
        protected override float EndProgress => 1f;

        public override void OnValueChanged()
        {
            if (ValueCallback != null)
            {
                base.OnValueChanged();
            }
        }

        public override TA Value => ValueCallback.GetValueInternal(0f, 0f, default(TA), default(TA), Progress, Progress, Progress);

        public override TA GetValue(Keyframe<TK> keyframe, float keyframeProgress)
        {
            return Value;
        }
    }
}