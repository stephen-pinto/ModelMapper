using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ModelMapper.Basic
{
    public class ModelMapperLite<SrcType, DstType> : IModelMapper<SrcType, DstType>
        where SrcType : class
        where DstType : class
    {
        private Dictionary<string, string> _memberMapping;
        private Dictionary<string, Func<SrcType, object>> _delegateMapping;
        private Dictionary<string, object> _defaults;

        public ModelMapperLite()
        {
            _memberMapping = new Dictionary<string, string>();
            _delegateMapping = new Dictionary<string, Func<SrcType, object>>();
            _defaults = new Dictionary<string, object>();
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination)
        {
            string targetMember = GetMemberName(destination);
            if (source.Body is MethodCallExpression || source.Body is ConstantExpression)
                AddMappingByMethodExpression(targetMember, source);
            else
                AddMappingByExpression(targetMember, source);
            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination)
        {
            string targetMember = GetMemberName(destination);

            if (source.Body is MethodCallExpression || source.Body is ConstantExpression)
                AddMappingByMethodExpression(targetMember, source);
            else
                AddMappingByExpression(targetMember, source);

            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class
        {
            string targetMember = GetMemberName(destination);
            if (source.Body is MethodCallExpression || source.Body is ConstantExpression)
                AddMappingByMethodExpression(targetMember, source);
            else
                AddMappingByExpression(targetMember, source);

            if(defaultValue != null)
                _defaults.Add(targetMember, defaultValue);

            return this;
        }

        public IModelMapper<SrcType, DstType> Add<ResType>(Expression<Func<SrcType, ResType>> source, Expression<Func<DstType, ResType>> destination, ResType defaultValue)
            where ResType : class
        {
            string targetMember = GetMemberName(destination);

            if (source.Body is MethodCallExpression || source.Body is ConstantExpression)
                AddMappingByMethodExpression(targetMember, source);
            else
                AddMappingByExpression(targetMember, source);

            if(defaultValue != null)
                _defaults.Add(targetMember, defaultValue);

            return this;
        }

        public IModelMapper<SrcType, DstType> Build()
        {
            if (!_memberMapping.Any() || !_delegateMapping.Any())
                throw new ArgumentException("No mapping provided for building");

            //Since this is just to achieve a common abstraction return simply
            return this;
        }

        public void Clear()
        {
            _memberMapping.Clear();
            _defaults.Clear();
        }

        public void CopyChanges(SrcType sourceObj, DstType destinationObj)
        {
            foreach (var kvp in _memberMapping)
            {
                var prop2 = destinationObj.GetType().GetProperty(kvp.Key);

                if (kvp.Value != null)
                {
                    var prop1 = sourceObj.GetType().GetProperty(kvp.Value);

                    if (prop1.GetValue(sourceObj) != prop2.GetValue(destinationObj))
                    {
                        if (prop1.GetValue(sourceObj) == null && _defaults.ContainsKey(prop1.Name))
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
                    prop2.SetValue(destinationObj, _delegateMapping[kvp.Key].Invoke(sourceObj));
                }
            }
        }

        public DstType GetNew(SrcType srcObj)
        {
            DstType dstObj = Activator.CreateInstance<DstType>();
            CopyChanges(srcObj, dstObj);
            return dstObj;
        }

        private void AddMappingByExpression<T1>(string targetMemberName, Expression<T1> sourceExpression)
        {
            if (sourceExpression.Body is MemberExpression membrExpr)
            {
                _memberMapping.Add(targetMemberName, membrExpr.Member.Name);
            }
            else if (sourceExpression.Body is UnaryExpression unaryExpr)
            {
                _memberMapping.Add(targetMemberName, ((MemberExpression)unaryExpr.Operand).Member.Name);
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
        }

        private void AddMappingByMethodExpression<ResType>(string targetMemberName, Expression<Func<SrcType, ResType>> source)
        {
            //Add null just for entry
            _memberMapping.Add(targetMemberName, null);

            //Since we allow only one param type explicit indexing should be fine
            var lamdaExprn = Expression.Lambda<Func<SrcType, object>>(source.Body, source.Parameters[0]);
            var lamdaFunc = lamdaExprn.Compile();

            //Add the new function to the list
            _delegateMapping.Add(targetMemberName, lamdaFunc);
        }

        private void AddMappingByMethodExpression<ResType>(string targetMemberName, Expression<Func<ResType>> source)
        {
            //Add null just for entry
            _memberMapping.Add(targetMemberName, null);

            var param = Expression.Parameter(typeof(SrcType));
            var lamdaExprn = Expression.Lambda<Func<SrcType, object>>(source.Body, param);
            var lamdaFunc = lamdaExprn.Compile();

            //Add the new function to the list
            _delegateMapping.Add(targetMemberName, lamdaFunc);
        }

        private string GetMemberName<T>(Expression<T> expression)
        {
            var targetExpr = (MemberExpression)((expression.Body is MemberExpression) ? expression.Body : ((UnaryExpression)expression.Body).Operand);
            return targetExpr.Member.Name;
        }

        private Func<object, object> Convert<T1, T2>(Func<T1, T2> func)
        {
            return p => func((T1)p);
        }        
    }
}
