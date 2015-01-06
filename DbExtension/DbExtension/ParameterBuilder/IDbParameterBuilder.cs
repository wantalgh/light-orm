using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// 查询参数组建接口
    /// </summary>
    interface IDbParameterBuilder
    {
        /// <summary>
        /// 组建查询参数
        /// </summary>
        IDataParameter[] BuildParameters(object parameterObj, string namePrefix = "");
    }
}
