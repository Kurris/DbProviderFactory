using DbProviderFactory.Ado;
using DbProviderFactory.ClassExtention;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;

namespace DbProviderFactory.ORM
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
        /// 静态构造函数
        /// </summary>
        static ORM()
        {
            GenerateSql<T> Generate = new GenerateSql<T>();

            _SelectSql = Generate.SelectSql();
            _InsertSql = Generate.InsertSql();
            _DeleteSql = Generate.DeleteSql();
            _UpdateSql = Generate.UpdateSql();
        }

        private static readonly string _SelectSql = string.Empty;
        private static readonly string _InsertSql = string.Empty;
        private static readonly string _DeleteSql = string.Empty;
        private static readonly string _UpdateSql = string.Empty;


        /// <summary>
        /// 根据实体,插入数据,返回执行结果
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>影响数</returns>
        public static int Insert(T Entity)
        {
            Entity.Validate();

            var props = MemberInfoEx.GetPropertyCache<T>();

            return DBHelper.RunSql(_InsertSql, props.Select(x => DBHelper.AddDbParameter(x.Name, x.FastGetValue(Entity))).ToArray());
        }

        /// <summary>
        /// 根据实体,删除数据,返回执行结果
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>影响数</returns>
        public static int Delete(T Entity)
        {
            return DBHelper.RunSql(_DeleteSql + "Where fGuid =@Uid", new[] { DBHelper.AddDbParameter("Uid", Entity.Uid) });
        }

        /// <summary>
        /// 根据ID,删除数据,返回执行结果
        /// </summary>
        /// <param name="Guid">Guid</param>
        /// <returns>影响数</returns>
        public static int DeleteById(string Guid)
        {
            return Delete(new T() { Uid = Guid });
        }

        /// <summary>
        /// 根据实体,更新数据,返回执行结果
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>影响数</returns>
        public static int UpdateEx(T Entity)
        {
            Entity.Validate();

            var props = MemberInfoEx.GetPropertyCache<T>();

            return DBHelper.RunSql(_UpdateSql + "Where fGuid = @Uid", props.Select(x => DBHelper.AddDbParameter(x.Name, x.FastGetValue(Entity))).ToArray());
        }

        /// <summary>
        /// 返回类型实体list
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <returns></returns>
        public static List<T> GetEntities()
        {
            using (var reader = DBHelper.GetDataReader(_SelectSql, CommandBehavior.Default, null))
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

        /// <summary>
        /// 返回类型实体
        /// </summary>
        /// <param name="Guid">Guid</param>
        /// <returns>实体</returns>
        public static T GetEntityById(string Guid)
        {
            using (var reader = DBHelper.GetDataReader(_SelectSql + "Where fGuid = @Uid", CommandBehavior.Default, new[] { DBHelper.AddDbParameter("Uid", Guid) }))
            {
                try
                {
                    T Entity = new T();

                    if (reader.Read())
                    {
                        return reader.GetEntity<T>();
                    }
                    return null;
                }
                catch (Exception)
                {
                    reader.Close();
                    throw;
                }
            }
        }

        /// <summary>
        /// 根据sql返回DataTable
        /// </summary>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable()
        {
            return DBHelper.GetDataTable(_SelectSql);
        }

        /// <summary>
        /// 检查是否存在
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>存在结果</returns>
        public static bool Exists(T Entity)
        {
            return DBHelper.GetScalar(_SelectSql + "Where fGuid=@Uid", CommandType.Text, new[] { DBHelper.AddDbParameter("Uid", Entity.Uid) }) != null;
        }

        /// <summary>
        /// 根据Guid,检查是否存在
        /// </summary>
        /// <param name="Guid">Guid</param>
        /// <returns>存在结果</returns>
        public static bool ExistsById(string Guid)
        {
            return Exists(new T() { Uid = Guid });
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <param name="Entity">实体</param>
        /// <returns>保存结果</returns>
        public static bool Save(T Entity)
        {
            if (Exists(Entity))
            {
                return UpdateEx(Entity) > 0;
            }
            return Insert(Entity) > 0;
        }

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

            List<PropertyInfo> properties;

            List<List<DbParameter>> listDbParas = new List<List<DbParameter>>();

            foreach (DataRow dr in dtChanges.Rows)
            {
                dr.Validate<T>();

                switch (dr.RowState)
                {
                    case DataRowState.Added:

                        listSqls.Add(_InsertSql);

                        properties = MemberInfoEx.GetPropertyCache<T>();

                        dr["fCreator"] = Environment.MachineName;
                        dr["fCreateTime"] = DateTime.Now;

                        listDbParas.Add(properties.Select(x => DBHelper.AddDbParameter(x.Name, dr[x.GetCustomName()])).ToList());

                        break;
                    case DataRowState.Deleted:

                        listSqls.Add(_DeleteSql + $" WHERE fGuid = '{dr["fGuid", DataRowVersion.Original]}'");

                        listDbParas.Add(new List<DbParameter>());

                        break;
                    case DataRowState.Modified:

                        listSqls.Add(_UpdateSql + $" WHERE fGuid ='{dr["fGuid"]}'");

                        properties = MemberInfoEx.GetPropertyCache<T>();

                        dr["fModifier"] = Environment.MachineName;
                        dr["fModifyTime"] = DateTime.Now;

                        listDbParas.Add(properties.Select(x => DBHelper.AddDbParameter(x.Name, dr[x.GetCustomName()])).ToList());

                        break;
                    default:
                        break;
                }
            }
            bool Result = DBHelper.RunSql(listSqls, listDbParas);
            if (Result)
            {
                dt.AcceptChanges();
            }

            return Result;

        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public static WhereHelper<T> Find() => new WhereHelper<T>().StringAppend(_SelectSql, " WHERE ");

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">实体</param>
        /// <returns></returns>
        public static WhereHelper<T> Update(T entity)
        {
            entity.Validate();

            WhereHelper<T> wherehelper = new WhereHelper<T>();

            List<PropertyInfo> properties = MemberInfoEx.GetPropertyCache<T>();

            properties.ForEach(x => wherehelper.ParaAppend(x.Name, x.FastGetValue(entity), false));

            return wherehelper.StringAppend(_UpdateSql, " WHERE ");
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <returns></returns>
        public static WhereHelper<T> Delete() => new WhereHelper<T>().StringAppend(_DeleteSql, " WHERE ");
    }
}