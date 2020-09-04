using System;

namespace DbProviderFactory.ORM
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FieldAttribute : CustomBaseAttribute
    {
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fieldName"></param>
        public FieldAttribute(string fieldName) : base(fieldName)
        {
        }
    }
}