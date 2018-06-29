// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    internal abstract class PathIterator
    {
        public enum ContourType
        {
            Arc,
            MoveTo,
            Line,
            Close,
            Bezier
        }

        public abstract bool Next();

        public abstract bool Done { get; }

        public abstract ContourType CurrentSegment(float[] points);
    }
}