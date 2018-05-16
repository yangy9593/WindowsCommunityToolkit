namespace LottieToWinComp.Expressions
{
    sealed class Vector2 : Expression
    {
        public Expression X { get; }
        public Expression Y { get; }

        public Vector2(WinCompData.Sn.Vector2 value)
        {
            X = value.X;
            Y = value.Y;
        }

        internal Vector2(Expression x, Expression y)
        {
            X = x;
            Y = y;
        }

        public static implicit operator Vector2(WinCompData.Sn.Vector2 value) => new Vector2(value);

        public static Vector2 operator *(Vector2 left, double right) => new Vector2(Multiply(left.X, right), Multiply(left.Y, right));

        public override Expression Simplified => this;
        public override string ToString() => $"Vector2({Parenthesize(X)},{Parenthesize(Y)})";

        internal override bool IsAtomic => true;

    }
}
