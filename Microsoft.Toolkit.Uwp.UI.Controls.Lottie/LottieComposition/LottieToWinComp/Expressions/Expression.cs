namespace LottieToWinComp.Expressions
{
    abstract class Expression
    {
        internal Expression() { }

        /// <summary>
        /// A simplified form of the expression. May be the same as this.
        /// </summary>
        public virtual Expression Simplified => this;

        public static implicit operator Expression(double value) => new Number(value);

        public static implicit operator Expression(string value) => new UntypedExpression(value);

        public static implicit operator Expression(WinCompData.Sn.Vector2 value) => new Vector2(value);

        public static Expression Name(string name) => new Name(name);

        protected static Expression Squared(Expression expression) => new Squared(expression);

        protected static Expression Cubed(Expression expression) => new Cubed(expression);

        protected static Expression Sum(Expression a, Expression b) => new Sum(a, b);
        protected static Expression Sum(Expression a, Expression b, params Expression[] parameters)
        {
            var result = new Sum(a, b);
            foreach (var parameter in parameters)
            {
                result = new Sum(result, parameter);
            }
            return result;
        }
        protected static Number Sum(Number a, Number b) => new Number(a.Value + b.Value);

        protected static Expression Subtract(Expression a, Expression b) => new Subtract(a, b);

        protected static Expression Multiply(Expression a, Expression b) => new Multiply(a, b);
        protected static Expression Multiply(Expression a, Expression b, params Expression[] parameters)
        {
            var result = new Multiply(a, b);
            foreach (var parameter in parameters)
            {
                result = new Multiply(result, parameter);
            }
            return result;
        }

        /// <summary>
        /// True iff the string form of the expression can be unambigiously
        /// parsed without parentheses.
        /// </summary>
        internal virtual bool IsAtomic => false;

        protected static string Parenthesize(Expression expression) =>
            expression.IsAtomic ? expression.ToString() : $"({expression})";

        protected static bool IsZero(Expression expression)
        {
            if (expression is Number numberExpression)
            {
                return numberExpression.Value == 0;
            }
            else if (expression is Vector2 vector2Expression)
            {
                return IsZero(vector2Expression.X) && IsZero(vector2Expression.Y);
            }
            else
            {
                return false;
            }
        }

        protected static bool IsOne(Expression expression)
        {
            if (expression is Number numberExpression)
            {
                return numberExpression.Value == 1;
            }
            else if (expression is Vector2 vector2Expression)
            {
                return IsOne(vector2Expression.X) && IsOne(vector2Expression.Y);
            }
            else
            {
                return false;
            }
        }

    }

}