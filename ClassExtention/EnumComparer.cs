using System;
using System.Collections.Generic;
using System.Linq.Expressions;


namespace DbProviderFactory.ClassExtention
{
    /// <summary>
    /// 自定义枚举比较器
    /// </summary>
    /// <typeparam name="T">枚举类型</typeparam>
    public class EnumComparer<T> : IEqualityComparer<T> where T : Enum
    {
        public bool Equals(T first, T second)
        {
            var firstPara = Expression.Parameter(typeof(T), "first");
            var secondPara = Expression.Parameter(typeof(T), "second");
            var equalExpression = Expression.Equal(firstPara, secondPara);

            return Expression.Lambda<Func<T, T, bool>>
              (equalExpression, new[] { firstPara, secondPara }).Compile().Invoke(first, second);
        }

        public int GetHashCode(T obj)
        {
            var para = Expression.Parameter(typeof(T), "instance");
            var convertExpression = Expression.Convert(para, typeof(int));

            return Expression.Lambda<Func<T, int>>(convertExpression, new[] { para }).Compile().Invoke(obj);
        }
    }
}
