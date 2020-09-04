using System;

namespace DbProviderFactory.ORM
{
    /// <summary>
    /// 实体基类
    /// </summary>
    [Serializable]
    public class BaseEntity
    {
        /// <summary>
        /// 创建人
        /// </summary>
        [Field("fCreator")]
        public string Creator { get; set; } = Environment.MachineName;

        /// <summary>
        /// 创建时间
        /// </summary>
        [Field("fCreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 修改人
        /// </summary>
        [Field("fModifier", Empty = true)]
        public string Modifier { get; set; } = Environment.MachineName;

        /// <summary>
        /// 修改事件
        /// </summary>
        [Field("fModifyTime", Empty = true)]
        public DateTime ModifyTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 全局唯一UID
        /// </summary>
        [Key]
        [Field("fGuid")]
        public string Uid { get; set; }
    }
}
