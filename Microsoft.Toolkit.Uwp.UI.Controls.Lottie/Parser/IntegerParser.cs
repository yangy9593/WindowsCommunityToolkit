// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal class IntegerParser : IValueParser<int?>
    {
        public static readonly IntegerParser Instance = new IntegerParser();

        public int? Parse(JsonReader reader, float scale)
        {
            return (int)Math.Round(JsonUtils.ValueFromObject(reader) * scale);
        }
    }
}
