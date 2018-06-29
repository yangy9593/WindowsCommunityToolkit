// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Keyframe;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Layer;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content
{
    internal class TrimPathContent : IContent
    {
        public event EventHandler ValueChanged;

        private readonly IBaseKeyframeAnimation<float?, float?> _startAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _endAnimation;
        private readonly IBaseKeyframeAnimation<float?, float?> _offsetAnimation;

        internal TrimPathContent(BaseLayer layer, ShapeTrimPath trimPath)
        {
            Name = trimPath.Name;
            Type = trimPath.GetType();
            _startAnimation = trimPath.Start.CreateAnimation();
            _endAnimation = trimPath.End.CreateAnimation();
            _offsetAnimation = trimPath.Offset.CreateAnimation();

            layer.AddAnimation(_startAnimation);
            layer.AddAnimation(_endAnimation);
            layer.AddAnimation(_offsetAnimation);

            _startAnimation.ValueChanged += OnValueChanged;
            _endAnimation.ValueChanged += OnValueChanged;
            _offsetAnimation.ValueChanged += OnValueChanged;
        }

        private void OnValueChanged(object sender, EventArgs eventArgs)
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SetContents(List<IContent> contentsBefore, List<IContent> contentsAfter)
        {
            // Do nothing.
        }

        public string Name { get; }

        internal virtual ShapeTrimPath.Type Type { get; }

        public virtual IBaseKeyframeAnimation<float?, float?> Start => _startAnimation;

        public virtual IBaseKeyframeAnimation<float?, float?> End => _endAnimation;

        public virtual IBaseKeyframeAnimation<float?, float?> Offset => _offsetAnimation;
    }
}