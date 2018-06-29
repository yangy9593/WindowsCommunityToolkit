// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable
{
    internal class AnimatableTextProperties
    {
        private readonly AnimatableColorValue color;
        private readonly AnimatableColorValue stroke;
        private readonly AnimatableFloatValue strokeWidth;
        private readonly AnimatableFloatValue tracking;

        internal AnimatableColorValue Color => color;

        internal AnimatableColorValue Stroke => stroke;

        internal AnimatableFloatValue StrokeWidth => strokeWidth;

        internal AnimatableFloatValue Tracking => tracking;

        public AnimatableTextProperties(AnimatableColorValue color, AnimatableColorValue stroke, AnimatableFloatValue strokeWidth, AnimatableFloatValue tracking)
        {
            this.color = color;
            this.stroke = stroke;
            this.strokeWidth = strokeWidth;
            this.tracking = tracking;
        }
    }
}
