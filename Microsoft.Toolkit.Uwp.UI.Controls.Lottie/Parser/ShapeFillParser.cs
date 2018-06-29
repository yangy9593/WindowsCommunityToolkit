// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class ShapeFillParser
    {
        internal static ShapeFill Parse(JsonReader reader, LottieComposition composition)
        {
            AnimatableColorValue color = null;
            bool fillEnabled = false;
            AnimatableIntegerValue opacity = null;
            string name = null;
            int fillTypeInt = 1;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        color = AnimatableValueParser.ParseColor(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    case "fillEnabled":
                        fillEnabled = reader.NextBoolean();
                        break;
                    case "r":
                        fillTypeInt = reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            var fillType = fillTypeInt == 1 ? PathFillType.Winding : PathFillType.EvenOdd;
            return new ShapeFill(name, fillEnabled, fillType, color, opacity);
        }
    }
}
