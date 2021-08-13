using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace ModelMapper
{
    public class ModelMapperBiDir<T1, T2> : Dictionary<string, string>
        where T1 : class
        where T2 : class
    {
        #region Internal class

        public interface IModelMapperConfigBase
        {
            object DefaultValue { get; }
        }

        public interface IModelMapperConfig<BType, OType, CType> : IModelMapperConfigBase
        {
            IModelMapperConfig<BType, OType, CType> SetDefaultValue(BType value);
            IModelMapperConfig<BType, OType, CType> SetTypedValueConverter(Func<BType, OType> converter);
            IModelMapperConfig<BType, OType, CType> SetObjectValueConverter(Func<CType, OType> converter);
        }

        internal class ModelMapperConfig<BType, OType, CType> : IModelMapperConfig<BType, OType, CType>
        {
            //BType: Base  type 
            //OType: Other type
            //CType: Class type
            public string PropertyName { get; set; }
            public object DefaultValue { get; internal set; }
            public Func<BType, OType> ValueConverter1 { get; set; }
            public Func<CType, OType> ValueConverter2 { get; set; }

            public ModelMapperConfig(string propertyName)
            {
                PropertyName = propertyName;
            }

            public IModelMapperConfig<BType, OType, CType> SetDefaultValue(BType value)
            {
                DefaultValue = value;
                return this;
            }

            public IModelMapperConfig<BType, OType, CType> SetTypedValueConverter(Func<BType, OType> converter)
            {
                ValueConverter1 = converter;
                return this;
            }

            public IModelMapperConfig<BType, OType, CType> SetObjectValueConverter(Func<CType, OType> converter)
            {
                ValueConverter2 = converter;
                return this;
            }
        }

        #endregion

        public Dictionary<string, IModelMapperConfigBase> _propertyConfig;

        public ModelMapperBiDir()
        {
            _propertyConfig = new Dictionary<string, IModelMapperConfigBase>();
        }

        public ModelMapperBiDir<T1, T2> Add<BType>(Expression<Func<BType>> expr1, Expression<Func<BType>> expr2)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn1 = (MemberExpression)((expr1.Body is MemberExpression) ? expr1.Body : ((UnaryExpression)expr1.Body).Operand);
            var memExprssn2 = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);

            Add(memExprssn1.Member.Name, memExprssn2.Member.Name);

            return this;
        }

        public ModelMapperBiDir<T1, T2> Add<BType>(Expression<Func<T1, BType>> expr1, Expression<Func<T2, BType>> expr2)
        {
            //If UnaryExpression then get operand of Body else get direct Body as MemberExpression
            var memExprssn1 = (MemberExpression)((expr1.Body is MemberExpression) ? expr1.Body : ((UnaryExpression)expr1.Body).Operand);
            var memExprssn2 = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);

            Add(memExprssn1.Member.Name, memExprssn2.Member.Name);

            return this;
        }

        public ModelMapperBiDir<T1, T2> Add<BType, OType>(
            Expression<Func<T1, BType>> expr1,  //Property 1
            Expression<Func<T2, OType>> expr2,  //Proeprty 2
            Action<IModelMapperConfig<BType, OType, T1>> config1, //Configuration 1
            Action<IModelMapperConfig<OType, BType, T2>> config2) //Configuration 2
        {
            if (config1 == null && config2 == null)
                throw new ArgumentException($"Mandatory paramters {nameof(config1)} or {nameof(config2)} cannot be left null");

            //Save the converter for the property of type T1
            var memExprssn1 = (MemberExpression)((expr1.Body is MemberExpression) ? expr1.Body : ((UnaryExpression)expr1.Body).Operand);
            var propName1 = memExprssn1.Member.Name;
            if (config1 != null)
            {
                var props1 = new ModelMapperConfig<BType, OType, T1>(propName1);
                config1(props1);
                _propertyConfig.Add("T1_" + propName1, props1);
            }

            //Save the converter for the property of type T2
            var memExprssn2 = (MemberExpression)((expr2.Body is MemberExpression) ? expr2.Body : ((UnaryExpression)expr2.Body).Operand);
            var propName2 = memExprssn2.Member.Name;
            if (config2 != null)
            {
                var props2 = new ModelMapperConfig<OType, BType, T2>(propName2);
                config2(props2);
                _propertyConfig.Add("T2_" + propName2, props2);
            }

            //Save the mapping
            Add(propName1, propName2);

            return this;
        }

        public new void Clear()
        {
            base.Clear();
            _propertyConfig.Clear();
        }

        public void CopyChanges(T1 sourceObj, T2 destinationObj, bool useDefaults = false)
        {
            foreach (var kvp in this)
            {
                var prop1 = sourceObj.GetType().GetProperty(kvp.Key);
                var prop2 = destinationObj.GetType().GetProperty(kvp.Value);

                var propType1 = prop1.PropertyType;
                var propType2 = prop2.PropertyType;
                var key = "T1_" + prop1.Name;

                if (propType1 != propType2)
                {
                    if (!_propertyConfig.ContainsKey(key))
                        throw new InvalidOperationException($"No configuration defined for {prop1.Name}");

                    //Get strong PropertyMapperType
                    var strongConfigType = typeof(ModelMapperConfig<,,>).MakeGenericType(typeof(T1), typeof(T2), propType1, propType2, typeof(T1));

                    var value = GetConvertedValue(prop1, strongConfigType, sourceObj, _propertyConfig[key]);

                    //Set the converted value
                    prop2.SetValue(destinationObj, value);
                    //Expression.Call(castFunc1,  _properyConfig["T1_" + prop1.Name]);
                }
                else if (prop1.GetValue(sourceObj) != prop2.GetValue(destinationObj))
                {
                    //If useDefault is true and default value is specified then use that
                    if (prop1.GetValue(sourceObj) == null && useDefaults && _propertyConfig.ContainsKey(key))
                        prop2.SetValue(destinationObj, _propertyConfig[key].DefaultValue);
                    else
                        prop2.SetValue(destinationObj, prop1.GetValue(sourceObj));
                }
            }
        }

        public void CopyChanges(T2 sourceObj, T1 destinationObj, bool useDefaults = false)
        {
            foreach (var kvp in this)
            {
                var prop1 = sourceObj.GetType().GetProperty(kvp.Value);
                var prop2 = destinationObj.GetType().GetProperty(kvp.Key);

                var propType1 = prop1.PropertyType;
                var propType2 = prop2.PropertyType;
                var key = "T2_" + prop1.Name;

                if (propType1 != propType2)
                {
                    if (!_propertyConfig.ContainsKey(key))
                        throw new InvalidOperationException($"No configuration defined for {prop1.Name}");

                    //Get strong PropertyMapperType
                    var strongConfigType = typeof(ModelMapperConfig<,,>).MakeGenericType(typeof(T1), typeof(T2), propType1, propType2, typeof(T2));

                    var value = GetConvertedValue(prop1, strongConfigType, sourceObj, _propertyConfig[key]);

                    //Set the converted value
                    prop2.SetValue(destinationObj, value);
                    //Expression.Call(castFunc1,  _properyConfig["T1_" + prop1.Name]);
                }
                else if (prop1.GetValue(sourceObj) != prop2.GetValue(destinationObj))
                {
                    //If useDefault is true and default value is specified then use that
                    if (prop1.GetValue(sourceObj) == null && useDefaults && _propertyConfig.ContainsKey(key))
                        prop2.SetValue(destinationObj, _propertyConfig[key].DefaultValue);
                    else
                        prop2.SetValue(destinationObj, prop1.GetValue(sourceObj));
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

        private object GetConvertedValue(PropertyInfo propInfo, Type configStrongType, object valueSource, object config)
        {
            var propertyName = propInfo.Name;

            //Cast our instance to this specific strong type
            var configInstance = Convert.ChangeType(config, configStrongType);

            //Obtain the delgate converter 1 
            var delegateFunc = configStrongType.GetProperty("ValueConverter1").GetValue(configInstance);

            if (delegateFunc != null)
            {
                var invokeMethod = delegateFunc.GetType().GetMethod("Invoke");

                //Get the value by invoking deligate
                var value = invokeMethod.Invoke(delegateFunc, new object[] { propInfo.GetValue(valueSource) });

                return value;
            }

            //If 1 not defined check if delegate converter 2 is defined 
            delegateFunc = configStrongType.GetProperty("ValueConverter2").GetValue(configInstance);

            if (delegateFunc != null)
            {
                var invokeMethod = delegateFunc.GetType().GetMethod("Invoke");

                //Get the value by invoking deligate
                var value = invokeMethod.Invoke(delegateFunc, new object[] { valueSource });

                return value;
            }

            //If niether of them are defined raise exception
            throw new InvalidOperationException($"No ValueConverter registerd for {propertyName}");
        }
    }
}
