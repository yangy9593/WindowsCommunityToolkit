namespace LottieToWinComp.Expressions
{
    sealed class Name : Expression
    {
        readonly string _value;
        public Name(string value)
        {
            _value = value;
        }

        public override Expression Simplified => this;
        public override string ToString() => _value;

        internal override bool IsAtomic => true;
    }
}
