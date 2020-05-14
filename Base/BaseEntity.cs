using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRRobot.Base.ORM
{
    /// <summary>
    /// 实体基类
    /// </summary>
    public class BaseEntity
    {
        [Key]
        public int Id { get; set; }

        [Field("fCreator")]
        public string Creator { get; set; } = Environment.MachineName;

        [Field("fCreateTime")]
        public DateTime CreateTime { get; set; } = DateTime.Now;


        [Field("fModifier", Empty = true)]
        public string Modifier { get; set; } = Environment.MachineName;

        [Field("fModifyTime", Empty = true)]
        public DateTime ModifyTime { get; set; } = DateTime.Now;
    }
}
