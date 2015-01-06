using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// SQL查询参数构建类
    /// </summary>
    abstract class DbParameterBuilderBase : IDbParameterBuilder
    {
        protected Dictionary<Type, DbType> TypeMapDic = new Dictionary<Type, DbType>();

        protected DbParameterBuilderBase()
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
        /// 根据对象构建查询参数
        /// </summary>
        public virtual IDataParameter[] BuildParameters(object parameterObj, string namePrefix = "")
        {
            var parameterList = new List<IDataParameter>();

            if (parameterObj != null)
            {
                var objType = parameterObj.GetType();
                var properties = ReflectHelper.GetProperties(objType);

                foreach (var property in properties)
                {
                    var paramType = property.PropertyType.IsEnum ? Enum.GetUnderlyingType(property.PropertyType) : property.PropertyType;

                    var parameter = CreateClientParameter();
                    parameter.DbType = TypeMapDic[paramType];
                    parameter.ParameterName = namePrefix + property.Name;
                    var value = property.GetValue(parameterObj);
                    parameter.Value = value ?? DBNull.Value;

                    parameterList.Add(parameter);
                }
            }

            return parameterList.ToArray();
        }

        /// <summary>
        /// 创建SQL查询参数
        /// </summary>
        protected abstract IDataParameter CreateClientParameter();
    }
}
