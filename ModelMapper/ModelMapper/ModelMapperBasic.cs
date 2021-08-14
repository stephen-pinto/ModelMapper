using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModelMapper
{
    public class ModelMapperBasic<SrcType, DstType>
        where SrcType : class
        where DstType : class
    {
        private Dictionary<string, string> _memberMapping;
        private Dictionary<string, Delegate> _delegateMapping;
        private Dictionary<string, Func<SrcType, object>> _methodMapping;
        private Dictionary<string, object> _defaults;

        public ModelMapperBasic()
        {
            _memberMapping = new Dictionary<string, string>();
            _methodMapping = new Dictionary<string, Func<SrcType, object>>();
            _delegateMapping = new Dictionary<string, Delegate>();
            _defaults = new Dictionary<string, object>();
        }

        public ModelMapperBasic<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> expr1, Expression<Func<DstType>> expr2)
        {
            AddMappingByExpression(expr1, expr2);
            return this;
        }

        public ModelMapperBasic<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> expr1, Expression<Func<DstType, ResType>> expr2, object defaultValue = null)
        {
            string targetMember;

            if (expr1.Body is MethodCallExpression || expr1.Body is ConstantExpression)
                targetMember = AddMappingByMethodExpression(expr1, expr2);
            else
                targetMember = AddMappingByExpression(expr1, expr2);
            
            if (defaultValue != null)
                _defaults.Add(targetMember, defaultValue);

            return this;
        }

        private string AddMappingByMethodExpression<ResType>(Expression<Func<SrcType, ResType>> expr1, Expression<Func<DstType, ResType>> expr2)
        {
            var targetExpr = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);
            var targetMember = targetExpr.Member.Name;

            //Add null just for entry
            _memberMapping.Add(targetMember, null);

            //Since we allow only one param type explicit indexing should be fine
            var lamdaExprn = Expression.Lambda<Func<SrcType, object>>(expr1.Body, expr1.Parameters[0]);
            var lamdaFunc = lamdaExprn.Compile();

            //Add the new function to the list
            _methodMapping.Add(targetMember, lamdaFunc);

            return targetMember;
        }

        private string AddMappingByExpression<T1, T2>(Expression<T1> sourceExpression, Expression<T2> targetExpression)
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
            //else if (sourceExpression.Body is ConstantExpression)
            //{
            //    var delgt = Expression.Lambda(sourceExpression.Body).Compile();
            //    _memberMapping.Add(targetMember, null);
            //    _delegateMapping.Add(targetMember, delgt);
            //}
            else
            {
                throw new NotSupportedException($"Expression of type {sourceExpression.Type.Name} not supported yet");
            }

            return targetMember;
        }

        public void Clear()
        {
            _memberMapping.Clear();
            _defaults.Clear();
        }

        public void CopyChanges(SrcType sourceObj, DstType destinationObj, bool useDefaultIfNull = true)
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
                    //if (_methodMapping.ContainsKey(kvp.Key))
                    //{
                    //    _methodMapping[kvp.Key].Invoke(sourceObj);
                    //}

                    //If its not a simple member maybe its a delegate so execute it
                    //prop2.SetValue(destinationObj, _delegateMapping[kvp.Key].DynamicInvoke());
                    prop2.SetValue(destinationObj, _methodMapping[kvp.Key].Invoke(sourceObj));
                }
            }
        }

        public DstType GetNew(SrcType srcObj)
        {
            DstType dstObj = Activator.CreateInstance<DstType>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }

        private Func<object, object> Convert<T1, T2>(Func<T1, T2> func)
        {
            return p => func((T1)p);
        }
    }
}
