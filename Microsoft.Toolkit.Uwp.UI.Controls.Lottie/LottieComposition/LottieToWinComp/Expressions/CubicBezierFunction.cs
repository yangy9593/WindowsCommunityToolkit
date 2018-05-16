using SnVector2 = WinCompData.Sn.Vector2;

namespace LottieToWinComp.Expressions
{
    sealed class CubicBezierFunction : Expression
    {
        readonly SnVector2 _p0;
        readonly SnVector2 _p1;
        readonly SnVector2 _p2;
        readonly SnVector2 _p3;
        readonly Expression _t;

        public static CubicBezierFunction Create(SnVector2 controlPoint0, SnVector2 controlPoint1, SnVector2 controlPoint2, SnVector2 controlPoint3, Expression t)
        {
            return new CubicBezierFunction(controlPoint0, controlPoint1, controlPoint2, controlPoint3, t);
        }

        CubicBezierFunction(SnVector2 controlPoint0, SnVector2 controlPoint1, SnVector2 controlPoint2, SnVector2 controlPoint3, Expression t)
        {
            _p0 = controlPoint0;
            _p1 = controlPoint1;
            _p2 = controlPoint2;
            _p3 = controlPoint3;
            _t = t;
        }

        /// <summary>
        /// True iff all 4 control points are on the same line, or the segment between
        /// controlPoint0 and controlPoint3 is 0 length. A cubic bezier with colinear control 
        /// points can be replaced by a linear function from controlPoint0 to controlPoint3.
        /// </summary>
        public bool IsColinear
        {
            get
            {
                if (_p0.Equals(_p3))
                {
                    return true;
                }

                var p01X = _p0.X - _p1.X;
                var p01Y = _p0.Y - _p1.Y;

                var p02X = _p0.X - _p2.X;
                var p02Y = _p0.Y - _p2.Y;

                var p03X = _p0.X - _p3.X;
                var p03Y = _p0.Y - _p3.Y;

                if (p01Y == 0 || p02Y == 0 || p03Y == 0)
                {
                    // Can't divide by Y because it's 0 in at least one case. (i.e. horizontal line)
                    if (p01X == 0 || p02X == 0 || p03X == 0)
                    {
                        // Can't divide by X because it's 0 in at least one case (i.e. vertical line)
                        // The points can only be colinear if they're all equal.
                        return _p0 == _p1 && _p0 == _p2 && _p0 == _p3;
                    }
                    else
                    {
                        return (p01Y / p01X) == (p02Y / p02X) &&
                               (p01Y / p01X) == (p03Y / p03X);
                    }
                }
                else
                {
                    return (p01X / p01Y) == (p02X / p02Y) &&
                           (p01X / p01Y) == (p03X / p03Y);
                }

            }
        }

        // (1-t)^3P0 + 3(1-t)^2tP1 + 3(1-t)t^2P2 + t^3P3
        public override Expression Simplified
        {
            get
            {
                var OneMinusT = Subtract(1, _t);

                // (1-t)^3P0
                var p0Part = Multiply(Cubed(OneMinusT), _p0);

                // (1-t)^2t3P1
                var p1Part = Multiply(3, Squared(OneMinusT), _t, _p1);

                // (1-t)t^23P2
                var p2Part = Multiply(3, OneMinusT, Squared(_t), _p2);

                // t^3P3
                var p3Part = Multiply(Cubed(_t), _p3);

                return Sum(p0Part, p1Part, p2Part, p3Part).Simplified;
            }
        }

        public override string ToString() => Simplified.ToString();
    }
}
