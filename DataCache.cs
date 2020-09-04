using System.Collections.Generic;
using System.Reflection;


namespace DbProviderFactory.ORM
{
    internal class DataCache
    {
        /// <summary>
        /// 关于不同操作产生的不同的属性缓存
        /// </summary>
        internal static Dictionary<string, List<PropertyInfo>> PropertyCache { get; set; } = new Dictionary<string, List<PropertyInfo>>();

        /// <summary>
        /// 特性缓存
        /// </summary>
        internal static Dictionary<string, Dictionary<string, List<CustomBaseAttribute>>> AttributeCache { get; set; } = new Dictionary<string, Dictionary<string, List<CustomBaseAttribute>>>();

        /// <summary>
        /// 自定义名称缓存
        /// </summary>
        internal static Dictionary<string, string> CustomNameCache { get; set; } = new Dictionary<string, string>();
    }
}
