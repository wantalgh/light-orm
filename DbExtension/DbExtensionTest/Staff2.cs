using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtensionTest
{
    /// <summary>
    /// 对应Staff表，属性名与表的列名一致，但类名与表名不一致。
    /// </summary>
    public class Staff2 : Staff
    {
        //属性不再自己写，使用父类的。
    }
}
