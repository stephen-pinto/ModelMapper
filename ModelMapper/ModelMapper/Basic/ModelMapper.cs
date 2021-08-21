using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ModelMapper.Basic
{
    public class ModelMapper<SrcType, DstType> : IModelMapper<SrcType, DstType>
        where SrcType : class
        where DstType : class
    {
        private List<string> destMemberUniqueNames = new List<string>();
        private List<Expression> assignmentExpressions = new List<Expression>();
        private ParameterExpression srcInstance = null;
        private ParameterExpression dstInstance = null;

        private Action<SrcType, DstType> CopyFuction { get; set; }

        public ModelMapper()
        {
            destMemberUniqueNames = new List<string>();
            assignmentExpressions = new List<Expression>();

            //Declare parameters for the converter
            srcInstance = Expression.Parameter(typeof(SrcType), "srcObj");
            dstInstance = Expression.Parameter(typeof(DstType), "dstObj");
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination)
        {
            var (targetMember, leftProp) = ValidateTargetBeforeAdding(destination);

            var rightProp = GetAppropriateExpression(source, typeof(ResType));

            assignmentExpressions.Add(Expression.Assign(leftProp, rightProp));

            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination)
        {
            var (targetMember, leftProp) = ValidateTargetBeforeAdding(destination);

            var rightProp = GetAppropriateExpression(source, typeof(ResType));

            assignmentExpressions.Add(Expression.Assign(leftProp, rightProp));

            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class
        {
            var (targetMember, leftProp) = ValidateTargetBeforeAdding(destination);

            var rightProp = GetAppropriateExpression(source, typeof(ResType));
            var rightPropDefault = Expression.Constant(defaultValue);

            assignmentExpressions.Add(Expression.Assign(leftProp, Expression.Coalesce(rightProp, rightPropDefault)));

            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class
        {
            var (targetMember, leftProp) = ValidateTargetBeforeAdding(destination);

            var rightProp = GetAppropriateExpression(source, typeof(ResType));
            var rightPropDefault = Expression.Constant(defaultValue);

            assignmentExpressions.Add(Expression.Assign(leftProp, Expression.Coalesce(rightProp, rightPropDefault)));

            return this;
        }

        public IModelMapper<SrcType, DstType> Build()
        {
            if (!assignmentExpressions.Any())
                throw new ArgumentException("No mapping provided for building");

            var block = Expression.Block(assignmentExpressions.ToArray());
            var lamda = Expression.Lambda<Action<SrcType, DstType>>(block, new ParameterExpression[] { srcInstance, dstInstance });
            CopyFuction = lamda.Compile();
            return this;
        }

        public void Clear()
        {
            assignmentExpressions.Clear();
            destMemberUniqueNames.Clear();
        }

        public void CopyChanges(SrcType sourceObj, DstType destinationObj)
        {
            if (CopyFuction == null)
                throw new InvalidOperationException("Build not called");

            //Invoke the compile function
            CopyFuction(sourceObj, destinationObj);
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

        private Expression GetAppropriateExpression<ResType>(Expression<Func<ResType>> sourceExpression, Type resultType)
        {
            var param = Expression.Parameter(typeof(SrcType));
            var lamdaExprn = Expression.Lambda<Func<SrcType, object>>(sourceExpression.Body, param);
            var invokeExpr = Expression.Invoke(lamdaExprn, new Expression[] { srcInstance });
            return Expression.Convert(invokeExpr, resultType);
        }

        private Tuple<string, MemberExpression> ValidateTargetBeforeAdding<ResType>(Expression<Func<DstType, ResType>> expr2)
        {
            string targetMember = GetMemberName(expr2);

            if (destMemberUniqueNames.Contains(targetMember))
                throw new InvalidOperationException("Cant contain multiple sources for one destination");

            if (CopyFuction != null)
                CopyFuction = null;

            destMemberUniqueNames.Add(targetMember);
            return Tuple.Create(targetMember, Expression.Property(dstInstance, targetMember));
        }

        private string GetMemberName<T>(Expression<T> expression)
        {
            var targetExpr = (MemberExpression)((expression.Body is MemberExpression) ? expression.Body : ((UnaryExpression)expression.Body).Operand);
            return targetExpr.Member.Name;
        }
    }
}
