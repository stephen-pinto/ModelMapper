using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ModelMapper
{
    public class ModelMapperBasic<T1, T2> : Dictionary<string, string>
        where T1 : class
        where T2 : class
    {
        private Dictionary<string, object> _defaults;
        
        public ModelMapperBasic()
        {
            _defaults = new Dictionary<string, object>();
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<Type>> expr1, Expression<Func<Type>> expr2)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn1 = (MemberExpression)((expr1.Body is MemberExpression) ? expr1.Body : ((UnaryExpression)expr1.Body).Operand);
            var memExprssn2 = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);

            Add(memExprssn1.Member.Name, memExprssn2.Member.Name);
            return this;
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<T1, Type>> expr1, Expression<Func<T2, Type>> expr2)
        {
            Add(expr1, expr2, null);
            return this;
        }

        public ModelMapperBasic<T1, T2> Add<Type>(Expression<Func<T1, Type>> expr1, Expression<Func<T2, Type>> expr2, object defaultValue = null)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn1 = (MemberExpression)((expr1.Body is MemberExpression) ? expr1.Body : ((UnaryExpression)expr1.Body).Operand);
            var memExprssn2 = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);

            Add(memExprssn1.Member.Name, memExprssn2.Member.Name);

            if (defaultValue != null)
                _defaults.Add(memExprssn1.Member.Name, defaultValue);

            return this;
        }

        public new void Clear()
        {
            base.Clear();
            _defaults.Clear();
        }

        public void CopyChanges(T1 sourceObj, T2 destinationObj, bool useDefaultIfNull = true)
        {
            foreach (var kvp in this)
            {
                var prop1 = sourceObj.GetType().GetProperty(kvp.Key);
                var prop2 = destinationObj.GetType().GetProperty(kvp.Value);

                //var val1 = prop1.GetValue(sourceObj);
                //var val2 = prop2.GetValue(destinationObj);

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
        }

        public void CopyChanges(T2 sourceObj, T1 destinationObj, bool useDefaultIfNull = true)
        {
            foreach (var kvp in this)
            {
                var prop1 = sourceObj.GetType().GetProperty(kvp.Value);
                var prop2 = destinationObj.GetType().GetProperty(kvp.Key);

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
        }

        public T1 GetNew(T2 srcObj)
        {
            T1 dstObj = Activator.CreateInstance<T1>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }

        public T2 GetNew(T1 srcObj)
        {
            T2 dstObj = Activator.CreateInstance<T2>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }
    }
}
