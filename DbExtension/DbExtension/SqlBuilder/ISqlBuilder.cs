using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// SQL命令
    /// </summary>
    interface ISqlBuilder
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tableName">要操作的数据表架构</param>
        /// <param name="columns">要查询的列</param>
        /// <param name="keyColumnDic">查询键与数据列的对应字典</param>
        /// <param name="skip">要跳过的数据行数。(-1表示不采用此机制)</param>
        /// <param name="take">要取的数据行数</param>
        /// <returns>生成的SQL语句</returns>
        string BuildSelectSql(string tableName, IEnumerable<string> columns, IEnumerable<KeyValuePair<string, string>> keyColumnDic, int? skip = null, int? take = null);

        /// <summary>
        /// 构建简单INSERT操作的SQL语句
        /// </summary>
        /// <param name="tableName">要操作的数据表架构</param>
        /// <param name="parameterColumnDic">参数与数据列的对应字典</param>
        /// <returns>生成的SQL语句</returns>
        string BuildInsertSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic);

        /// <summary>
        /// 构建UPDATE操作的SQL语句
        /// </summary>
        /// <param name="tableName">要操作的表架构名</param>
        /// <param name="parameterColumnDic">参数与数据列的对应字典</param>
        /// <param name="keyColumnDic">键与数据列的对应字典</param>
        /// <returns>构建的SQL语句</returns>
        string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic);

        /// <summary>
        /// 构建删除操作的SQL语句
        /// </summary>
        /// <param name="tableName">要删除数据的表名</param>
        /// <param name="keyColumnDic">要删除数据的键</param>
        /// <returns>生成的SQL语句</returns>
        string BuildDeleteSql(string tableName, IEnumerable<KeyValuePair<string, string>> keyColumnDic);

        /// <summary>
        /// 组建AddOrUpdate操作的SQL
        /// </summary>
        /// <param name="tableName">要操作的表架构名</param>
        /// <param name="parameterColumnDic">参数与列名对应字典</param>
        /// <param name="keyColumnDic">查找条件与列名对应字典</param>
        /// <returns>拼装好的SQL语句</returns>
        string BuildMergeSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic);
    }
}
