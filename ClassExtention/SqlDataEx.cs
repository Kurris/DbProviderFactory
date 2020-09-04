using DbProviderFactory.ORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Reflection;


namespace DbProviderFactory.ClassExtention
{
    public static class SqlDataEx
    {
        /// <summary>
        /// 数据验证
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dr">DataRow</param>
        internal static void Validate<T>(this DataRow dr) where T : BaseEntity, new()
        {
            if (dr.RowState == DataRowState.Deleted) return;

            string ClassName = typeof(T).Name;

            if (!DataCache.AttributeCache.TryGetValue(ClassName, out var PropAttrs)) return;

            List<PropertyInfo> props = MemberInfoEx.GetPropertyCache<T>();

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

                        string Value = dr[prop.GetCustomName()] + "";
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


        /// <summary>
        /// 根据DataReader获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T GetEntity<T>(this DbDataReader reader) where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = MemberInfoEx.GetPropertyCache<T>();

            T entity = new T();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType.Name.IndexOf("Boolean") == 0)
                {
                    prop.FastSetValue(entity, Convert.ToBoolean(reader[prop.GetCustomName()]));
                }
                else if (prop.PropertyType.Name.IndexOf("DateTime") == 0)
                {
                    prop.FastSetValue(entity, default(DateTime));
                }
                else if (prop.Name.Equals("Modifier", StringComparison.InvariantCultureIgnoreCase) && reader[prop.GetCustomName()] is DBNull)
                {
                    prop.FastSetValue(entity, string.Empty);
                }
                else
                {
                    object obj = reader[prop.GetCustomName()];

                    if (obj is DBNull)
                    {
                        if (prop.PropertyType.Name.IndexOf("String", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            obj = string.Empty;
                        }
                        else if (prop.PropertyType.Name.IndexOf("Int", StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            obj = 0;
                        }
                    }
                    prop.FastSetValue(entity, obj);
                }
            }
            return entity;
        }
    }
}
