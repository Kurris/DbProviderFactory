using System.Windows.Forms;


/* 功能: TextBox事件统一处理
 * 
 * 
 * 修改时间                               修改人                                 修改内容
 * 20200620                              ligy                                  create 
 * 20200623                              ligy                                  增加对负数处理

 ***************************************************************************************************************/

namespace HRRobot.Base.ControlExtention
{
    public static class TextBoxEvents
    {
        /// <summary>
        /// 注册KeyPress事件
        /// </summary>
        /// <param name="textBox">TextBox</param>
        public static void InitEventKeyPress(this TextBox textBox)
        {
            textBox.KeyPress += (s, e) =>
            {
                bool bLegal = true;

                ////Ctrl+C ; Ctrl+X
                //if (e.KeyChar == '\u0003' || e.KeyChar == '\u0018')
                //{
                //    //不需要处理
                //}
                ////Ctrl+V
                //else if (e.KeyChar == '\u0016')
                //{
                //    string sClip = Clipboard.GetText();

                //    bLegal = double.TryParse(sClip, out _);
                //}


                // "."
                if (e.KeyChar == 46)
                {
                    string sValue = textBox.Text;

                    if (textBox.SelectionStart == 0)
                        bLegal = false;
                    else if (string.IsNullOrEmpty(sValue))
                        bLegal = false;
                    else if (sValue.Contains("."))
                        bLegal = false;
                    else if (sValue.EndsWith("."))
                        bLegal = false;
                }
                //"-"
                else if (e.KeyChar == 45)
                {
                    string sValue = textBox.Text;
                    if (sValue.Contains("-"))
                    {
                        bLegal = false;
                    }
                    else
                    {
                        textBox.SelectionStart = 0;
                    }
                }
                else if (!char.IsNumber(e.KeyChar)
                        && e.KeyChar != (char)Keys.Back
                        && e.KeyChar != (char)Keys.Tab
                        && e.KeyChar != (char)Keys.Enter)
                {
                    bLegal = false;
                }

                if (!bLegal)
                {
                    Msg.ShowInfo("请输入数字!");
                    e.Handled = true;
                }
            };
        }
    }
}
