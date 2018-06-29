// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Animation.Content;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Layer;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content
{
    internal class ShapeStroke : IContentModel
    {
        public enum LineCapType
        {
            Butt,
            Round,
            Unknown
        }

        internal static CanvasCapStyle LineCapTypeToPaintCap(LineCapType lineCapType)
        {
            switch (lineCapType)
            {
                case LineCapType.Butt:
                    return CanvasCapStyle.Flat;
                case LineCapType.Round:
                    return CanvasCapStyle.Round;
                case LineCapType.Unknown:
                default:
                    return CanvasCapStyle.Square;
            }
        }

        public enum LineJoinType
        {
            Miter,
            Round,
            Bevel
        }

        internal static CanvasLineJoin LineJoinTypeToPaintLineJoin(LineJoinType lineJoinType)
        {
            switch (lineJoinType)
            {
                case LineJoinType.Bevel:
                    return CanvasLineJoin.Bevel;
                case LineJoinType.Miter:
                    return CanvasLineJoin.Miter;
                case LineJoinType.Round:
                default:
                    return CanvasLineJoin.Round;
            }
        }

        internal ShapeStroke(string name, AnimatableFloatValue offset, List<AnimatableFloatValue> lineDashPattern, AnimatableColorValue color, AnimatableIntegerValue opacity, AnimatableFloatValue width, LineCapType capType, LineJoinType joinType)
        {
            Name = name;
            DashOffset = offset;
            LineDashPattern = lineDashPattern;
            Color = color;
            Opacity = opacity;
            Width = width;
            CapType = capType;
            JoinType = joinType;
        }

        public IContent ToContent(LottieDrawable drawable, BaseLayer layer)
        {
            return new StrokeContent(drawable, layer, this);
        }

        internal virtual string Name { get; }

        internal virtual AnimatableColorValue Color { get; }

        internal virtual AnimatableIntegerValue Opacity { get; }

        internal virtual AnimatableFloatValue Width { get; }

        internal virtual List<AnimatableFloatValue> LineDashPattern { get; }

        internal virtual AnimatableFloatValue DashOffset { get; }

        internal virtual LineCapType CapType { get; }

        internal virtual LineJoinType JoinType { get; }
    }
}