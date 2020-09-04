using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DbProviderFactory.ORM;


namespace DbProviderFactory.ClassExtention
{
    /// <summary>
    /// 属性扩展方法
    /// </summary>
    public static class MemberInfoEx
    {
        /// <summary>
        /// 快速赋值属性
        /// </summary>
        /// <typeparam name="TObj">对象类型</typeparam>
        /// <typeparam name="TVal">值类型</typeparam>
        /// <param name="prop">属性</param>
        /// <param name="obj">对象实例</param>
        /// <param name="value">值</param>
        public static void FastSetValue<TObj, TVal>(this PropertyInfo prop, TObj obj, TVal value) where TObj : new()
        {
            Type type = obj.GetType();

            var instance = Expression.Parameter(type, "instance");
            var valType = Expression.Parameter(typeof(TVal), "val");

            var convertValType = Expression.Convert(valType, prop.PropertyType);

            MethodInfo method = prop.GetSetMethod(true);

            var MethodExpression = Expression.Call(instance, method, convertValType);

            Expression.Lambda<Action<TObj, TVal>>(MethodExpression, new[] { instance, valType }).Compile().Invoke(obj, value);
        }

        /// <summary>
        /// 快速获取属性值
        /// </summary>
        /// <typeparam name="TObj">对象类型</typeparam>
        /// <param name="prop">属性</param>
        /// <param name="obj">对象实例</param>
        /// <returns>属性值</returns>
        public static object FastGetValue<TObj>(this PropertyInfo prop, TObj obj) where TObj : new()
        {
            Type type = obj.GetType();

            var instance = Expression.Parameter(typeof(TObj), "instance");

            var body_obj = Expression.Convert(instance, type);

            var body = Expression.Property(body_obj, prop);

            var finishEpr = Expression.Convert(body, typeof(object));

            return Expression.Lambda<Func<TObj, object>>(finishEpr, instance).Compile().Invoke(obj);
        }

        /// <summary>
        /// 通过当前操作枚举获取特定的属性缓存
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="status">当前操作</param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetPropertyCache<T>() where T : BaseEntity, new()//OperatorStatus status) where T : BaseEntity, new()
        {
            #region Void

            //string Key = typeof(T).Name;

            //if (!DataCache.PropertyCache.ContainsKey(Key + Enum.GetName(typeof(OperatorStatus), status)))
            //{
            //    List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

            //    switch (status)
            //    {
            //        case OperatorStatus.Insert:
            //            properties.RemoveAll(p => p.IsDefined(typeof(KeyAttribute), false)
            //                                     || p.Name.Equals("Modifier", StringComparison.InvariantCultureIgnoreCase)
            //                                     || p.Name.Equals("ModifyTime", StringComparison.InvariantCultureIgnoreCase));
            //            break;

            //        case OperatorStatus.Update:
            //            properties.RemoveAll(p => p.IsDefined(typeof(KeyAttribute), false)
            //                                    || p.Name.Equals("Creator", StringComparison.InvariantCultureIgnoreCase)
            //                                   || p.Name.Equals("CreateTime", StringComparison.InvariantCultureIgnoreCase));
            //            break;
            //    }

            //    DataCache.PropertyCache.Add(Key + Enum.GetName(typeof(OperatorStatus), status), properties);
            //}

            //return DataCache.PropertyCache[Key + Enum.GetName(typeof(OperatorStatus), status)]; 
            #endregion

            string sClassName = typeof(T).Name;

            if (!DataCache.PropertyCache.ContainsKey(sClassName))
            {
                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

                DataCache.PropertyCache.Add(sClassName, properties);
            }

            return DataCache.PropertyCache[sClassName];
        }

