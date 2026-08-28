using System.Linq.Expressions;

namespace Tool
{
    public static class PredicateBuilder
    {
        /// <summary>true 初始谓词，用于And拼接种子</summary>
        public static Expression<Func<T, bool>> True<T>() => x => true;
        /// <summary>false初始谓词，用于Or拼接种子</summary>
        public static Expression<Func<T, bool>> False<T>() => x => false;

        public static Expression<Func<T, bool>> And<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            var param = left.Parameters[0];
            var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
            return Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left.Body, rightBody), param);
        }

        public static Expression<Func<T, bool>> Or<T>(
            Expression<Func<T, bool>> left,
            Expression<Func<T, bool>> right)
        {
            if (left == null) return right;
            if (right == null) return left;

            var param = left.Parameters[0];
            var rightBody = new ParameterReplacer(right.Parameters[0], param).Visit(right.Body);
            return Expression.Lambda<Func<T, bool>>(
                Expression.OrElse(left.Body, rightBody), param);
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public ParameterReplacer(ParameterExpression oldParam, ParameterExpression newParam)
            {
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return ReferenceEquals(node, _oldParam) ? _newParam : base.VisitParameter(node);
            }
        }
    }
}
