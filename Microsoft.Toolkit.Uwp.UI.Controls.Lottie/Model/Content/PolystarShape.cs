// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Layer;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content
{
    internal class PolystarShape : IContentModel
    {
        public enum Type
        {
            Star = 1,
            Polygon = 2
        }

        private readonly Type _type;

        internal PolystarShape(string name, Type type, AnimatableFloatValue points, IAnimatableValue<Vector2?, Vector2?> position, AnimatableFloatValue rotation, AnimatableFloatValue innerRadius, AnimatableFloatValue outerRadius, AnimatableFloatValue innerRoundedness, AnimatableFloatValue outerRoundedness)
        {
            Name = name;
            _type = type;
            Points = points;
            Position = position;
            Rotation = rotation;
            InnerRadius = innerRadius;
            OuterRadius = outerRadius;
            InnerRoundedness = innerRoundedness;
            OuterRoundedness = outerRoundedness;
        }

        internal virtual string Name { get; }

        internal new virtual Type GetType()
        {
            return _type;
        }

        internal virtual AnimatableFloatValue Points { get; }

        internal virtual IAnimatableValue<Vector2?, Vector2?> Position { get; }

        internal virtual AnimatableFloatValue Rotation { get; }

        internal virtual AnimatableFloatValue InnerRadius { get; }

        internal virtual AnimatableFloatValue OuterRadius { get; }

        internal virtual AnimatableFloatValue InnerRoundedness { get; }

        internal virtual AnimatableFloatValue OuterRoundedness { get; }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new PolystarContent(drawable, layer, this);
        }
    }
}