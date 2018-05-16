namespace LottieToWinComp.Expressions
{
    sealed class Multiply : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        internal Multiply(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        public override Expression Simplified
        {
            get
            {
                var a = Left.Simplified;
                var b = Right.Simplified;

                if (IsZero(a))
                {
                    return a;
                }
                if (IsZero(b))
                {
                    return b;
                }
                if (IsOne(a))
                {
                    return b;
                }
                if (IsOne(b))
                {
                    return a;
                }

                var numberA = a as Number;
                var numberB = b as Number;
                if (numberA != null && numberB != null)
                {
                    // They're both constants. Evaluate them.
                    return new Number(numberA.Value * numberB.Value);
                }

                return this;
            }
        }

        public override string ToString()
        {
            var a = Left.Simplified;
            var b = Right.Simplified;

            var aString = a is Multiply ? a.ToString() : Parenthesize(a);
            var bString = b is Multiply ?  b.ToString() : Parenthesize(b);

            return $"{aString} * {bString}";
        }
    }
}
