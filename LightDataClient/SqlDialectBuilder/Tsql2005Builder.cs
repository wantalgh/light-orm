using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wantalgh.LightDataClient.SqlDialectBuilder
{
    public class Tsql2005Builder : ISqlDialectBuilder
    {
        public string BuildSelectSql(string tableName, IEnumerable<string> columns,
            IEnumerable<KeyValuePair<string, string>> condition, int? skip = null, int? take = null)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" SELECT ");
            sqlBuilder.Append(string.Join(",", columns.Select(GetFieldName)));
            sqlBuilder.Append(" FROM " + GetFieldName(tableName));

            if (condition != null)
            {
                var conditionAry = condition.ToArray();
                if (conditionAry.Length > 0)
                {
                    sqlBuilder.Append(" WHERE ");
                    var where = conditionAry.Select(p =>
                        $"{GetFieldName(p.Key)} = {GetParamName(p.Value)}");
                    var whereStr = string.Join(" AND ", where);
                    sqlBuilder.Append(whereStr);
                }
            }

            var sql = sqlBuilder.ToString();
            if (skip != null && take != null)
            {
                sql = PageQueryHelper.BuildQuerySql(sql, skip.Value, take.Value, tableName);
            }

            return sql;
        }

        public string BuildInsertSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns)
        {
            var columnAry = columns.ToArray();

            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" INSERT INTO " + GetFieldName(tableName));
            sqlBuilder.Append(" (");
            sqlBuilder.Append(string.Join(",", columnAry.Select(p => GetFieldName(p.Key))));
            sqlBuilder.Append(" )");
            sqlBuilder.Append(" VALUES");
            sqlBuilder.Append(" (");
            sqlBuilder.Append(string.Join(",", columnAry.Select(p => GetParamName(p.Value))));
            sqlBuilder.Append(" )");

            return sqlBuilder.ToString();
        }

        public string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns,
            IEnumerable<KeyValuePair<string, string>> condition)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" UPDATE " + GetFieldName(tableName));
            sqlBuilder.Append(" SET ");
            var param = columns.Select(p =>
                $"{GetFieldName(p.Key)} = {GetParamName(p.Value)}");
            var paramStr = string.Join(",", param);
            sqlBuilder.Append(paramStr);

            if (condition != null)
            {
                var conditionAry = condition.ToArray();
                if (conditionAry.Length > 0)
                {
                    sqlBuilder.Append(" WHERE ");
                    var where = conditionAry.Select(p =>
                        $"{GetFieldName(p.Key)} = {GetParamName(p.Value)}");
                    var whereStr = string.Join(" AND ", where);
                    sqlBuilder.Append(whereStr);
                }
            }
            return sqlBuilder.ToString();
        }

        public string BuildDeleteSql(string tableName, IEnumerable<KeyValuePair<string, string>> condition)
        {
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append(" DELETE FROM " + GetFieldName(tableName));

            if (condition != null)
            {
                var conditionAry = condition.ToArray();
                if (conditionAry.Length > 0)
                {
                    sqlBuilder.Append(" WHERE ");
                    var where = conditionAry.Select(p =>
                        $"{GetFieldName(p.Key)} = {GetParamName(p.Value)}");
                    var whereStr = string.Join(" AND ", where);
                    sqlBuilder.Append(whereStr);
                }
            }
            return sqlBuilder.ToString();
        }

        public string BuildMergeSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns,
            IEnumerable<KeyValuePair<string, string>> condition)
        {
            KeyValuePair<string, string>[] conditionAry = null;
            var whereBuilder = new StringBuilder();
            if (condition != null)
            {
                conditionAry = condition.ToArray();
                if (conditionAry.Length > 0)
                {
                    whereBuilder.Append(" WHERE ");
                    var where = conditionAry.Select(p =>
                        $"{GetFieldName(p.Key)} = {GetParamName(p.Value)}");
                    var whereStr = string.Join(" AND ", where);
                    whereBuilder.Append(whereStr);
                }
            }

            var columnAry = columns.ToArray();
            var sqlBuilder = new StringBuilder();
            sqlBuilder.Append($"IF EXISTS(SELECT * FROM {GetFieldName(tableName)} {whereBuilder})");
            sqlBuilder.Append(" BEGIN");
            sqlBuilder.Append(BuildUpdateSql(tableName, columnAry, conditionAry));
            sqlBuilder.Append(" END ELSE BEGIN");
            sqlBuilder.Append(BuildInsertSql(tableName, columnAry));
            sqlBuilder.Append(" END");

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        private static string GetFieldName(string field)
        {
            return $"[{field}]";
        }

        /// <summary>
        /// 获取参数名称
        /// </summary>
        private static string GetParamName(string param)
        {
            return $"@{param}";
        }

        /// <summary>
        /// 分页查询工具类
        /// </summary>
        private static class PageQueryHelper
        {
            const string SqlTemplate =
                " SELECT * FROM (SELECT *, ROW_NUMBER() OVER(ORDER BY (SELECT 0)) AS [{2}] FROM ({0}) AS [{1}]) AS [{1}] WHERE [{2}] BETWEEN {3} AND {4}";

            private static readonly Regex CheckSelectRgx =
                new Regex(@"^ *SELECT +?TOP.*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            private static readonly Regex ReplaceSelectRgx =
                new Regex(@"^ *SELECT(?=.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <summary>
            /// 构建分页查询的SQL语句
            /// </summary>
            public static string BuildQuerySql(string sql, int skipRows, int takeRows,
                string pageTableName = "PagedDataTable", string numRowName = "rownum")
            {
                var startRows = skipRows + 1;
                var endRows = skipRows + takeRows;

                if (CheckSelectRgx.IsMatch(sql) == false)
                {
                    sql = ReplaceSelectRgx.Replace(sql,
                        $" SELECT TOP {endRows.ToString(CultureInfo.InvariantCulture)} ");
                }

                if (endRows < 0)
                {
                    endRows = int.MaxValue;
                }

                var querySql = string.Format(SqlTemplate, sql, pageTableName, numRowName,
                    startRows.ToString(CultureInfo.InvariantCulture), endRows.ToString(CultureInfo.InvariantCulture));
                return querySql;
            }
        }
    }
}
