// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class AnimatableValueParser
    {
        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition)
        {
            return ParseFloat(reader, composition, true);
        }

        public static AnimatableFloatValue ParseFloat(JsonReader reader, LottieComposition composition, bool isDp)
        {
            return new AnimatableFloatValue(Parse(reader, isDp ? Utils.Utils.DpScale() : 1f, composition, FloatParser.Instance));
        }

        internal static AnimatableIntegerValue ParseInteger(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableIntegerValue(Parse(reader, composition, IntegerParser.Instance));
        }

        internal static AnimatablePointValue ParsePoint(JsonReader reader, LottieComposition composition)
        {
            return new AnimatablePointValue(Parse(reader, Utils.Utils.DpScale(), composition, PointFParser.Instance));
        }

        internal static AnimatableScaleValue ParseScale(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableScaleValue(Parse(reader, composition, ScaleXyParser.Instance));
        }

        internal static AnimatableShapeValue ParseShapeData(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableShapeValue(Parse(reader, Utils.Utils.DpScale(), composition, ShapeDataParser.Instance));
        }

        internal static AnimatableTextFrame ParseDocumentData(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableTextFrame(Parse(reader, composition, DocumentDataParser.Instance));
        }

        internal static AnimatableColorValue ParseColor(JsonReader reader, LottieComposition composition)
        {
            return new AnimatableColorValue(Parse(reader, composition, ColorParser.Instance));
        }

        internal static AnimatableGradientColorValue ParseGradientColor(JsonReader reader, LottieComposition composition, int points)
        {
            return new AnimatableGradientColorValue(Parse(reader, composition, new GradientColorParser(points)));
        }

        // Will return null if the animation can't be played such as if it has expressions.
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, 1, valueParser);
        }

        // Will return null if the animation can't be played such as if it has expressions.
        private static List<Keyframe<T>> Parse<T>(JsonReader reader, float scale, LottieComposition composition, IValueParser<T> valueParser)
        {
            return KeyframesParser.Parse(reader, composition, scale, valueParser);
        }
    }
}