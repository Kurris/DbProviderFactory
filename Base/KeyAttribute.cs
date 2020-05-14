/******************************************
 *Function        CreatTime         Creator
 *主键特性          20200413          Ligy
 *
 *****************************************/

using System;

namespace HRRobot.Base.ORM
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : CustomBaseAttribute
    {
        public KeyAttribute() : base(string.Empty)
        {
        }
    }
}