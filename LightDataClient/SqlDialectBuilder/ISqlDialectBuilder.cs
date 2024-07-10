// SPDX-License-Identifier: MIT

using System.Collections.Generic;

namespace Wantalgh.LightDataClient.SqlDialectBuilder
{
    /// <summary>
    /// SQL dialect builder, used to build SQL statements
    /// </summary>
    public interface ISqlDialectBuilder
    {
        /// <summary>
        /// Build SELECT SQL for execution.
        /// </summary>
        /// <param name="tableName">
        /// The table name of SELECT statement.
        /// </param>
        /// <param name="columns">
        /// Column names and parameters of SELECT statement.
        /// </param>
        /// <param name="condition">
        /// Where conditions and parameters of SELECT statement.
        /// </param>
        /// <param name="skip">
        /// Specify the number of rows to be skipped. Takes effect only when skip and take parameters are both specified and not less than 0.
        /// </param>
        /// <param name="take">
        /// Specify the number of rows to be returned. Takes effect only when skip and take parameters are both specified and not less than 0.
        /// </param>
        /// <returns>
        /// Generated SQL statement.
        /// </returns>
        string BuildSelectSql(string tableName, IEnumerable<string> columns, IEnumerable<KeyValuePair<string, string>> condition, int? skip = null, int? take = null);

        /// <summary>
        /// Build INSERT SQL for execution.
        /// </summary>
        /// <param name="tableName">
        /// The table name of INSERT statement.
        /// </param>
        /// <param name="columns">
        /// Column names and parameters of INSERT statement.
        /// </param>
        /// <returns>
        /// Generated SQL statement.
        /// </returns>
        string BuildInsertSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns);


        /// <summary>
        /// Build UPDATE SQL for execution.
        /// </summary>
        /// <param name="tableName">
        /// The table name of UPDATE statement.
        /// </param>
        /// <param name="columns">
        /// Column names and parameters of UPDATE statement.
        /// </param>
        /// <param name="condition">
        /// Where conditions and parameters of UPDATE statement.
        /// </param>
        /// <returns>
        /// Generated SQL statement.
        /// </returns>
        string BuildUpdateSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns, IEnumerable<KeyValuePair<string, string>> condition);

        /// <summary>
        /// Build DELETE SQL for execution.
        /// </summary>
        /// <param name="tableName">
        /// The table name of DELETE statement.
        /// </param>
        /// <param name="condition">
        /// Where conditions and parameters of DELETE statement.
        /// </param>
        /// <returns>
        /// Generated SQL statement.
        /// </returns>
        string BuildDeleteSql(string tableName, IEnumerable<KeyValuePair<string, string>> condition);

        /// <summary>
        /// Build InsertOrUpdate SQL for execution.
        /// </summary>
        /// <param name="tableName">
        /// The table name of InsertOrUpdate statement.
        /// </param>
        /// <param name="columns">
        /// Column names and parameters of InsertOrUpdate statement.
        /// </param>
        /// <param name="condition">
        /// Where conditions and parameters of InsertOrUpdate statement.
        /// </param>
        /// <returns>
        /// Generated SQL statement.
        /// </returns>
        string BuildMergeSql(string tableName, IEnumerable<KeyValuePair<string, string>> columns, IEnumerable<KeyValuePair<string, string>> condition);
    }
}
