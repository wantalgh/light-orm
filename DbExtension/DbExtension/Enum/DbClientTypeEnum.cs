using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{

    /// <summary>
    /// 表示数据库客户端的类型
    /// </summary>
    internal enum DbClientTypeEnum
    {
        /// <summary>
        /// System.Data.SqlClient客户端
        /// </summary>
        SqlClient = 0,

        /// <summary>
        /// System.Data.OleDb客户端
        /// </summary>
        OleDb = 1,

        /// <summary>
        /// System.Data.Odbc客户端
        /// </summary>
        Odbc = 2,

        /// <summary>
        /// System.Data.Oracle客户端(尚未实现)
        /// </summary>
        OracleClient = 3
    }
}
