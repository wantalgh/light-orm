// SPDX-License-Identifier: MIT

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
        /// Type's properties cache.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> TypePropertyDic = new ConcurrentDictionary<Type, PropertyInfo[]>();


        /// <summary>
        /// Get properties of type.
        /// </summary>
        public static PropertyInfo[] GetProperties(Type type)
        {
            var properties = TypePropertyDic.GetOrAdd(type, t => t.GetProperties());
            return properties;
        }
    }
}
