using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace Wantalgh.LightDataClient
{
    internal static class DbCommandHelper
    {
        private static readonly Dictionary<Type, DbType> TypeMapDic = new Dictionary<Type, DbType>();

        static DbCommandHelper()
        {
            TypeMapDic.Add(typeof(byte), DbType.Byte);
            TypeMapDic.Add(typeof(sbyte), DbType.SByte);
            TypeMapDic.Add(typeof(short), DbType.Int16);
            TypeMapDic.Add(typeof(ushort), DbType.UInt16);
            TypeMapDic.Add(typeof(int), DbType.Int32);
            TypeMapDic.Add(typeof(uint), DbType.UInt32);
            TypeMapDic.Add(typeof(long), DbType.Int64);
            TypeMapDic.Add(typeof(ulong), DbType.UInt64);
            TypeMapDic.Add(typeof(float), DbType.Single);
            TypeMapDic.Add(typeof(double), DbType.Double);
            TypeMapDic.Add(typeof(decimal), DbType.Decimal);
            TypeMapDic.Add(typeof(bool), DbType.Boolean);
            TypeMapDic.Add(typeof(string), DbType.String);
            TypeMapDic.Add(typeof(char), DbType.StringFixedLength);
            TypeMapDic.Add(typeof(Guid), DbType.Guid);
            TypeMapDic.Add(typeof(DateTime), DbType.DateTime);
            TypeMapDic.Add(typeof(DateTimeOffset), DbType.DateTimeOffset);
            TypeMapDic.Add(typeof(TimeSpan), DbType.Time);
            TypeMapDic.Add(typeof(byte[]), DbType.Binary);
            TypeMapDic.Add(typeof(byte?), DbType.Byte);
            TypeMapDic.Add(typeof(sbyte?), DbType.SByte);
            TypeMapDic.Add(typeof(short?), DbType.Int16);
            TypeMapDic.Add(typeof(ushort?), DbType.UInt16);
            TypeMapDic.Add(typeof(int?), DbType.Int32);
            TypeMapDic.Add(typeof(uint?), DbType.UInt32);
            TypeMapDic.Add(typeof(long?), DbType.Int64);
            TypeMapDic.Add(typeof(ulong?), DbType.UInt64);
            TypeMapDic.Add(typeof(float?), DbType.Single);
            TypeMapDic.Add(typeof(double?), DbType.Double);
            TypeMapDic.Add(typeof(decimal?), DbType.Decimal);
            TypeMapDic.Add(typeof(bool?), DbType.Boolean);
            TypeMapDic.Add(typeof(char?), DbType.StringFixedLength);
            TypeMapDic.Add(typeof(Guid?), DbType.Guid);
            TypeMapDic.Add(typeof(DateTime?), DbType.DateTime);
            TypeMapDic.Add(typeof(DateTimeOffset?), DbType.DateTimeOffset);
            TypeMapDic.Add(typeof(TimeSpan?), DbType.Time);
            TypeMapDic.Add(typeof(Object), DbType.Object);
        }


        /// <summary>
        /// 组建查询参数
        /// </summary>
        public static void AddParameters(this IDbCommand command, object parameterObj, string namePrefix = "")
        {
            if (parameterObj != null)
            {
                var objType = parameterObj.GetType();
                var properties = ReflectHelper.GetProperties(objType);

                foreach (var property in properties)
                {
                    var parameter = command.CreateParameter();
                    var paramType = property.PropertyType.IsEnum ? Enum.GetUnderlyingType(property.PropertyType) : property.PropertyType;
                    if (TypeMapDic.TryGetValue(paramType, out var dbType))
                    {
                        parameter.DbType = dbType;  
                    }
                    parameter.ParameterName = namePrefix + property.Name;
                    var value = property.GetValue(parameterObj);
                    parameter.Value = value ?? DBNull.Value;
                    command.Parameters.Add(parameter);
                }
            }
        }

        /// <summary>
        /// 执行SQL，把结果映射到对象上。
        /// </summary>
        /// <typeparam name="T">对象的类型，类型的属性名应与SQL返回的列名一致。</typeparam>
        /// <param name="command">被扩展调用的DbCommand对象</param>
        /// <param name="behavior">执行命令的行为参数</param>
        /// <returns>返回结果</returns>
        public static IEnumerable<T> ExecuteObjects<T>(this IDbCommand command, CommandBehavior behavior = CommandBehavior.Default)
            where T : new()
        {
            var tType = typeof(T);
            var properties = ReflectHelper.GetProperties(tType);

            var reader = command.ExecuteReader(behavior);

            var propertyColumnMap = new Dictionary<PropertyInfo, int>();
            foreach (var property in properties.Where(p => p.CanWrite))
            {
                try
                {
                    var id = reader.GetOrdinal(property.Name);
                    propertyColumnMap.Add(property, id);
                }
                catch (IndexOutOfRangeException)
                {
                }
            }

            while (reader.Read())
            {
                var obj = new T();
                foreach (var propertyId in propertyColumnMap)
                {
                    var property = propertyId.Key;
                    var id = propertyId.Value;
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
        public static T ExecuteObject<T>(this IDbCommand command)
        {
            var result = command.ExecuteScalar();
            return (result == null || result == DBNull.Value) ? default(T) : (T)result;
        }
    }
}
