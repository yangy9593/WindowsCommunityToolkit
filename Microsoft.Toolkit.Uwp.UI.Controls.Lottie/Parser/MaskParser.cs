// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Animatable;
using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class MaskParser
    {
        internal static Mask Parse(JsonReader reader, LottieComposition composition)
        {
            Mask.MaskMode maskMode = Mask.MaskMode.MaskModeAdd;
            AnimatableShapeValue maskPath = null;
            AnimatableIntegerValue opacity = null;

            reader.BeginObject();
            while (reader.HasNext())
            {
                string mode = reader.NextName();
                switch (mode)
                {
                    case "mode":
                        switch (reader.NextString())
                        {
                            case "a":
                                maskMode = Mask.MaskMode.MaskModeAdd;
                                break;
                            case "s":
                                maskMode = Mask.MaskMode.MaskModeSubtract;
                                break;
                            case "i":
                                maskMode = Mask.MaskMode.MaskModeIntersect;
                                break;
                            default:
                                Debug.WriteLine($"Unknown mask mode {mode}. Defaulting to Add.", LottieLog.Tag);
                                maskMode = Mask.MaskMode.MaskModeAdd;
                                break;
                        }

                        break;
                    case "pt":
                        maskPath = AnimatableValueParser.ParseShapeData(reader, composition);
                        break;
                    case "o":
                        opacity = AnimatableValueParser.ParseInteger(reader, composition);
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            reader.EndObject();

            return new Mask(maskMode, maskPath, opacity);
        }
    }
}
