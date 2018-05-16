using System;
using System.Collections.Generic;

namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class BezierSegment : IEquatable<BezierSegment>
    {
        public BezierSegment(Vector3 cp1, Vector3 cp2, Vector3 vertex)
        {
            ControlPoint1 = cp1;
            ControlPoint2 = cp2;
            Vertex = vertex;
        }

        public Vector3 ControlPoint1 { get; }
        public Vector3 ControlPoint2 { get; }
        public Vector3 Vertex { get; }

        public bool Equals(BezierSegment other) => EqualityComparer.Equals(this, other);

        internal static IEqualityComparer<BezierSegment> EqualityComparer { get; } = new Comparer();

        sealed class Comparer : IEqualityComparer<BezierSegment>
        {
            public bool Equals(BezierSegment x, BezierSegment y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x == null || y == null)
                {
                    return false;
                }

                return
                    x.Vertex.Equals(y.Vertex) &&
                    x.ControlPoint1.Equals(y.ControlPoint1) &&
                    x.ControlPoint2.Equals(y.ControlPoint2);
            }

            public int GetHashCode(BezierSegment obj)
            {
                if (obj == null)
                {
                    return 0;
                }

                return
                    obj.Vertex.GetHashCode() ^
                    obj.ControlPoint1.GetHashCode() ^
                    obj.ControlPoint2.GetHashCode();
            }
        }

    }
}
