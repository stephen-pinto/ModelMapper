using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModelMapper
{
    public class ModelMapperNext<SrcType, DstType>
        where SrcType : class
        where DstType : class
    {
        private Dictionary<string, object> defaults;
        private List<Expression> assignmentExpressions = new List<Expression>();
        private ParameterExpression srcInstance = null;
        private ParameterExpression dstInstance = null;
        private Action<SrcType, DstType> ConverterFunction { get; set; }

        public ModelMapperNext()
        {
            defaults = new Dictionary<string, object>();

            //Declare parameters for the converter
            srcInstance = Expression.Parameter(typeof(SrcType), "srcObj");
            dstInstance = Expression.Parameter(typeof(DstType), "dstObj");
        }

        public ModelMapperNext<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> expr1, Expression<Func<DstType, ResType>> expr2, object defaultValue = null)
        {
            if (ConverterFunction != null)
                ConverterFunction = null;

            string targetMember = GetMemberName(expr2);

            var leftProp = Expression.Property(dstInstance, targetMember);
            var rightProp = GetAppropriateExpression(expr1, typeof(ResType));

            assignmentExpressions.Add(Expression.Assign(leftProp, rightProp));

            return this;
        }

        public ModelMapperNext<SrcType, DstType> Build()
        {
            var block = Expression.Block(assignmentExpressions.ToArray());
            var lamda = Expression.Lambda<Action<SrcType, DstType>>(block, new ParameterExpression[] { srcInstance, dstInstance });
            ConverterFunction = lamda.Compile();
            return this;
        }

        public void Clear()
        {
            assignmentExpressions.Clear();
            defaults.Clear();
        }

        public void CopyChanges(SrcType sourceObj, DstType destinationObj)
        {
            if (ConverterFunction == null)
                throw new InvalidOperationException("Build not called");

            ConverterFunction(sourceObj, destinationObj);
        }

        public DstType GetNew(SrcType srcObj)
        {
            DstType dstObj = Activator.CreateInstance<DstType>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }

        private Expression GetAppropriateExpression<T1>(Expression<T1> sourceExpression, Type resultType)
        {
            if (sourceExpression.Body is MemberExpression membrExpr)
            {
                string srcMember = GetMemberName(sourceExpression);
                return Expression.Property(srcInstance, srcMember);
            }
            else if (sourceExpression.Body is UnaryExpression unaryExpr)
            {
                return Expression.Property(srcInstance, ((MemberExpression)unaryExpr.Operand).Member.Name);
            }
            else if (sourceExpression.Body is ConstantExpression constant)
            {
                return constant;
            }
            else
            {
                //Since we allow only one param type explicit indexing should be fine
                var lamdaExprn = Expression.Lambda<Func<SrcType, object>>(sourceExpression.Body, sourceExpression.Parameters[0]);
                var invokeExpr = Expression.Invoke(lamdaExprn, new Expression[] { srcInstance });
                return Expression.Convert(invokeExpr, resultType);
            }
        }

        private string GetMemberName<T>(Expression<T> expression)
        {
            var targetExpr = (MemberExpression)((expression.Body is MemberExpression) ? expression.Body : ((UnaryExpression)expression.Body).Operand);
            return targetExpr.Member.Name;
        }
    }
}
