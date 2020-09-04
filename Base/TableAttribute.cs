using System;

namespace DbProviderFactory.ORM
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TableAttribute : CustomBaseAttribute
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fieldName"></param>
        public TableAttribute(string TableName) : base(TableName)
        {
        }
    }
}