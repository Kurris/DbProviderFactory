using HRRobot.Base.Ado;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

/*修改时间                               修改人                                 修改内容
 *20200413                              ligy                                  create 自定义ORM
 *20200429                              ligy                                  增加获取list实体,和datatable的方法
 *20200429                              wjm                                   增加根据datatable更新数据库和datarow转实体
 *20200429                              ligy                                  删除RowtoT方法,改为使用原有的方法逻辑
 *20200506                              ligy                                  修改Save方法,将以往DataRow转entity的方法改为直接取DataRow的数据
 ***************************************************************************************************************/

namespace HRRobot.Base.ORM
{
    /// <summary>
    /// 使用泛型类对不同类型产生对应的类型副本
    /// </summary>
    /// <typeparam name="T">实体类型</typeparam>
    public sealed class ORM<T> where T : BaseEntity, new()
    {
        /// <summary>
        /// 私有,外部不可初始化
        /// </summary>
        private ORM()
        {
        }

        /// <summary>
        /// 静态构造函数,只初始化一次
        /// </summary>
        static ORM()
        {
            GeneralSql gen = new GeneralSql();

            _querySql = gen.QuerySql<T>();
            _insertSql = gen.InsertSql<T>();
            _deleteSql = gen.DeleteSql<T>();
            _updateSql = gen.UpdateSql<T>();
        }

        private static readonly string _querySql = string.Empty;
        private static readonly string _insertSql = string.Empty;
        private static readonly string _deleteSql = string.Empty;
        private static readonly string _updateSql = string.Empty;

        /// <summary>
        /// 根据Id查询
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns>实体</returns>
        public static T FindById(int Id) => Find().FieldEqual("id", Id).GetEntity();

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public static WhereHelper<T> Find() => new WhereHelper<T>().StringAppend(_querySql, " WHERE ");

        public static List<T> FindAllEntity()
        {
            return StaticExtendMethod.GetEntities<T>(_querySql);
        }
        public static DataTable FindAllDataTable()
        {
            return DBHelper.GetDataTable(_querySql);
        }


        /// <summary>
        /// 根据实体,插入数据,返回执行结果
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>影响数</returns>
        public static int Insert(T Entity)
        {
            Entity.ValidateEntity();

            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Insert);

            return DBHelper.RunSql(_insertSql,
                System.Data.CommandType.Text,
                properties.Select(x => DBHelper.AddDbParameter(x.Name, x.FastGetValue(Entity)))
                .ToArray());
        }

        /// <summary>
        /// 根据Id删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public static int DeleteById(int Id) => Delete().FieldEqual("id", Id).RunSql();


        /// <summary>
        /// 根据实体更新数据
        /// </summary>
        /// <param name="Entity"></param>
        /// <returns></returns>
        public static int UpdateByID(T Entity) => Update(Entity).FieldEqual("id", Entity.Id).RunSql();

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static WhereHelper<T> Update(T entity)
        {
            entity.ValidateEntity();

            WhereHelper<T> wherehelper = new WhereHelper<T>();

            List<PropertyInfo> properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Update);

            properties.ForEach(x => wherehelper.ParaAppend(x.Name, x.FastGetValue(entity), false));

            return wherehelper.StringAppend(_updateSql, " WHERE ");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public static WhereHelper<T> Delete() => new WhereHelper<T>().StringAppend(_deleteSql, " WHERE ");

        /// <summary>
        /// 根据DataTable,方法中自动获取变化数据更新数据库
        /// </summary>
        /// <param name="dt">内存数据</param>
        /// <param name="isChange">是否为变化数据</param>
        /// <returns></returns>
        public static bool Save(DataTable dt, bool isChange = false)
        {
            DataTable dtChanges = null;

            if (isChange)
                dtChanges = dt;
            else dtChanges = dt.GetChanges();

            if (dtChanges == null || dtChanges.Rows.Count == 0)
            {
                return true;
            }

            List<string> listSqls = new List<string>();

            List<PropertyInfo> properties = null;

            List<List<DbParameter>> listDbParas = new List<List<DbParameter>>();

            foreach (DataRow dr in dtChanges.Rows)
            {
                dr.ValidateDataRow<T>();

                switch (dr.RowState)
                {
                    case DataRowState.Added:

                        listSqls.Add(_insertSql);

                        properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Insert);

                        listDbParas.Add(properties.Select(x => DBHelper.AddDbParameter(x.Name, dr[x.GetCustomName()])).ToList());

                        break;
                    case DataRowState.Deleted:

                        listSqls.Add(_deleteSql + " WHERE id = " + dr["id", DataRowVersion.Original]);

                        listDbParas.Add(new List<DbParameter>());

                        break;
                    case DataRowState.Modified:

                        listSqls.Add(_updateSql + " WHERE id =" + dr["id"]);

                        properties = StaticExtendMethod.GetPropertyCache<T>(OperatorStatus.Update);

                        listDbParas.Add(properties.Select(x => DBHelper.AddDbParameter(x.Name, dr[x.GetCustomName()])).ToList());

                        break;
                    default:
                        break;
                }
            }
            return DBHelper.RunSql(listSqls, System.Data.CommandType.Text, listDbParas);

        }
    }
}