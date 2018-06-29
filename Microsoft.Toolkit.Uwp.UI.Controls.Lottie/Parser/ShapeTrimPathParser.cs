// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class ShapeTrimPathParser
    {
        internal static ShapeTrimPath Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            ShapeTrimPath.Type type = ShapeTrimPath.Type.Simultaneously;
            AnimatableFloatValue start = null;
            AnimatableFloatValue end = null;
            AnimatableFloatValue offset = null;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "s":
                        start = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "e":
                        end = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "o":
                        offset = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "m":
                        type = (ShapeTrimPath.Type)reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new ShapeTrimPath(name, type, start, end, offset);
        }
    }
}
