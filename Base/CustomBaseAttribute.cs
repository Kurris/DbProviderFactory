using System;


namespace DbProviderFactory.ORM
{
    /// <summary>
    /// 自定义特性基类
    /// </summary>
    public class CustomBaseAttribute : Attribute
    {
        /// <summary>
        /// 数据库的字段名称
        /// </summary>
        public string FieldName { get; } = string.Empty;

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

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="fieldName">数据库字段名称</param>
        public CustomBaseAttribute(string fieldName)
        {
            FieldName = fieldName;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is CustomBaseAttribute attr) || !this.GetType().Name.Equals(attr.GetType().Name))
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}