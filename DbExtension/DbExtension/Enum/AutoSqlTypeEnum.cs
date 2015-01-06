using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// SQL语句版本类型
    /// </summary>
    public enum AutoSqlTypeEnum
    {
        /// <summary>
        /// SQLServer使用的Transact SQL
        /// </summary>
        Tsql = 0,

        /// <summary>
        /// Oracle使用的Procedural Language SQL
        /// </summary>
        PlSql = 1,

        /// <summary>
        /// JET SQL
        /// </summary>
        JetSql = 2,

        /// <summary>
        /// ANSI SQL
        /// </summary>
        AnsiSql = 3
    }
}
