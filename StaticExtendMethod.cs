using HRRobot.Base.Ado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

/*修改时间                               修改人                                 修改内容
 *20200413                              ligy                                  create 静态方法
 *20200429                              ligy                                   增加获取list实体的方法
 *20200429                              ligy                                   增加GetEntity扩展方法重载,将datarow转成实体 
 *20200506                              ligy                                   属性缓存添加类型+操作方式为标志 
 *20200506                              ligy                                  datarow的扩展方法GetEntity方法暂时不会再使用 
 *20200508                              ligy                                   datarow ,不验证被删除的行 
 ***************************************************************************************************************/

namespace HRRobot.Base.ORM
{
    public static class StaticExtendMethod
    {
        #region 静态缓存
        /// <summary>
        /// 关于不同操作产生的不同的属性缓存
        /// </summary>
        internal static Dictionary<string, List<PropertyInfo>>
            _PropertyCache = new Dictionary<string, List<PropertyInfo>>();

        /// <summary>
        /// 属性的特性记录缓存
        /// </summary>
        internal static Dictionary<string, Dictionary<string, List<CustomBaseAttribute>>>
            _PropertyAttributeCache = new Dictionary<string, Dictionary<string, List<CustomBaseAttribute>>>();

        /// <summary>
        /// 属性自定义名称的缓存
        /// </summary>
        internal static Dictionary<string, string>
       _CustomNameCache = new Dictionary<string, string>();

        #endregion 静态缓存

        #region 属性赋值方法

        /// <summary>
        /// 通过表达式目录树快速设置属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="prop">属性</param>
        /// <param name="t">实体</param>
        /// <param name="value">值</param>
        internal static void FastSetValue<T>(this PropertyInfo prop, T t, object value) where T : BaseEntity, new()
        {
            Type type = t.GetType();

            var param_obj = Expression.Parameter(type);
            var param_val = Expression.Parameter(typeof(object));
            var body_obj = Expression.Convert(param_obj, type);
            var body_val = Expression.Convert(param_val, prop.PropertyType);

            MethodInfo method = prop.GetSetMethod();

            var body = Expression.Call(param_obj, method, body_val);

            Action<T, object> set = Expression.Lambda<Action<T, object>>(body, param_obj, param_val).Compile();

            set.Invoke(t, value);
        }

        #endregion 属性赋值方法

        #region 属性取值方法

        /// <summary>
        /// 通过表达式目录树快速获取属性值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="prop">属性</param>
        /// <param name="t">实体</param>
        /// <returns>object</returns>
        internal static object FastGetValue<T>(this PropertyInfo prop, T t) where T : BaseEntity, new()
        {
            try
            {
                Type type = t.GetType();

                var param_obj = Expression.Parameter(typeof(T));

                var body_obj = Expression.Convert(param_obj, type);

                var body = Expression.Property(body_obj, prop);

                var finishEpr = Expression.Convert(body, typeof(object));

                var getValue = Expression.Lambda<Func<T, object>>(finishEpr, param_obj).Compile();

                return getValue(t);
            }
            catch (Exception)
            {
                throw;
            }
        }

        #endregion 属性取值方法

        #region 保存属性特性

        /// <summary>
        /// 保存属性的特性到缓存中
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="Prop">属性</param>
        /// <param name="attribute">特性</param>
        internal static void SavePropAttributeCache<T>(T Prop, CustomBaseAttribute attribute) where T : MemberInfo
        {
            if (Prop.DeclaringType == null)
            {
                return;
            }
            string typeName = Prop.DeclaringType.Name;

            if (!_PropertyAttributeCache.ContainsKey(typeName))
            {
                _PropertyAttributeCache.Add(typeName, new Dictionary<string, List<CustomBaseAttribute>>());
            }

            if (!_PropertyAttributeCache[typeName].ContainsKey(Prop.Name))
            {
                _PropertyAttributeCache[typeName].Add(Prop.Name, new List<CustomBaseAttribute>());
            }

            _PropertyAttributeCache[typeName][Prop.Name].Add(attribute);
        }

        #endregion 保存属性特性

        #region 取自定义名称

