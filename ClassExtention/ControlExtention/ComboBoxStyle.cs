using System.Windows.Forms;

/* 功能: ComboBox样式和key/value设定
 * 
 * 
 * 修改时间                               修改人                                 修改内容
 * 20200620                              ligy                                  create 

 ***************************************************************************************************************/

namespace HRRobot.Base.ControlExtention
{
    public static class ComboBoxStyle
    {
        /// <summary>
        /// 初始化ComboBox样式和设置DisplayMember/ValueMember
        /// </summary>
        /// <param name="comboBox"></param>
        public static void InitStyle(this ComboBox comboBox)
        {
            comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            comboBox.DisplayMember = "Key";
            comboBox.ValueMember = "Value";
        }
    }
}
