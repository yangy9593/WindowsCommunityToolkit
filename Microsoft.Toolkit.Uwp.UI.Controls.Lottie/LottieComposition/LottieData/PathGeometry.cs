using System;
using System.Collections.Generic;
using System.Linq;

namespace LottieData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class PathGeometry : IEquatable<PathGeometry>
    {
        public PathGeometry(
            Vector3 start,
            IEnumerable<BezierSegment> beziers,
            bool isClosed)
        {
            Start = start;
            Beziers = beziers;
            IsClosed = isClosed;
        }

        public Vector3 Start { get; }
        public IEnumerable<BezierSegment> Beziers { get; }
        public bool IsClosed { get; }

        public bool Equals(PathGeometry other) => 
            other != null &&
            Start.Equals(other.Start) && 
            IsClosed == other.IsClosed && 
            Enumerable.SequenceEqual(Beziers, other.Beziers, BezierSegment.EqualityComparer);
    }
}
