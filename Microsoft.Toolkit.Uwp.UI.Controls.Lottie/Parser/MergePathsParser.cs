// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model.Content;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Parser
{
    internal static class MergePathsParser
    {
        internal static MergePaths Parse(JsonReader reader)
        {
            string name = null;
            MergePaths.MergePathsMode mode = MergePaths.MergePathsMode.Add;

            while (reader.HasNext())
            {
                switch (reader.NextName())
                {
                    case "nm":
                        name = reader.NextString();
                        break;
                    case "mm":
                        mode = (MergePaths.MergePathsMode)reader.NextInt();
                        break;
                    default:
                        reader.SkipValue();
                        break;
                }
            }

            return new MergePaths(name, mode);
        }
    }
}
