using DbProviderFactory.Ado;
using DbProviderFactory.ClassExtention;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;


namespace DbProviderFactory.ORM
{

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
                : scurrentKey, value.ToString() == string.Empty ? " " : value);

            DbParas.Add(para);

            return this;
        }

        /// <summary>
        /// 转换wherehelper中的参数,防止参数重复导致赋值错误
        /// </summary>
        /// <param name="key">字段名</param>
        /// <returns>@where+字段名</returns>
        public static string TurnParaToWherePrefix(string key) => "@where" + key;

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

    public static class WhereHelperEx
    {

        /// <summary>
        /// 并且
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> And<T>(this WhereHelper<T> wherehelper) where T : BaseEntity, new()
            => wherehelper.StringAppend(" And ");

        /// <summary>
        ///  字段=值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> FieldEqual<T>(this WhereHelper<T> wherehelper, string field, object value) where T : BaseEntity, new()
            => wherehelper.StringAppend(" ", field, "=", WhereHelper<T>.TurnParaToWherePrefix(field)).ParaAppend(field, value);

        /// <summary>
        /// 字段 like %值%
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> FieldContains<T>(this WhereHelper<T> wherehelper, string field, object value) where T : BaseEntity, new()
            => wherehelper.StringAppend(" ", field, " Like ", WhereHelper<T>.TurnParaToWherePrefix(field)).ParaAppend(field, $"%{value}%");

        /// <summary>
        /// 字段 like %值
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> FieldEndWith<T>(this WhereHelper<T> wherehelper, string field, object value) where T : BaseEntity, new()
            => wherehelper.StringAppend(" ", field, " Like ", WhereHelper<T>.TurnParaToWherePrefix(field)).ParaAppend(field, $"%{value}");

        /// <summary>
        /// 字段 like 值%
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="field">字段</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> FieldStartWith<T>(this WhereHelper<T> wherehelper, string field, object value) where T : BaseEntity, new()
            => wherehelper.StringAppend(" ", field, " Like ", WhereHelper<T>.TurnParaToWherePrefix(field)).ParaAppend(field, $"{value}%");

        /// <summary>
        /// 比较符
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> GreaterThan<T>(this WhereHelper<T> wherehelper, object value) where T : BaseEntity, new()
        => wherehelper.StringAppend(" > ", WhereHelper<T>.TurnParaToWherePrefix(wherehelper.PreField)).ParaAppend(string.Empty, value);


        /// <summary>
        /// 比较符
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="wherehelper">wherehelper</param>
        /// <param name="value">值</param>
        /// <returns><see cref="WhereHelper{T}"/></returns>
        public static WhereHelper<T> LessThan<T>(this WhereHelper<T> wherehelper, object value) where T : BaseEntity, new()
        => wherehelper.StringAppend(" < ", WhereHelper<T>.TurnParaToWherePrefix(wherehelper.PreField)).ParaAppend(string.Empty, value);


        /// <summary>
        /// 执行获取DataTable
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns><see cref="DataTable"/></returns>
        public static DataTable GetDataTable<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetDataTable(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns><see cref="int"/> 受影响行数</returns>
        public static int RunSql<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
         => DBHelper.RunSql(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,返回首行首列
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns><see cref="object"/></returns>
        public static object GetScalar<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetScalar(whereHelper.ToString(), CommandType.Text, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,返回单行数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns><see cref="DbDataReader"/></returns>
        public static DbDataReader GetDataReader<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        => DBHelper.GetDataReader(whereHelper.ToString(), CommandBehavior.Default, whereHelper.ToDBArry());

        /// <summary>
        /// 执行Sql语句,判断数据是否存在
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns>存在结果</returns>
        public static bool Exists<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        {
            return whereHelper.GetScalar() != null;
        }

        /// <summary>
        /// 执行Sql语句,返回实体数据
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="whereHelper">wherehelper</param>
        /// <returns><see cref="T"/></returns>
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
            return default;
        }

        /// <summary>
        /// 获取list实体
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="whereHelper">where帮助</param>
        /// <returns><see cref="List{T}"/></returns>
        public static List<T> GetEntities<T>(this WhereHelper<T> whereHelper) where T : BaseEntity, new()
        {
            using (var reader = whereHelper.GetDataReader())
            {
                if (reader.HasRows)
                {
                    List<T> listT = new List<T>();

                    while (reader.Read())
                    {
                        listT.Add(reader.GetEntity<T>());
                    }

                    return listT;
                }
                return default;
            }
        }
    }


    /// <summary>
    /// 数据库字段验证错误
    /// </summary>
    [Serializable]
    public class SqlFieldsException : Exception
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="errorInfos">错误字段信息</param>
        public SqlFieldsException(List<ErrorInfo> errorInfos) : base("数据验证失败,存在非法数据!")
        {
            ErrorInfos = errorInfos;
        }

        /// <summary>
        /// 错误字段信息
        /// </summary>
        public List<ErrorInfo> ErrorInfos { get; }

    }

    /// <summary>
    /// 错误信息
    /// </summary>
    public sealed class ErrorInfo
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="field">字段名</param>
        /// <param name="errmsg">错误提醒</param>
        internal ErrorInfo(string field, string errmsg)
        {
            ErrorField = field;
            ErrorMessage = errmsg;
        }
        /// <summary>
        /// 错误字段
        /// </summary>
        public string ErrorField { get; }

        /// <summary>
        /// 错误提醒
        /// </summary>
        public string ErrorMessage { get; }
    }
}