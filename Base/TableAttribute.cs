/******************************************
 *Function        CreatTime         Creator
 *表特性            20200413          Ligy
 *
 *****************************************/

using System;

namespace HRRobot.Base.ORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableAttribute : CustomBaseAttribute
    {
        public TableAttribute(string fieldName) : base(fieldName)
        {
        }
    }
}