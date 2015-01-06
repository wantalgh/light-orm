using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    internal static class ReflectHelper
    {
        /// <summary>
        /// 缓存已反射过的类型的结果
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertyDic = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>
        /// 获取某个类型的属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>属性</returns>
        public static PropertyInfo[] GetProperties(Type type)
        {
            var properties = TypePropertyDic.GetOrAdd(type, t => t.GetProperties());
            return properties;
        }
    }
}
