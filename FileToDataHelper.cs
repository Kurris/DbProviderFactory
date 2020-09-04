using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;


namespace DbProviderFactory.FileToData
{
    public sealed class FileToDataHelper
    {
        /// <summary>
        /// 正确做法是使用",",但是程序数据里存在
        /// </summary>
        private static readonly char _msSpiltter = '$';

        /// 导出
        /// </summary>
        /// <param name="Tables"></param>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public static bool Export(IEnumerable<DataTable> Tables, string FilePath)
        {
            StringBuilder sb = new StringBuilder();

            try
            {
                using (StreamWriter sw = new StreamWriter(FilePath, false, Encoding.UTF8))
                {
                    foreach (DataTable Table in Tables)
                    {
                        GenerateDataFromData(Table.Copy(), sb);
                    }

                    sw.Write(sb.ToString());

                    return true;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        /// <summary>
        /// 产生需要的数据
        /// </summary>
        /// <param name="Table">内存数据</param>
        /// <param name="SB">StringBuilder</param>
        private static void GenerateDataFromData(DataTable Table, StringBuilder SB)
        {
            if (Table == null || Table.Rows.Count == 0) return;

            Table.Columns.Remove("fCreator");
            Table.Columns.Remove("fCreateTime");
            Table.Columns.Remove("fModifier");
            Table.Columns.Remove("fModifyTime");

            //表名
            SB.AppendLine(Table.TableName);

            //列名
            SB.AppendLine(GetColumnNames(Table));

            //每一行的数据
            foreach (DataRow dr in Table.Rows)
            {
                for (int i = 0; i < dr.ItemArray.Count(); i++)
                {
                    if (i != dr.ItemArray.Count() - 1)
                        SB.Append(dr.ItemArray[i].ToString() + _msSpiltter);
                    else
                        SB.AppendLine(dr.ItemArray[i].ToString());
                }
            }
            SB.AppendLine();
        }

        /// <summary>
        /// 导入为内存表
        /// </summary>
        public static List<DataTable> Import(string FilePath)
        {
            using (StreamReader reader = new StreamReader(FilePath, Encoding.UTF8))
            {
                string sRead = string.Empty;

                List<DataTable> RtnTlbs = new List<DataTable>();

                DataTable dt = null;
                while ((sRead = reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(sRead))
                    {
                        RtnTlbs.Add(dt);
                        continue;
                    }

                    if (!sRead.Contains(_msSpiltter) && !string.IsNullOrEmpty(sRead))
                    {
                        dt = new DataTable
                        {
                            TableName = sRead
                        };

                        //列名
                        sRead = reader.ReadLine();
                        string[] arrColumns = sRead.Split(_msSpiltter);
                        foreach (string colName in arrColumns)
                        {
                            dt.Columns.Add(colName);
                        }

                        //主键
                        dt.PrimaryKey = new[] { dt.Columns["fGuid"] };

                        continue;
                    }

                    string[] arrSplit = sRead.Split(_msSpiltter);

                    DataRow dr = dt.NewRow();

                    for (int i = 0; i < arrSplit.Length; i++)
                    {
                        dr[i] = arrSplit[i];
                    }

                    dt.Rows.Add(dr);
                }
                return RtnTlbs;
            }
        }


        /// <summary>
        /// 取列名
        /// </summary>
        /// <param name="Table"></param>
        /// <returns>列名,以逗号隔开</returns>
        private static string GetColumnNames(DataTable Table)
        {
            if (Table == null || Table.Columns.Count == 0) throw new NullReferenceException("当前数据为空");

            List<string> listStr = new List<string>(Table.Columns.Count);

            foreach (DataColumn col in Table.Columns)
            {
                listStr.Add(col.ColumnName);
            }

            return string.Join(_msSpiltter.ToString(), listStr);
        }

    }
}