        /// <summary>
        /// 获取属性的自定义名称
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="Prop">属性</param>
        /// <returns>sql字段名称</returns>
        internal static string GetCustomName<T>(this T Prop) where T : MemberInfo
        {
            if (_CustomNameCache.ContainsKey(Prop.Name))
            {
                return _CustomNameCache[Prop.Name];
            }

            string sName = Prop.Name;

            if (Prop.IsDefined(typeof(CustomBaseAttribute), false))
            {
                CustomBaseAttribute[] Attributes = Prop.GetCustomAttributes(typeof(CustomBaseAttribute), false) as CustomBaseAttribute[];

                foreach (CustomBaseAttribute attr in Attributes)
                {
                    if ((attr is FieldAttribute) || (attr is TableAttribute))
                    {
                        SavePropAttributeCache(Prop, attr);

                        sName = attr.FieldName;

                        break;
                    }
                }
            }
            _CustomNameCache.Add(Prop.Name, sName);

            return sName;
        }

        #endregion 取自定义名称

        #region 属性缓存

        /// <summary>
        /// 通过当前操作枚举获取特定的属性缓存
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="status">当前操作</param>
        /// <returns></returns>
        internal static List<PropertyInfo> GetPropertyCache<T>(OperatorStatus status) where T : BaseEntity, new()
        {
            string Key = typeof(T).Name;

            if (!_PropertyCache.ContainsKey(Key + Enum.GetName(typeof(OperatorStatus), status)))
            {
                List<PropertyInfo> properties = typeof(T).GetProperties().ToList();

                switch (status)
                {
                    case OperatorStatus.Insert:
                        properties.RemoveAll(p => p.IsDefined(typeof(KeyAttribute), false)
                                                 || p.Name.Equals("Modifier", StringComparison.InvariantCultureIgnoreCase)
                                                 || p.Name.Equals("ModifyTime", StringComparison.InvariantCultureIgnoreCase));
                        break;

                    case OperatorStatus.Update:
                        properties.RemoveAll(p => p.IsDefined(typeof(KeyAttribute), false)
                                                || p.Name.Equals("Creator", StringComparison.InvariantCultureIgnoreCase)
                                               || p.Name.Equals("CreateTime", StringComparison.InvariantCultureIgnoreCase));
                        break;
                }

                _PropertyCache.Add(Key + Enum.GetName(typeof(OperatorStatus), status), properties);
            }

            return _PropertyCache[Key + Enum.GetName(typeof(OperatorStatus), status)];
        }

        #endregion 属性缓存

        #region 实体属性值验证

