using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// SqlServer命令构建器
    /// </summary>
    class Tsql2005Builder: ISqlBuilder
    {
        public string BuildSelectSql(string tableName, IEnumerable<string> columns, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" SELECT ");
            sqlBuilder.Append(string.Join(",", columns.Select(GetFieldName)));
            sqlBuilder.Append(" FROM " + GetFieldName(tableName));

            var keyColumnDic2 = keyColumnDic.ToCollection();

            if (keyColumnDic2 != null && keyColumnDic2.Count > 0)
            {
                sqlBuilder.Append(" WHERE ");
                var ids = keyColumnDic2.Select(p => string.Format("{0} = {1}", GetFieldName(p.Value), GetParamName(p.Key)));
                var idStr = string.Join(" AND ", ids);
                sqlBuilder.Append(idStr);
            }

            var sql = sqlBuilder.ToString();
            return sql;
        }


        public string DecoratePageSelectSql(string selectSql, int skip, int take)
        {
            var sql = PageQueryHelper.BuildQuerySql(selectSql, skip, take);
            return sql;
        }

        public string BuildInsertSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic)
        {
            var parameterColumnDic2 = parameterColumnDic.ToCollection();

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" INSERT INTO " + GetFieldName(tableName));
            sqlBuilder.Append(" (");
            sqlBuilder.Append(string.Join(",", parameterColumnDic2.Select(p => GetFieldName(p.Value))));
            sqlBuilder.Append(" )");
            sqlBuilder.Append(" VALUES");
            sqlBuilder.Append(" (");
            sqlBuilder.Append(string.Join(",", parameterColumnDic2.Select(p => GetParamName(p.Key))));
            sqlBuilder.Append(" )");

            return sqlBuilder.ToString();
        }

        public string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" UPDATE " + GetFieldName(tableName));
            sqlBuilder.Append(" SET ");
            var param = parameterColumnDic.Select(p => string.Format("{0} = {1}", GetFieldName(p.Value), GetParamName(p.Key)));
            var paramStr = string.Join(",", param);
            sqlBuilder.Append(paramStr);

            var keyColumnDic2 = keyColumnDic.ToCollection();
            if (keyColumnDic2 != null && keyColumnDic2.Count > 0)
            {
                sqlBuilder.Append(" WHERE ");
                var ids = keyColumnDic2.Select(p => string.Format("{0} = {1}", GetFieldName(p.Value), GetParamName(p.Key)));
                var idStr = string.Join(" AND ", ids);
                sqlBuilder.Append(idStr);
            }

            return sqlBuilder.ToString();
        }

        public string BuildDeleteSql(string tableName, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" DELETE FROM " + GetFieldName(tableName));

            var keyColumnDic2 = keyColumnDic.ToCollection();
            if (keyColumnDic2 != null && keyColumnDic2.Count > 0)
            {
                sqlBuilder.Append(" WHERE ");
                var ids = keyColumnDic2.Select(p => string.Format("{0} = {1}", GetFieldName(p.Value), GetParamName(p.Key)));
                var idStr = string.Join(" AND ", ids);
                sqlBuilder.Append(idStr);
            }

            return sqlBuilder.ToString();
        }

        public string BuildMergeSql(string tableName, IEnumerable<KeyValuePair<string, string>> parameterColumnDic, IEnumerable<KeyValuePair<string, string>> keyColumnDic)
        {
            var parameterColumnDic2 = parameterColumnDic.ToCollection();
            var keyColumnDic2 = keyColumnDic.ToCollection();
            var whereBuilder = new StringBuilder();
            if (keyColumnDic2 != null && keyColumnDic2.Count > 0)
            {
                whereBuilder.Append(" WHERE ");
                var ids = keyColumnDic2.Select(p => string.Format("{0} = {1}", GetFieldName(p.Value), GetParamName(p.Key)));
                var idStr = string.Join(" AND ", ids);
                whereBuilder.Append(idStr);
            }

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(string.Format(" IF EXISTS(SELECT * FROM {0} {1})", GetFieldName(tableName), whereBuilder));
            sqlBuilder.Append(" BEGIN");
            sqlBuilder.Append(BuildUpdateSql(tableName, parameterColumnDic2, keyColumnDic2));
            sqlBuilder.Append(" END ELSE BEGIN");
            sqlBuilder.Append(BuildInsertSql(tableName, parameterColumnDic2));
            sqlBuilder.Append(" END");

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        private static string GetFieldName(string field)
        {
            return string.Format("[{0}]", field);
        }

        /// <summary>
        /// 获取参数名称
        /// </summary>
        public static string GetParamName(string param)
        {
            return string.Format("@{0}", param);
        }

        /// <summary>
        /// 分页查询工具类
        /// </summary>
        private static class PageQueryHelper
        {
            const string SqlTemplate = " SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT 0)) AS [{2}] FROM ({0}) AS [{1}]) AS [{1}] WHERE [{2}] BETWEEN {3} AND {4}";
            private static readonly Regex CheckSelectRgx = new Regex(@"^ *SELECT +?TOP.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static readonly Regex ReplaceSelectRgx = new Regex(@"^ *SELECT(?=.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            private static readonly Regex CheckTableNameRgx = new Regex(@"^ *SELECT +.*? +FROM +(?<TableName>[^ ]*) *.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <summary>
            /// 构建分页查询的SQL语句
            /// </summary>
            public static string BuildQuerySql(string sql, int skipRows, int takeRows, string numRowName = "rownum")
            {
                var startRows = skipRows + 1;
                var endRows = skipRows + takeRows;

                if (CheckSelectRgx.IsMatch(sql) == false)
                {
                    sql = ReplaceSelectRgx.Replace(sql, string.Format(" SELECT TOP {0} ", endRows.ToString(CultureInfo.InvariantCulture)));
                }
                if (endRows < 0)
                {
                    endRows = int.MaxValue;
                }

                var tableNameMatch = CheckTableNameRgx.Match(sql);
                var tableName = tableNameMatch.Groups["TableName"].Value;
                tableName = tableName.TrimStart('[').TrimEnd(']');
                var querySql = string.Format(SqlTemplate, sql, tableName, numRowName, startRows.ToString(CultureInfo.InvariantCulture), endRows.ToString(CultureInfo.InvariantCulture));
                return querySql;
            }
        }
    }
}
