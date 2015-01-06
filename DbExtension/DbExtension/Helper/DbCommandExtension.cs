using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// 针对System.Data.IDbCommand接口的扩展
    /// </summary>
    internal static class DbCommandExtension
    {
        /// <summary>
        /// 执行SQL，把结果映射到对象上。
        /// </summary>
        /// <typeparam name="T">对象的类型，类型的属性名应与SQL返回的列名一致。</typeparam>
        /// <param name="command">被扩展调用的DbCommand对象</param>
        /// <param name="behavior">执行命令的行为参数</param>
        /// <returns>返回结果</returns>
        public static IEnumerable<T> ExecuteObjects<T>(IDbCommand command, CommandBehavior behavior = CommandBehavior.Default)
            where T : new()
        {
            var tType = typeof (T);
            var properties = ReflectHelper.GetProperties(tType);

            var nameIdDic = new Dictionary<string, int>();
            var reader = command.ExecuteReader(behavior);
            foreach (var name in properties.Where(p => p.CanWrite).Select(property => property.Name))
            {
                try
                {
                    var id = reader.GetOrdinal(name);
                    nameIdDic.Add(name, id);
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            while (reader.Read())
            {
                var obj = new T();
                foreach (var nameId in nameIdDic)
                {
                    var name = nameId.Key;
                    var id = nameId.Value;

                    var property = properties.First(p => p.Name == name);
                    var dbValue = reader.GetValue(id);
                    var value = dbValue == DBNull.Value ? null : dbValue;
                    property.SetValue(obj, value);
                }
                yield return obj;
            }
        }

        /// <summary>
        /// 查询返回结果的第一行和第一列
        /// </summary>
        public static T ExecuteObject<T>(IDbCommand command)
        {
            var result = command.ExecuteScalar();
            return (result == null || result == DBNull.Value) ? default(T) : (T) result;
        }
    }
}
