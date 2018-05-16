namespace WinCompData
{
#if !WINDOWS_UWP
    public
#endif
    sealed class ExpressionAnimation : CompositionAnimation
    {
        internal ExpressionAnimation(string expression) : this(null, expression)
        {
        }

        ExpressionAnimation(ExpressionAnimation other, string expression) : base(other)
        {
            Expression = expression;
        }

        public string Expression { get; }

        public override CompositionObjectType Type => CompositionObjectType.ExpressionAnimation;

        internal override CompositionAnimation Clone() => new ExpressionAnimation(this, Expression);

        public override string ToString() => Expression;
    }
}
