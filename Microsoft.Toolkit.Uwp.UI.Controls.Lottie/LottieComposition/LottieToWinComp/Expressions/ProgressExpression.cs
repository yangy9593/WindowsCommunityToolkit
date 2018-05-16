using System;
using System.Linq;

namespace LottieToWinComp.Expressions
{
    static class ProgressExpression
    {

        /// <summary>
        /// A segment of a progress expression. Defines the expression that is to be
        /// evaluated between two progress values.
        /// </summary>
        internal sealed class Segment
        {
            public Segment(double fromProgress, double toProgress, Expression value)
            {
                Value = value;
                FromProgress = fromProgress;
                ToProgress = toProgress;
            }

            /// <summary>
            /// Defines the value for a progress expression over this segment.
            /// </summary>
            public Expression Value { get; }
            public double FromProgress { get; }
            public double ToProgress { get; }
        }


        internal static Expression CreateProgressExpression(Expression progress, params Segment[] segments)
        {
            // Verify that the segments are contiguous and start <= 0 and end >= 1
            var orderedSegments = segments.OrderBy(e => e.FromProgress).ToArray();
            if (orderedSegments.Length == 0)
            {
                throw new ArgumentException();
            }

            double previousTo = orderedSegments[0].FromProgress;
            int? firstSegmentIndex = null;
            int? lastSegmentIndex = null;

            for (var i = 0; i < orderedSegments.Length && !lastSegmentIndex.HasValue; i++)
            {
                var cur = orderedSegments[i];
                if (cur.FromProgress != previousTo)
                {
                    throw new ArgumentException("Progress expression is not contiguous.");
                }
                previousTo = cur.ToProgress;

                // If the segment includes 0, it is the first segment.
                if (!firstSegmentIndex.HasValue)
                {
                    if (cur.FromProgress <= 0 && cur.ToProgress > 0)
                    {
                        firstSegmentIndex = i;
                    }
                }

                // If the segment includes 1, it is the last segment.
                if (!lastSegmentIndex.HasValue)
                {
                    if (cur.ToProgress >= 1)
                    {
                        lastSegmentIndex = i;
                    }
                }
            }

            if (!firstSegmentIndex.HasValue || !lastSegmentIndex.HasValue)
            {
                throw new ArgumentException("Progress expression is not fully defined.");
            }

            // Include only the segments that are >= 0 or <= 1.
            return CreateProgressExpression(
                new ArraySegment<Segment>(
                    array: orderedSegments,
                    offset: firstSegmentIndex.Value,
                    count: 1 + lastSegmentIndex.Value - firstSegmentIndex.Value), progress);
        }

        static Expression CreateProgressExpression(ArraySegment<Segment> segments, Expression progress)
        {
            switch (segments.Count)
            {
                case 0:
                    throw new ArgumentException();
                case 1:
                    return segments.Array[segments.Offset].Value;
                default:
                    // Divide the list of expressions into 2 segments.
                    var pivot = segments.Count / 2;
                    var segmentsArray = segments.Array;
                    var expression0 = CreateProgressExpression(new ArraySegment<Segment>(segmentsArray, segments.Offset, pivot), progress);
                    var expression1 = CreateProgressExpression(new ArraySegment<Segment>(segmentsArray, segments.Offset + pivot, segments.Count - pivot), progress);
                    var pivotProgress = segmentsArray[segments.Offset + pivot - 1].ToProgress;
                    return new Ternary(
                        condition: new LessThen(progress, new Number(pivotProgress)),
                        trueValue: expression0,
                        falseValue: expression1);
            }
        }

    }
}
