using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

/*修改时间                               修改人                                 修改内容
 *20200413                              ligy                                  create 类型的增删改查语句生成
 *
 ***************************************************************************************************************/

namespace HRRobot.Base.ORM
{
    internal sealed class GeneralSql
    {
        /// <summary>
        /// 生成查询SQL
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>查询sql语句</returns>
        public string QuerySql<T>() where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Select);

            string sql = $"SELECT {string.Join(",", properties.Select(x => $"{ x.GetCustomName() + "\r\n"}"))} \r\n" +
                         $" FROM {typeof(T).GetCustomName()} ";

            return sql;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public string DeleteSql<T>() where T : BaseEntity, new()=> $"DELETE FROM {typeof(T).GetCustomName()} \r\n";


        /// <summary>
        /// 生成插入SQL
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>插入sql语句</returns>
        public string InsertSql<T>() where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Insert);

            string sql = $"INSERT INTO {typeof(T).GetCustomName()}\r\n(\r\n{string.Join(",", properties.Select(x => x.GetCustomName() + "\r\n"))})  \r\n" +
                         $"values({string.Join(",", properties.Select(x => $"@{x.Name}"))})";

            return sql;
        }

        /// <summary>
        /// /生成更新SQL
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <returns>更新SQL</returns>
        public string UpdateSql<T>() where T : BaseEntity, new()
        {
            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Update);

            string sql = $"UPDATE {typeof(T).GetCustomName()} \r\n" +
                         $"SET {string.Join(",", properties.Select(x => x.GetCustomName() + "=@" + x.Name+"\r\n"))}";

            return sql;
        }
    }
}