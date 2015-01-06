using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// 数据参数构建器的工厂
    /// </summary>
    internal static class DbParameterBuilderFactory
    {
        /// <summary>
        /// 参数构建器单例字典
        /// </summary>
        private static readonly ConcurrentDictionary<DbClientTypeEnum, IDbParameterBuilder> ParamBuilderDic;

        /// <summary>
        /// 初始化构建器单例字典
        /// </summary>
        static DbParameterBuilderFactory()
        {
            ParamBuilderDic = new ConcurrentDictionary<DbClientTypeEnum, IDbParameterBuilder>();
        }

        /// <summary>
        /// 获取构建器
        /// </summary>
        public static IDbParameterBuilder GetBuilder(DbClientTypeEnum clientType)
        {
            var builder = ParamBuilderDic.GetOrAdd(clientType, t =>
            {
                switch (t)
                {
                    case DbClientTypeEnum.SqlClient:
                        return new SqlParameterBuilder();
                    case DbClientTypeEnum.OleDb:
                        return new OleDbParameterBuilder();
                    case DbClientTypeEnum.Odbc:
                        return new OdbcParameterBuilder();
                    case DbClientTypeEnum.OracleClient:
                        return new OracleParameterBuilder();
                    default:
                        throw new ArgumentOutOfRangeException("clientType");
                }
            });
            return builder;
        }
    }
}
