namespace LottieToWinComp.Expressions
{
    sealed class LessThen : Expression
    {
        public Expression Left { get; }
        public Expression Right { get; }

        public LessThen(Expression left, Expression right)
        {
            Left = left;
            Right = right;
        }

        // TODO - could be simplified to a constant bool in some circumstances.
        public override Expression Simplified => this;

        public override string ToString() => $"{Parenthesize(Left.Simplified)} < {Parenthesize(Right.Simplified)}";
    }
}
