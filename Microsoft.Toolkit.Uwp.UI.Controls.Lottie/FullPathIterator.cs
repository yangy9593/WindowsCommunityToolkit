// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie
{
    internal class FullPathIterator : PathIterator
    {
        private readonly Path _path;

        private int _index;

        public FullPathIterator(Path path)
        {
            _path = path;
        }

        public override bool Next()
        {
            _index++;
            if (_index > _path.Contours.Count)
            {
                return false;
            }

            return true;
        }

        public override bool Done => _index >= _path.Contours.Count;

        public override ContourType CurrentSegment(float[] points)
        {
            var contour = _path.Contours[_index];

            for (var i = 0; i < contour.Points.Length; i++)
            {
                points[i] = contour.Points[i];
            }

            return contour.Type;
        }
    }
}