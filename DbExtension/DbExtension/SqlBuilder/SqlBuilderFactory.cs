using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// sql构建器工厂
    /// </summary>
    static class SqlBuilderFactory
    {
        /// <summary>
        /// 参数构建器单例字典
        /// </summary>
        private static readonly ConcurrentDictionary<AutoSqlTypeEnum, ISqlBuilder> SqlBuilderDic;

        /// <summary>
        /// 初始化
        /// </summary>
        static SqlBuilderFactory()
        {
            SqlBuilderDic = new ConcurrentDictionary<AutoSqlTypeEnum, ISqlBuilder>();
        }

        /// <summary>
        /// 获取SQL构建器
        /// </summary>
        public static ISqlBuilder GetBuilder(AutoSqlTypeEnum autoSqlType)
        {
            var builder = SqlBuilderDic.GetOrAdd(autoSqlType, t =>
            {
                switch (t)
                {
                    case AutoSqlTypeEnum.AnsiSql:
                        return new AnsiSql92Builder();
                    case AutoSqlTypeEnum.JetSql:
                        return new JetSql4Builder();
                    case AutoSqlTypeEnum.PlSql:
                        return new PlSql11Builder();
                    case AutoSqlTypeEnum.Tsql:
                        return new Tsql2005Builder();
                    default:
                        throw new ArgumentOutOfRangeException("autoSqlType");
                }
            });
            return builder;
        }
    }
}
