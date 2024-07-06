using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wantalgh.LightDataClient.SqlDialectBuilder
{
    public class Sqlite3SqlBuilder : ISqlDialectBuilder
    {
        public string BuildSelectSql(string tableName, IEnumerable<string> columns, IEnumerable<KeyValuePair<string, string>> condition, int? skip = null, int? take = null)
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

            if (skip != null && take != null)
            {
                sqlBuilder.Append($" LIMIT {take.Value} OFFSET {skip.Value}");
            }
            var sql = sqlBuilder.ToString();

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

        public string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns, IEnumerable<KeyValuePair<string, string>> condition)
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
            var sqlBuilder = new StringBuilder();
            var columnAry = columns.ToArray();
            var updateSql = BuildUpdateSql(tableName, columnAry, condition);
            sqlBuilder.Append(updateSql);
            sqlBuilder.Append("; ");
            sqlBuilder.Append(" INSERT INTO " + GetFieldName(tableName));
            sqlBuilder.Append(" (");
            sqlBuilder.Append(string.Join(",", columnAry.Select(p => GetFieldName(p.Key))));
            sqlBuilder.Append(" )");
            sqlBuilder.Append(" SELECT ");
            sqlBuilder.Append(string.Join(",", columnAry.Select(p => GetParamName(p.Value))));
            sqlBuilder.Append(" WHERE (SELECT changes() = 0)");

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 获取字段名称
        /// </summary>
        private static string GetFieldName(string field)
        {
            return $"\"{field}\"";
        }

        private static string GetParamName(string param)
        {
            return $"@{param}";
        }
    }
}
