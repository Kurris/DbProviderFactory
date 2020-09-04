using System.Drawing;
using System.Windows.Forms;

/* 功能: DataGridView样式
 * 
 * 
 * 修改时间                               修改人                                 修改内容
 * 20200620                              ligy                                  create 
 * 20200813                              ligy                                  VisibleCommonColumns增加对fGuid的隐藏

 ***************************************************************************************************************/

namespace HRRobot.Base.ControlExtention
{
    public static class DataGridViewStyle
    {
        /// <summary>
        /// 初始化DataGridView样式
        /// </summary>
        /// <param name="dgv">数据容器</param>
        /// <param name="ReadOnly">只读</param>
        public static void InitStyle(this DataGridView dgv, bool ReadOnly = true)
        {
            for (int i = 0; i < dgv.Rows.Count; i++)
            {
                dgv.Rows[i].ReadOnly = ReadOnly;
            }

            dgv.BackgroundColor = Color.White;
            dgv.RowHeadersVisible = false;
            dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgv.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            for (int i = 0; i < dgv.ColumnCount; i++)
            {
                dgv.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                dgv.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dgv.AllowUserToAddRows = false;
            dgv.MultiSelect = false;

            dgv.CellBorderStyle = DataGridViewCellBorderStyle.None;
            dgv.AdvancedColumnHeadersBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;
            dgv.AdvancedColumnHeadersBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;
            dgv.AdvancedColumnHeadersBorderStyle.Left = DataGridViewAdvancedCellBorderStyle.None;
            dgv.AdvancedColumnHeadersBorderStyle.Right = DataGridViewAdvancedCellBorderStyle.None;
        }

        /// <summary>
        /// 隐藏通用的列
        /// <para>Id,fCreator,fCreateTime,fModifier,fModifyTime</para>
        /// <paramref name="dgv"/>
        /// </summary>
        /// <param name="dgv">数据容器</param>
        /// <param name="Visible">是否显示</param>
        public static void VisibleCommonColumns(this DataGridView dgv, bool Visible)
        {
            string[] arrCol = new string[] { "Id", "fCreator", "fCreateTime", "fModifier", "fModifyTime", "fGuid" };

            foreach (var colName in arrCol)
            {
                var Col = dgv.Columns[colName];
                if (Col != null)
                {
                    Col.Visible = Visible;
                }
            }
        }
    }
}
