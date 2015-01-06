using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// Oracle参数构建器
    /// </summary>
    internal class OracleParameterBuilder : DbParameterBuilderBase
    {
        /// <summary>
        /// 。。。
        /// </summary>
        protected override IDataParameter CreateClientParameter()
        {
            throw new NotImplementedException();
        }
    }
}
