/******************************************
 *Function        CreatTime         Creator
 *字段约束特性       20200413          Ligy
 *
 *****************************************/

using System;

namespace HRRobot.Base.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class FieldAttribute : CustomBaseAttribute
    {
        public FieldAttribute(string fieldName) : base(fieldName)
        {
        }
    }
}