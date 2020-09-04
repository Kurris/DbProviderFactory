using DbProviderFactory.ClassExtention;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace DbProviderFactory.ORM
{
    internal sealed class GenerateSql<T> where T : BaseEntity, new()
    {
        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <returns>查询sql语句</returns>
        internal string SelectSql()
        {
            List<PropertyInfo> props = MemberInfoEx.GetPropertyCache<T>();

            string sql = $"SELECT {string.Join(",", props.Select(x => $"{ x.GetCustomName() + "\r\n"}"))} \r\n" +
                         $"FROM {typeof(T).GetCustomName()}" + "\r\n";

            return sql;
        }

        /// <summary>
        ///生成删除SQL
        /// </summary>
        /// <returns>删除SQL</returns>
        internal string DeleteSql() => $"DELETE FROM {typeof(T).GetCustomName()} \r\n";


        /// <summary>
        /// 生成插入SQL
        /// </summary>
        /// <returns>插入sql语句</returns>
        public string InsertSql()
        {
            var props = MemberInfoEx.GetPropertyCache<T>()
                                  .Where(x => !x.Name.Equals("Modifier", StringComparison.OrdinalIgnoreCase)
                                            && !x.Name.Equals("ModifyTime", StringComparison.OrdinalIgnoreCase));

            string sql = $"INSERT INTO {typeof(T).GetCustomName()}\r\n" +
                         $"(\r\n" +
                         $"{string.Join(",", props.Select(x => x.GetCustomName() + "\r\n"))}" +
                         $")\r\n" +
                         $"values({string.Join(",", props.Select(x => $"@{x.Name}"))})" + "\r\n";

            return sql;
        }

        /// <summary>
        /// /生成更新SQL
        /// </summary>
        /// <returns>更新SQL</returns>
        public string UpdateSql()
        {
            var props = MemberInfoEx.GetPropertyCache<T>()
                                  .Where(x => !x.Name.Equals("Creator", StringComparison.OrdinalIgnoreCase)
                                           && !x.Name.Equals("CreateTime", StringComparison.OrdinalIgnoreCase)
                                           && !x.Name.Equals("Uid", StringComparison.OrdinalIgnoreCase));

            string sql = $"UPDATE {typeof(T).GetCustomName()} \r\n" +
                         $"SET {string.Join(",", props.Select(x => x.GetCustomName() + "= @" + x.Name + "\r\n"))}" + "\r\n";

            return sql;
        }
    }
}