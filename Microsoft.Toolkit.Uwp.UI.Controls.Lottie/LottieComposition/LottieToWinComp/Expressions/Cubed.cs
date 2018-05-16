namespace LottieToWinComp.Expressions
{
    /// <summary>
    /// Raises a value to the power of 3. 
    /// </summary>
    sealed class Cubed : Expression
    {
        public Cubed(Expression value)
        {
            Value = value;
        }

        public Expression Value { get; }

        public override Expression Simplified
        {
            get
            {
                var simplifiedValue = Value.Simplified;
                var numberValue = simplifiedValue as Number;
                return (numberValue != null)
                    ? new Number(numberValue.Value * numberValue.Value * numberValue.Value)
                    : (Expression)this;
            }
        }

        internal override bool IsAtomic => true;

        public override string ToString()
        {
            var simplifiedValue = Value.Simplified;

            return $"Pow({simplifiedValue}, 3)";
        }
    }
}
