/******************************************
 *Function        CreatTime         Creator
 *自定义特性基类     20200413          Ligy
 *
 *****************************************/

using System;

namespace HRRobot.Base.ORM
{
    public class CustomBaseAttribute : Attribute
    {
        private string _fieldName = string.Empty;

        public string FieldName { get => _fieldName; }
        public string ErrorMessage { get; set; }
        public int Length { get; set; } = 0;
        public bool Empty { get; set; } = true;

        public CustomBaseAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }
    }
}