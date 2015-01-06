using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// ODBC参数构建类
    /// </summary>
    class OdbcParameterBuilder : DbParameterBuilderBase
    {
        /// <summary>
        /// 构建参数
        /// </summary>
        protected override IDataParameter CreateClientParameter()
        {
            return new OdbcParameter();
        }
    }
}
