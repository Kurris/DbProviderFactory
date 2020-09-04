using System;

namespace DbProviderFactory.ORM
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class KeyAttribute : CustomBaseAttribute
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public KeyAttribute() : base(string.Empty)
        {
        }
    }
}