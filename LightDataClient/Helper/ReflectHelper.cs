using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Wantalgh.LightDataClient
{
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
