using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WT.Data.DbExtension;

namespace WT.Data.DbExtensionTest
{
    /// <summary>
    /// 数据源工厂
    /// </summary>
    public static class DataSourceFactory
    {
        static DataSourceFactory()
        {
            DataSource = new SqlDataSource("TestDbContextConnStr");
        }

        /// <summary>
        /// 数据源
        /// </summary>
        public static SqlDataSource DataSource
        { get; private set; }
    }
}
