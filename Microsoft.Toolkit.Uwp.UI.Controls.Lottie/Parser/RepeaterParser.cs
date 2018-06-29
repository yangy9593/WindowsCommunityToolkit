// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class RepeaterParser
    {
        internal static Repeater Parse(JsonReader reader, LottieComposition composition)
        {
            string name = null;
            AnimatableFloatValue copies = null;
            AnimatableFloatValue offset = null;
            AnimatableTransform transform = null;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "c":
                        copies = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "o":
                        offset = AnimatableValueParser.ParseFloat(reader, composition, false);
                        break;
                    case "tr":
                        transform = AnimatableTransformParser.Parse(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new Repeater(name, copies, offset, transform);
        }
    }
}
