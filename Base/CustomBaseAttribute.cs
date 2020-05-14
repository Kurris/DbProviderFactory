/******************************************
 *Function        CreatTime         Creator
 *自定义特性基类     20200413          Ligy
 *
 *****************************************/

using System;

namespace HRRobot.Base.ORM
{
    /// <summary>
    /// 特性基类
    /// </summary>
    public class CustomBaseAttribute : Attribute
    {
        private string _fieldName = string.Empty;

        /// <summary>
        /// 数据库的字段名称
        /// </summary>
        public string FieldName { get => _fieldName; }

        /// <summary>
        /// 错误提醒
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// 允许长度
        /// </summary>
        public int Length { get; set; } = 0;

        /// <summary>
        /// 默认设置不能空
        /// </summary>
        public bool Empty { get; set; } = false;

        public CustomBaseAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }
    }
}