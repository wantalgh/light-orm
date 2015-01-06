using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// DataAdapter的组建类
    /// </summary>
    internal static class DataAdapterBuilderFactory
    {
        /// <summary>
        /// 客户端的数据适配器构建工厂
        /// </summary>
        /// <param name="command">数据执行命令</param>
        /// <param name="clientType">客户端类型</param>
        /// <returns>数据适配器</returns>
        public static IDataAdapter BuildDataAdapter(IDbCommand command, DbClientTypeEnum clientType)
        {
            IDataAdapter dataAdapter;

            switch (clientType)
            {
                case DbClientTypeEnum.SqlClient:
                    dataAdapter = new SqlDataAdapter(command as SqlCommand);
                    break;
                case DbClientTypeEnum.Odbc:
                    dataAdapter = new OdbcDataAdapter(command as OdbcCommand);
                    break;
                case DbClientTypeEnum.OleDb:
                    dataAdapter = new OleDbDataAdapter(command as OleDbCommand);
                    break;
                case DbClientTypeEnum.OracleClient:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException("clientType");
            }

            return dataAdapter;
        }
    }
}
