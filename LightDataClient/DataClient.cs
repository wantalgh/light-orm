// SPDX-License-Identifier: MIT

using System.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Wantalgh.LightDataClient;
using System.Data.Common;
using Wantalgh.LightDataClient.SqlDialectBuilder;

namespace Wantalgh.LightDataClient
{
    /// <summary>
    /// A light-weight and easy-to-use SQL data client.
    /// </summary>
    public class DataClient
    {
        private readonly ISqlDialectBuilder _dialectBuilder;

        private readonly Func<IDbConnection> _connectionFactory;

        /// <summary>
        /// Creates a new instance of the <see cref="DataClient"/> class.
        /// </summary>
        /// <param name="connectionFactory">
        /// The connection factory. This client will retrieve connection from this factory.
        /// </param>
        /// <param name="dialectBuilder">
        /// The sql dialect builder. If null, <see cref="Tsql2005Builder"/> will be used.
        /// </param>
        public DataClient(Func<IDbConnection> connectionFactory, ISqlDialectBuilder dialectBuilder = null)
        {
            _connectionFactory = connectionFactory;
            _dialectBuilder = dialectBuilder ?? new Tsql2005Builder();
        }

        /// <summary>
        /// Executes SQL and returns a list of T.
        /// This method can execute specified or generated SQL, then automatically map column names to property names.
        /// </summary>
        /// <typeparam name="T">
        /// Item type of the result list. 
        /// Each row of the SQL result will be mapped to an instance of T, columns will be mapped to properties of T with the same name by default.
        /// When generating SQL, T's type name will be used as table name, and each property's name will be used as column name by default.
        /// </typeparam>
        /// <param name="sql">
        /// SQL to be executed. If null, this method will automatically generate SELECT SQL.
        /// </param>
        /// <param name="tableName">
        /// When generating SQL, this parameter will be used as table name. If null, T's type name will be used.
        /// </param>
        /// <param name="parameter">
        /// When generating SQL, this parameter will be used to generate sql query parameters.
        /// This parameter is an object whose properties will be used to generate query parameters of the same name.
        /// </param>
        /// <param name="skip">
        /// When generating SQL, if specified, this parameter will be used to skip specified number of rows.
        /// </param>
        /// <param name="take">
        /// When generating SQL, if specified, this parameter will be used to limit specified number of rows.
        /// </param>
        /// <param name="commandType">
        /// The type of sql to execute, if null, CommandType.Text will be used. For details, <see cref="CommandType"/>.
        /// </param>
        /// <param name="commandBehavior">
        /// The behavior of executing sql, if null, CommandBehavior.Default will be used. For details, <see cref="CommandBehavior"/>
        /// </param>
        /// <returns>
        /// Sql query result, automatically mapped to IEnumerable of T
        /// </returns>
        public IEnumerable<T> ExecuteModels<T>(string sql = null, object parameter = null, string tableName = null, int? skip = null, int? take = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default)
            where T : new()
        {
            if (sql == null)
            {
                var type = typeof(T);
                if (tableName == null)
                {
                    tableName = type.Name;
                }
                var columns = ReflectHelper.GetProperties(type).Select(p => p.Name);
                var condition = GetObjectParameterDic(parameter);
                sql = _dialectBuilder.BuildSelectSql(tableName, columns, condition, skip, take);
            }

            IEnumerable<T> result;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql, commandType, parameter))
                {
                    result = command.ExecuteObjects<T>(commandBehavior);
                    result = result.ToArray();
                }
            }
            return result;
        }

        /// <summary>
        /// Executes SQL and returns the first row.
        /// Equivalent to ExecuteModels method, but only get the first row, other rows will be ignored. />
        /// </summary>
        /// <typeparam name="T">
        /// Item type of the result list. 
        /// Each row of the SQL result will be mapped to an instance of T, columns will be mapped to properties of T with the same name by default.
        /// When generating SQL, T's type name will be used as table name, and each property's name will be used as column name by default.
        /// </typeparam>
        /// <param name="sql">
        /// SQL to be executed. If null, this method will automatically generate SELECT SQL.
        /// </param>
        /// <param name="tableName">
        /// When generating SQL, this parameter will be used as table name. If null, T's type name will be used.
        /// </param>
        /// <param name="parameter">
        /// When generating SQL, this parameter will be used to generate sql query parameters.
        /// This parameter is an object whose properties will be used to generate query parameters of the same name.
        /// </param>
        /// <param name="commandType">
        /// The type of sql to execute, if null, CommandType.Text will be used. For details, <see cref="CommandType"/>.
        /// </param>
        /// <param name="commandBehavior">
        /// The behavior of executing sql, if null, CommandBehavior.Default will be used. For details, <see cref="CommandBehavior"/>
        /// </param>
        /// <returns>
        /// First row of result, automatically mapped to T
        /// </returns>
        public T ExecuteModel<T>(string sql = null, object parameter = null, string tableName = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default)
            where T : new()
        {
            var models = ExecuteModels<T>(sql, parameter, tableName, 0, 1, commandType, commandBehavior);
            var model = models.FirstOrDefault();
            return model;
        }

        /// <summary>
        /// Executes SQL and returns the first single value of the result.
        /// </summary>
        /// <typeparam name="T">
        /// Value type of the result.
        /// </typeparam>
        /// <param name="sql">
        /// SQL to be executed. 
        /// </param>
        /// <param name="parameter">
        /// This parameter will be used to generate sql query parameters.
        /// This parameter is an object whose properties will be used to generate query parameters of the same name.
        /// </param>
        /// <param name="commandType">
        /// The type of sql command, if null, CommandType.Text will be used. For details, <see cref="CommandType"/>.
        /// </param>
        /// <returns>
        /// The first single value of the result
        /// </returns>
        public T ExecuteObject<T>(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            T result;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql, commandType, parameter))
                {
                    result = command.ExecuteObject<T>();
                }
            }
            return result;
        }

        /// <summary>
        /// Executes SQL and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">
        /// SQL to be executed. 
        /// </param>
        /// <param name="parameter">
        /// This parameter will be used to generate sql query parameters.
        /// This parameter is an object whose properties will be used to generate query parameters of the same name.
        /// </param>
        /// <param name="commandType">
        /// The type of sql to execute, if null, CommandType.Text will be used. For details, <see cref="CommandType"/>.
        /// </param>
        /// <returns>
        /// The number of rows affected
        /// </returns>
        public int ExecuteNone(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            int result;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql, commandType, parameter))
                {
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        /// <summary>
        /// Executes SQL and returns a <see cref="DataTable"/>.
        /// </summary>
        /// <param name="sql">
        /// SQL to be executed. 
        /// </param>
        /// <param name="parameter">
        /// This parameter will be used to generate sql query parameters.
        /// This parameter is an object whose properties will be used to generate query parameters of the same name.
        /// </param>
        /// <param name="commandType">
        /// The type of sql to execute, if null, CommandType.Text will be used. For details, <see cref="CommandType"/>.
        /// </param>
        /// <returns>
        /// DataTable of sql query result.
        /// </returns>
        public DataTable ExecuteTable(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            var dataTable = new DataTable();
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql, commandType, parameter))
                {
                    dataTable.Load(command.ExecuteReader());
                }
            }
            return dataTable;
        }


        /// <summary>
        /// Insert a row into data table using the automatic mapping logic.
        /// </summary>
        /// <typeparam name="T">
        /// Type of inserted model.
        /// T's type name will be used to generate INSERT SQL table name by default.
        /// T's properties names will be used to generate INSERT SQL column and query parameter names by default.
        /// </typeparam>
        /// <param name="model">
        /// Row model to be inserted, properties' value will be inserted into the columns with same name by default.
        /// </param>
        /// <param name="tableName">
        /// If the name of the data table is inconsistent with the type name of the model, you can specify the table name here.
        /// </param>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public int InsertModel<T>(T model, string tableName = null)
        {
            var modelType = typeof(T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            var columnParamDic = GetTypeParameterDic(modelType);
            var sql = _dialectBuilder.BuildInsertSql(tableName, columnParamDic);
            var rowCount = ExecuteNone(sql, model);
            return rowCount;
        }

        /// <summary>
        /// Update a row of data table using the automatic mapping logic.
        /// </summary>
        /// <typeparam name="T">
        /// Type of updated model.
        /// T's type name will be used to generate UPDATE SQL table name by default.
        /// T's properties names will be used to generate UPDATE SQL column and query parameter names by default.
        /// </typeparam>
        /// <param name="model">
        /// Row model to be updated, properties' value will be updated to the columns with same name by default.
        /// </param>
        /// <param name="condition">
        /// Condition is an object whose properties will be used to generate WHERE clause of UPDATE SQL.
        /// Setting the condition to null will generate an UPDATE SQL without WHERE clause.
        /// </param>
        /// <param name="tableName">
        /// If the name of the data table is inconsistent with the type name of the model, you can specify the table name here.
        /// </param>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public int UpdateModel<T>(T model, object condition, string tableName = null)
        {
            var modelType = typeof(T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            const string columnPrefix = "c_";
            const string conditionPrefix = "w_";
            IEnumerable<KeyValuePair<string, string>> columns = GetTypeParameterDic(modelType, columnPrefix);
            var conditionParamDic = GetObjectParameterDic(condition, conditionPrefix);
            var sql = _dialectBuilder.BuildUpdateSql(tableName, columns, conditionParamDic);

            int rowCount;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql))
                {
                    command.AddParameters(model, columnPrefix);
                    command.AddParameters(condition, conditionPrefix);
                    rowCount = command.ExecuteNonQuery();
                }
            }
            return rowCount;
        }


        /// <summary>
        /// Insert or update a row of data table using the automatic mapping logic.
        /// </summary>
        /// <typeparam name="T">
        /// Type of model.
        /// T's type name will be used to generate InsertOrUpdate SQL table name by default.
        /// T's properties names will be used to generate InsertOrUpdate SQL column and query parameter names by default.
        /// </typeparam>
        /// <param name="model">
        /// Row model to be operated, properties' value will be mapped to the columns with same name by default.
        /// </param>
        /// <param name="condition">
        /// Condition is an object whose properties will be used to generate WHERE clause of SQL.
        /// </param>
        /// <param name="tableName">
        /// If the name of the data table is inconsistent with the type name of the model, you can specify the table name here.
        /// </param>
        /// <returns>
        /// The number of rows affected.
        /// </returns>
        public int InsertOrUpdateModel<T>(T model, object condition, string tableName = null)
        {
            var modelType = typeof(T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            const string columnPrefix = "c_";
            const string conditionPrefix = "w_";
            var columnParams = GetTypeParameterDic(modelType, columnPrefix);
            var conditionParams = GetObjectParameterDic(condition, conditionPrefix);
            var sql = _dialectBuilder.BuildMergeSql(tableName, columnParams, conditionParams);

            int rowCount;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql))
                {
                    command.AddParameters(model, columnPrefix);
                    command.AddParameters(condition, conditionPrefix);
                    rowCount = command.ExecuteNonQuery();
                }
            }
            return rowCount;

        }


        /// <summary>
        /// Delete rows using the automatically generated SQL.
        /// </summary>
        /// <param name="tableName">
        /// Table name of automatically generated DELETE SQL.
        /// </param>
        /// <param name="condition">
        /// Condition is an object whose properties will be used to generate WHERE clause of DELETE SQL.
        /// Setting the condition to null will generate a DELETE SQL without WHERE clause.
        /// </param>
        /// <returns>是否删除成功。</returns>
        public int DeleteModel(string tableName, object condition)
        {
            var conditionParamDic = GetObjectParameterDic(condition);
            var sql = _dialectBuilder.BuildDeleteSql(tableName, conditionParamDic);
            var rowCount = ExecuteNone(sql, condition);
            return rowCount;
        }

        /// <summary>
        /// Builds a dictionary of column and parameter mapping based on the type of the model.
        /// </summary>
        private static Dictionary<string, string> GetTypeParameterDic(Type modelType, string namePrefix = "")
        {
            var properties = ReflectHelper.GetProperties(modelType);
            var columnParameterDic = properties.ToDictionary(property => property.Name, property => namePrefix + property.Name);
            return columnParameterDic;
        }


        /// <summary>
        /// Builds a dictionary of column and parameter mapping based on the condition object.
        /// </summary>
        private static Dictionary<string, string> GetObjectParameterDic(object condition, string namePrefix = "")
        {
            if (condition == null)
            {
                return null;
            }

            var result = GetTypeParameterDic(condition.GetType(), namePrefix);
            return result;
        }

        /// <summary>
        /// Builds a command object.
        /// </summary>
        private static IDbCommand BuildCommand(IDbConnection conn, string sqlText, CommandType commandType = CommandType.Text, object parameter = null)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            cmd.CommandType = commandType;
            cmd.AddParameters(parameter);
            
            return cmd;
        }

        private IDbConnection GetConnection()
        {
            return _connectionFactory.Invoke();
        }
    }
}
