using System.Windows.Forms;

/* 功能: ListBox统一处理
 * 
 * 
 * 修改时间                               修改人                                 修改内容
 * 20200624                              ligy                                  create 

 ***************************************************************************************************************/

namespace HRRobot.Base.ControlExtention
{
    public static class ListBoxStyle
    {
        public static void InitStyle(this ListBox Box)
        {
            Box.DisplayMember = "Key";
            Box.ValueMember = "Value";
        }
    }
}
