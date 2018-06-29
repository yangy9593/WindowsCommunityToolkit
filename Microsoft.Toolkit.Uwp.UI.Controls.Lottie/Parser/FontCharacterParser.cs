// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class FontCharacterParser
    {
        internal static FontCharacter Parse(JsonReader reader, LottieComposition composition)
        {
            char character = '\0';
            int size = 0;
            double width = 0;
            string style = null;
            string fontFamily = null;
            List<ShapeGroup> shapes = new List<ShapeGroup>();

            reader.BeginObject();
            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "ch":
                        character = reader.NextString()[0];
                        break;
                    case "size":
                        size = reader.NextInt();
                        break;
                    case "w":
                        width = reader.NextDouble();
                        break;
                    case "style":
                        style = reader.NextString();
                        break;
                    case "fFamily":
                        fontFamily = reader.NextString();
                        break;
                    case "data":
                        reader.BeginObject();
                        while (reader.HasNext())
                        {
                            if ("shapes".Equals(reader.NextName()))
                            {
                                reader.BeginArray();
                                while (reader.HasNext())
                                {
                                    shapes.Add((ShapeGroup)ContentModelParser.Parse(reader, composition));
                                }

                                reader.EndArray();
                            }
                            else
                            {
                                reader.SkipValue();
                            }
                        }

                        reader.EndObject();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            reader.EndObject();

            return new FontCharacter(shapes, character, size, width, style, fontFamily);
        }
    }
}
