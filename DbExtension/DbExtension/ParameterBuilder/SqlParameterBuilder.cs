using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// SqlServer参数构建器
    /// </summary>
    class SqlParameterBuilder: DbParameterBuilderBase
    {
        /// <summary>
        /// 构建SqlServer参数
        /// </summary>
        protected override IDataParameter CreateClientParameter()
        {
            return new SqlParameter();
        }
    }
}
