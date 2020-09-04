using System;
using System.IO;
using System.Linq;

namespace DbProviderFactory.ClassExtention
{
    /// <summary>
    /// 字符串扩展方法
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// 检查文件名是否保存非法字符
        /// </summary>
        /// <param name="FileName">文件名</param>
        public static void ValidFileName(this string FileName)
        {
            var Invalids = Path.GetInvalidFileNameChars();

            foreach (var item in Invalids)
            {
                if (FileName.Contains(item))
                {
                    throw new ArgumentException($"名称存在非法字符[{ item }]");
                }
            }
        }
    }
}
