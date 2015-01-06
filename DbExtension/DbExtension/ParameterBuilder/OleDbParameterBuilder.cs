using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// OleDb参数构建类
    /// </summary>
    class OleDbParameterBuilder: DbParameterBuilderBase
    {
        /// <summary>
        /// 构建OleDb参数
        /// </summary>
        protected override IDataParameter CreateClientParameter()
        {
            return new OleDbParameter();
        }
    }
}
