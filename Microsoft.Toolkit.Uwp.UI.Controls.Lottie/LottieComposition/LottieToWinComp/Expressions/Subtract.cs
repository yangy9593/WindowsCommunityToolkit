namespace LottieToWinComp.Expressions
{
    sealed class Subtract : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }
        internal Subtract(Expression left, Expression right)
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
                if (IsZero(b))
                {
                    return a;
                }

                var numberA = a as Number;
                var numberB = b as Number;

                // If both are numbers, simplify to the calculated value.
                if (numberA != null && numberB != null)
                {
                    return new Number(numberA.Value - numberB.Value);
                }

                return this;
            }
        }
        public override string ToString() => $"{Parenthesize(Left.Simplified)} - {Parenthesize(Right.Simplified)}";
    }
}
