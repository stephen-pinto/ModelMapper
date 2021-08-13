using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModelMapper
{
    public class ModelMapperBasic<T1, T2>
        where T1 : class
        where T2 : class
    {
        private Dictionary<string, string> _memberMapping;
        private Dictionary<string, Delegate> _delegateMapping;
        private Dictionary<string, object> _defaults;

        public ModelMapperBasic()
        {
            _memberMapping = new Dictionary<string, string>();
            _delegateMapping = new Dictionary<string, Delegate>();
            _defaults = new Dictionary<string, object>();            
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<Type>> expr1, Expression<Func<T2, Type>> expr2)
        {
            AddMappingByExpression(expr1, expr2);
            return this;
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<T1, Type>> expr1, Expression<Func<T2, Type>> expr2)
        {
            Add(expr1, expr2, null);
            return this;
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<T1, Type>> expr1, Expression<Func<T2, Type>> expr2, object defaultValue = null)
        {
            var targetMember = AddMappingByExpression(expr1, expr2);

            if (defaultValue != null)
                _defaults.Add(targetMember, defaultValue);

            return this;
        }

        private string AddMappingByExpression<Type1, Type2>(Expression<Type1> sourceExpression, Expression<Type2> targetExpression)
        {
            var targetExpr = (MemberExpression)((targetExpression.Body is MemberExpression) ? targetExpression.Body : ((UnaryExpression)targetExpression.Body).Operand);
            var targetMember = targetExpr.Member.Name;

            if (sourceExpression.Body is MemberExpression membrExpr)
            {
                _memberMapping.Add(targetMember, membrExpr.Member.Name);
            }
            else if (sourceExpression.Body is UnaryExpression unaryExpr)
            {
                _memberMapping.Add(targetMember, ((MemberExpression)unaryExpr.Operand).Member.Name);
            }           
            else if (sourceExpression.Body is MethodCallExpression || sourceExpression.Body is ConstantExpression)
            {
                var delgt = Expression.Lambda(sourceExpression.Body).Compile();
                _memberMapping.Add(targetMember, null);
                _delegateMapping.Add(targetMember, delgt);
            }
            else
            {
                throw new NotSupportedException($"{nameof(Type1)} not supported yet");
            }

            return targetMember;
        }

        public void Clear()
        {
            _memberMapping.Clear();
            _defaults.Clear();
        }

        public void CopyChanges(T1 sourceObj, T2 destinationObj, bool useDefaultIfNull = true)
        {
            foreach (var kvp in _memberMapping)
            {
                var prop2 = destinationObj.GetType().GetProperty(kvp.Key);

                if (kvp.Value != null)
                {
                    var prop1 = sourceObj.GetType().GetProperty(kvp.Value);

                    if (prop1.GetValue(sourceObj) != prop2.GetValue(destinationObj))
                    {
                        if (useDefaultIfNull && prop1.GetValue(sourceObj) == null && _defaults.ContainsKey(prop1.Name))
                        {
                            prop2.SetValue(destinationObj, _defaults[prop1.Name]);
                        }
                        else
                        {
                            prop2.SetValue(destinationObj, prop1.GetValue(sourceObj));
                        }
                    }
                }
                else
                {
                    //If its not a simple member maybe its a delegate so execute it
                    prop2.SetValue(destinationObj, _delegateMapping[kvp.Key].DynamicInvoke());                    
                }
            }
        }

        public T2 GetNew(T1 srcObj)
        {
            T2 dstObj = Activator.CreateInstance<T2>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }
    }
}
