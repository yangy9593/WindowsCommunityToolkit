namespace LottieToWinComp.Expressions
{
    sealed class UntypedExpression : Expression
    {
        readonly string _value;
        public UntypedExpression(string value)
        {
            _value = value;
        }

        public override Expression Simplified => this;
        public override string ToString() => _value;
    }
}
