namespace LottieToWinComp.Expressions
{
    sealed class Number : Expression
    {
        public double Value { get; }
        public Number(double value)
        {
            Value = value;
        }

        public override string ToString() => ToString(Value);

        internal override bool IsAtomic => Value >= 0;

        public override Expression Simplified => this;
        internal static string ToString(double value) => ((float)value).ToString("0.######################################");
    }
}
