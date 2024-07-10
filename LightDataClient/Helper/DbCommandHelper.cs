// SPDX-License-Identifier: MIT

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
        /// Add object parameters to command
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
        /// Executes a db command and map result to enumerable of T.
        /// </summary>
        /// <typeparam name="T">
        /// The type of the result list.
        /// Each row in the result is mapped to an instance of T, values of columns are mapped to properties of T with same name.
        /// </typeparam>
        /// <param name="command">
        /// DbCommand which contains sql and parameters.
        /// </param>
        /// <param name="behavior">
        /// The behavior of executing sql, if null, CommandBehavior.Default will be used. For details, <see cref="CommandBehavior"/>
        /// </param>
        /// <returns>
        /// Result of command, each row is mapped to an instance of T.
        /// </returns>
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
                foreach (var propertyId in propertyColumnMap.ToArray())
                {
                    try
                    {
                        var property = propertyId.Key;
                        var id = propertyId.Value;
                        var dbValue = reader.GetValue(id);
                        var value = dbValue == DBNull.Value ? null : dbValue;
                        property.SetValue(obj, value);
                    }
                    catch (IndexOutOfRangeException)
                    {
                        propertyColumnMap.Remove(propertyId.Key);
                    }
                }

                yield return obj;
            }
        }

        /// <summary>
        /// Executes a db command and returns a single value.
        /// </summary>
        public static T ExecuteObject<T>(this IDbCommand command)
        {
            var result = command.ExecuteScalar();
            return (result == null || result == DBNull.Value) ? default(T) : (T)result;
        }
    }
}