        /// <summary>
        /// 实体数据验证
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="Entity">实体</param>
        internal static void ValidateEntity<T>(this T Entity) where T : BaseEntity, new()
        {
            if (_PropertyAttributeCache.Count == 0)
            {
                return;
            }
            string TypeName = typeof(T).Name;

            List<PropertyInfo> properties = GetPropertyCache<T>(OperatorStatus.Select);

            List<ErrorInfo> Errs = new List<ErrorInfo>();

            foreach (PropertyInfo prop in properties)
            {
                if (_PropertyAttributeCache[TypeName].ContainsKey(prop.Name))
                {
                    List<CustomBaseAttribute> attributes = _PropertyAttributeCache[TypeName][prop.Name];

                    foreach (CustomBaseAttribute attr in attributes)
                    {
                        bool bfield = false;
                        if ((attr is FieldAttribute) && !bfield)
                        {
                            bfield = true;

                            FieldAttribute FieldAttr = attr as FieldAttribute;

                            string Value = prop.FastGetValue(Entity) + "";
                            if (string.IsNullOrEmpty(Value) && !FieldAttr.Empty)
                            {
                                Errs.Add(new ErrorInfo(string.IsNullOrEmpty(_CustomNameCache[prop.Name])
                                    ? prop.Name
                                    : _CustomNameCache[prop.Name], "非空约束"));
                            }

                            if (FieldAttr.Length > 0)
                            {
                                if (Value.Length > FieldAttr.Length)
                                {
                                    Errs.Add(new ErrorInfo(prop.GetCustomName(), string.IsNullOrEmpty(FieldAttr.ErrorMessage)
                                        ? "长度超出限定范围,范围:" + FieldAttr.Length
                                        : FieldAttr.ErrorMessage));
                                }
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

        #endregion 实体属性值验证

        #region DataRow数据验证
        /// <summary>
        /// DataRow 数据验证
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="dr">DataRow</param>
        internal static void ValidateDataRow<T>(this DataRow dr) where T : BaseEntity, new()
        {
            //20200508 ligy 验证datarow时,不处理被删除的行
            if (dr.RowState== DataRowState.Deleted)
            {
                return;
            }
            string TypeName = typeof(T).Name;

            List<PropertyInfo> properties = GetPropertyCache<T>(OperatorStatus.Select);

            List<ErrorInfo> Errs = new List<ErrorInfo>();

            foreach (PropertyInfo prop in properties)
            {
                if (_PropertyAttributeCache[TypeName].ContainsKey(prop.Name))
                {
                    List<CustomBaseAttribute> attributes = _PropertyAttributeCache[TypeName][prop.Name];

                    foreach (CustomBaseAttribute attr in attributes)
                    {
                        bool bfield = false;
                        if ((attr is FieldAttribute) && !bfield)
                        {
                            bfield = true;

                            FieldAttribute FieldAttr = attr as FieldAttribute;

                            string Value = dr[prop.GetCustomName()] + "";
                            if (string.IsNullOrEmpty(Value) && !FieldAttr.Empty)
                            {
                                Errs.Add(new ErrorInfo(string.IsNullOrEmpty(_CustomNameCache[prop.Name])
                                    ? prop.Name
                                    : _CustomNameCache[prop.Name], "非空约束"));
                            }

                            if (FieldAttr.Length > 0)
                            {
                                if (Value.Length > FieldAttr.Length)
                                {
                                    Errs.Add(new ErrorInfo(prop.GetCustomName(), string.IsNullOrEmpty(FieldAttr.ErrorMessage)
                                        ? "长度超出限定范围,范围:" + FieldAttr.Length
                                        : FieldAttr.ErrorMessage));
                                }
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
        #endregion

        #region 自定义的数据库异常

        [Serializable]
        public class SqlFieldsException : Exception
        {
            public SqlFieldsException(List<ErrorInfo> errorInfos) : base("数据验证失败,存在非法数据!")
            {
                ErrorInfos = errorInfos;
            }

            public List<ErrorInfo> ErrorInfos { get; private set; }

        }

        /// <summary>
        /// 错误信息
        /// </summary>
        public sealed class ErrorInfo
        {
            internal ErrorInfo(string field, string errmsg)
            {
                ErrorField = field;
                ErrorMessage = errmsg;
            }
            public string ErrorField { get; set; }
            public string ErrorMessage { get; set; }
        }


        #endregion

        #region WhereHelper 数据库操作方法

        /// <summary>
        /// And
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <returns></returns>
        public static WhereHelper<T> And<T>(this WhereHelper<T> wherehelper) where T : BaseEntity, new()
            => wherehelper.StringAppend(" And ");

        /// <summary>
        /// Field = Value
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static WhereHelper<T> FieldEqual<T>(this WhereHelper<T> wherehelper, string field, object value) where T : BaseEntity, new()
            => wherehelper.StringAppend(" ", field, "=", WhereHelper<T>.TurnParaToWherePrefix(field)).ParaAppend(field, value);

        /// <summary>
        /// Field
        /// </summary>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <returns></returns>
        public static WhereHelper<T> Field<T>(this WhereHelper<T> wherehelper, string field) where T : BaseEntity, new()
        {
            wherehelper.StringAppend(" ", field).PreField = field;

            return wherehelper;
        }

        /// <summary>
        /// >Value
        /// </summary>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static WhereHelper<T> GreaterThan<T>(this WhereHelper<T> wherehelper, object value) where T : BaseEntity, new()
        => wherehelper.StringAppend(" > ", WhereHelper<T>.TurnParaToWherePrefix(wherehelper.PreField)).ParaAppend(string.Empty, value);


        /// <summary>
        /// Sql中的比较符 < value
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static WhereHelper<T> LessThan<T>(this WhereHelper<T> wherehelper, object value) where T : BaseEntity, new()
        => wherehelper.StringAppend(" < ", WhereHelper<T>.TurnParaToWherePrefix(wherehelper.PreField)).ParaAppend(string.Empty, value);


        /// <summary>
        /// 执行获取DataTable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetDataTable(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,返回受影响行
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns></returns>
        public static int RunSql<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
         => DBHelper.RunSql(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,返回首行首列
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns></returns>
        public static object GetScalar<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetScalar(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,返回单行数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns></returns>
        public static DbDataReader GetDataReader<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetDataReader(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());


        #endregion

        /// <summary>
        /// 执行Sql语句,返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns>实体</returns>
        public static T GetEntity<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        {
            using (var reader = whereHelper.GetDataReader())
            {
                try
                {
                    if (reader.HasRows)
                    {
                        if (reader.Read())
                        {
                            return reader.GetEntity<T>();
                        }
                    }
                }
                catch (Exception)
                {
                    reader.Close();
                    throw;
                }
            }
            return default(T);
        }

        /// <summary>
        /// 根据DataReader获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static T GetEntity<T>(this DbDataReader reader) where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Select);

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
                    prop.FastSetValue(entity, reader[prop.GetCustomName()] is DBNull ? DBNull.Value : reader[prop.GetCustomName()]);
                }
            }
            return entity;
        }

        /// <summary>
        /// 根据DataRow获取实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        /// <returns></returns>
        [Obsolete("20200506 方法不适用", false)]
        public static T GetEntity<T>(this DataRow dr) where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Select);

            T entity = new T();

            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType.Name.IndexOf("Boolean") == 0)
                {
                    prop.FastSetValue(entity, Convert.ToBoolean(dr[prop.GetCustomName()]));
                }
                else if (prop.PropertyType.Name.IndexOf("DateTime") == 0)
                {
                    prop.FastSetValue(entity, default(DateTime));
                }
                else if (prop.Name.Equals("Modifier", StringComparison.InvariantCultureIgnoreCase) && dr[prop.GetCustomName()] is DBNull)
                {
                    prop.FastSetValue(entity, string.Empty);
                }
                else
                {
                    //获取datarow值,如果datarow为dbnull或者null,直接取属性的默认值  modify by ligy 20200430
                    prop.FastSetValue(entity, (dr[prop.GetCustomName()] is DBNull) || (dr[prop.GetCustomName()] == null) ? prop.FastGetValue(entity) : dr[prop.GetCustomName()]);
                }
            }
            return entity;
        }

        /// <summary>
        /// 根据sql返回类型实体list,无参
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <returns></returns>
        public static List<T> GetEntities<T>(string sql) where T : BaseEntity, new()
        {
            using (var reader = DBHelper.GetDataReader(sql, CommandBehavior.Default, null))
            {
                try
                {
                    List<T> listEntity = new List<T>();

                    while (reader.Read())
                    {
                        listEntity.Add(reader.GetEntity<T>());
                    }
                    return listEntity;
                }
                catch (Exception)
                {
                    reader.Close();
                    throw;
                }
            }
        }



    }

    #region 操作状态

    /// <summary>
    /// 操作状态
    /// </summary>
    internal enum OperatorStatus
    {
        Insert = 0,
        Delete = 1,
        Update = 2,
        Select = 3
    }

    #endregion

    #region WhereHelper

    /// <summary>
    /// where条件帮助类
    /// </summary>
    public class WhereHelper<T> where T : BaseEntity, new()
    {
        internal WhereHelper() { }

        /// <summary>
        /// 当前sql
        /// </summary>
        private StringBuilder SqlString { get; set; } = new StringBuilder(1000);
        /// <summary>
        /// 上一次的字段
        /// </summary>
        internal string PreField { get; set; }

        /// <summary>
        /// 参数列表
        /// </summary>
        private List<DbParameter> DbParas { get; set; } = new List<DbParameter>();

        /// <summary>
        /// 添加参数
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">value</param>
        /// <param name="turn">是否需要转换成前缀where</param>
        /// <returns></returns>
        internal WhereHelper<T> ParaAppend(string key, object value, bool turn = true)
        {
            string scurrentKey = string.IsNullOrEmpty(key)
                ? PreField
                : key;

            var para = DBHelper.AddDbParameter(turn
                ? TurnParaToWherePrefix(scurrentKey)
                : scurrentKey, TypeConvert.ToStr(value) == string.Empty ? " " : value);

            DbParas.Add(para);

            return this;
        }

        /// <summary>
        /// 转换wherehelper中的参数,防止参数重复导致赋值错误
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string TurnParaToWherePrefix(string key)
        {
            return "@where" + key;
        }

        /// <summary>
        /// 追加字符串
        /// </summary>
        /// <param name="objValue"></param>
        internal WhereHelper<T> StringAppend(params object[] objValue)
        {
            if (objValue == null || objValue.Count() == 0) return this;

            foreach (var val in objValue)
            {
                SqlString.Append(val);
            }

            return this;
        }


        /// <summary>
        /// 返回当前的SQL字符串
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return SqlString.ToString();
        }

        /// <summary>
        /// 参数数组
        /// </summary>
        /// <returns>DbParameter[]</returns>
        internal DbParameter[] ToDBArry()
        {
            return DbParas.ToArray();
        }

    }


    #endregion


}