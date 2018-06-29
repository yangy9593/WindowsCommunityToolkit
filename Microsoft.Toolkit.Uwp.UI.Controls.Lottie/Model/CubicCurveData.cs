// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Numerics;

namespace Microsoft.Toolkit.Uwp.UI.Controls.Lottie.Model
{
    internal class CubicCurveData
    {
        private Vector2 _controlPoint1;
        private Vector2 _controlPoint2;
        private Vector2 _vertex;

        internal CubicCurveData()
        {
            _controlPoint1 = default(Vector2);
            _controlPoint2 = default(Vector2);
            _vertex = default(Vector2);
        }

        internal CubicCurveData(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 vertex)
        {
            _controlPoint1 = controlPoint1;
            _controlPoint2 = controlPoint2;
            _vertex = vertex;
        }

        internal virtual void SetControlPoint1(float x, float y)
        {
            _controlPoint1.X = x;
            _controlPoint1.Y = y;
        }

        internal virtual Vector2 ControlPoint1 => _controlPoint1;

        internal virtual void SetControlPoint2(float x, float y)
        {
            _controlPoint2.X = x;
            _controlPoint2.Y = y;
        }

        internal virtual Vector2 ControlPoint2 => _controlPoint2;

        internal virtual void SetVertex(float x, float y)
        {
            _vertex.X = x;
            _vertex.Y = y;
        }

        internal virtual Vector2 Vertex => _vertex;
    }
}