        /// <summary>
        /// 获取属性的自定义名称
        /// </summary>
        /// <param name="memberInfo">类成员</param>
        /// <returns>自定义名称</returns>
        internal static string GetCustomName(this MemberInfo memberInfo)
        {
            //缓存Key
            string Key;

            //类名+属性名
            if (memberInfo.DeclaringType != null)
            {
                Key = memberInfo.DeclaringType.Name + "|" + memberInfo.Name;
            }
            //类名
            else
            {
                Key = memberInfo.Name;
            }

            //存在
            if (DataCache.CustomNameCache.ContainsKey(Key)) return DataCache.CustomNameCache[Key];

            string sName = memberInfo.Name;

            if (memberInfo.IsDefined(typeof(CustomBaseAttribute), false))
            {
                var Attributes = memberInfo.GetCustomAttributes(typeof(CustomBaseAttribute), false) as CustomBaseAttribute[];

                foreach (CustomBaseAttribute attr in Attributes)
                {
                    if ((attr is FieldAttribute) || (attr is TableAttribute))
                    {
                        memberInfo.SaveAttribute(attr);

                        sName = attr.FieldName;

                        break;
                    }
                }
            }
            DataCache.CustomNameCache.Add(Key, sName);

            return sName;
        }

        /// <summary>
        /// 保存特性到缓存中
        /// </summary>
        /// <param name="memberInfo">类成员</param>
        /// <param name="attribute">特性</param>
        internal static void SaveAttribute(this MemberInfo memberInfo, CustomBaseAttribute attribute)
        {
            if (memberInfo.DeclaringType == null) return;

            string ClassName = memberInfo.DeclaringType.Name;

            if (!DataCache.AttributeCache.ContainsKey(ClassName))
            {
                DataCache.AttributeCache.Add(ClassName, new Dictionary<string, List<CustomBaseAttribute>>());
            }
            if (!DataCache.AttributeCache[ClassName].ContainsKey(memberInfo.Name))
            {
                DataCache.AttributeCache[ClassName].Add(memberInfo.Name, new List<CustomBaseAttribute>());
            }

            if (!DataCache.AttributeCache[ClassName][memberInfo.Name].Contains(attribute))
            {
                DataCache.AttributeCache[ClassName][memberInfo.Name].Add(attribute);
            }
        }

        /// <summary>
        /// 实体数据验证
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="Entity">实体</param>
        internal static void Validate<T>(this T Entity) where T : BaseEntity, new()
        {
            if (DataCache.AttributeCache.Count == 0)
            {
                return;
            }
            string ClassName = typeof(T).Name;

            if (!DataCache.AttributeCache.TryGetValue(ClassName, out var PropAttrs)) return;

            List<PropertyInfo> props = GetPropertyCache<T>();

            List<ErrorInfo> Errs = new List<ErrorInfo>();

            foreach (PropertyInfo prop in props)
            {
                if (!PropAttrs.TryGetValue(prop.Name, out var attributes)) continue;

                foreach (CustomBaseAttribute attr in attributes)
                {
                    if (attr is FieldAttribute FieldAttr)
                    {
                        string FieldName = string.IsNullOrEmpty(DataCache.CustomNameCache[ClassName + "|" + prop.Name])
                                ? prop.Name
                                : DataCache.CustomNameCache[ClassName + "|" + prop.Name];

                        string Value = prop.FastGetValue(Entity) + "";
                        if (string.IsNullOrEmpty(Value) && !FieldAttr.Empty)
                        {
                            Errs.Add(new ErrorInfo(FieldName, "不能为空!"));
                        }

                        if (FieldAttr.Length > 0)
                        {
                            if (Value.Length > FieldAttr.Length)
                            {
                                Errs.Add(new ErrorInfo(FieldName, string.IsNullOrEmpty(FieldAttr.ErrorMessage)
                                    ? "长度超出限定范围,范围:" + FieldAttr.Length
                                    : FieldAttr.ErrorMessage));
                            }
                        }
                    }
                }
            }

            if (Errs.Count > 0)
            {
                throw new SqlFieldsException(Errs);
            }
        }
    }
}
