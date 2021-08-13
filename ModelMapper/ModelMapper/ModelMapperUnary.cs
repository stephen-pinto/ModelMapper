using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ModelMapper
{
    public class ModelMapperUnary<T> : HashSet<string>
        where T : class
    {
        private Dictionary<string, object> _defaults;

        public ModelMapperUnary()
        {
            _defaults = new Dictionary<string, object>();
        }

        public ModelMapperUnary<T> Add<Type>(Expression<Func<Type>> expr)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn = (MemberExpression)((expr.Body is MemberExpression) ? expr.Body : ((UnaryExpression)expr.Body).Operand);

            Add(memExprssn.Member.Name);

            return this;
        }

        public ModelMapperUnary<T> Add<Type>(Expression<Func<T, Type>> expr, object defaultValue = null)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn = (MemberExpression)((expr.Body is MemberExpression) ? expr.Body : ((UnaryExpression)expr.Body).Operand);

            Add(memExprssn.Member.Name);

            if (defaultValue != null)
                _defaults.Add(memExprssn.Member.Name, defaultValue);

            return this;
        }

        public bool HasChanges(T obj1, T obj2)
        {
            foreach (var props in obj1.GetType().GetProperties())
            {
                var prop1 = obj1.GetType().GetProperty(props.Name);
                var prop2 = obj2.GetType().GetProperty(props.Name);

                if (prop1.GetValue(obj1) != prop2.GetValue(obj2))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyAll(T sourceObj, T destinationObj, bool useDefaultIfNull = false)
        {
            foreach (var props in sourceObj.GetType().GetProperties())
            {
                var prop1 = sourceObj.GetType().GetProperty(props.Name);
                var prop2 = destinationObj.GetType().GetProperty(props.Name);

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

        public void CopyChanges(T sourceObj, T destinationObj, bool useDefaultIfNull = false)
        {
            foreach (var key in this)
            {
                var prop1 = sourceObj.GetType().GetProperty(key);
                var prop2 = destinationObj.GetType().GetProperty(key);

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
    }
}
