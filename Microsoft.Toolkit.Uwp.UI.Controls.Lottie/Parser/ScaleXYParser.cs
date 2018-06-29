// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Value;
using Newtonsoft.Json;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal class ScaleXyParser : IValueParser<ScaleXy>
    {
        public static readonly ScaleXyParser Instance = new ScaleXyParser();

        public ScaleXy Parse(JsonReader reader, float scale)
        {
            bool isArray = reader.Peek() == JsonToken.StartArray;
            if (isArray)
            {
                reader.BeginArray();
            }

            float sx = reader.NextDouble();
            float sy = reader.NextDouble();
            while (reader.HasNext())
            {
                reader.SkipValue();
            }

            if (isArray)
            {
                reader.EndArray();
            }

            return new ScaleXy(sx / 100f * scale, sy / 100f * scale);
        }
    }
}
