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
    public class DataClient
    {
        private readonly ISqlDialectBuilder _dialectBuilder;

        private readonly Func<IDbConnection> _connectionFactory;


        public DataClient(Func<IDbConnection> connectionFactory, ISqlDialectBuilder dialectBuilder = null)
        {
            _connectionFactory = connectionFactory;
            _dialectBuilder = dialectBuilder ?? new Tsql2005Builder();
        }

        /// <summary>
        /// 执行一条SQL，返回一批执行结果。(自动拼装SQL会有些许性能损耗，且只拼装简单的相等AND查询条件，如果对性能要求高或使用复杂查询条件，请手写要执行的SQL。)
        /// </summary>
        /// <typeparam name="T">结果集的元素类型</typeparam>
        /// <param name="sql">要执行的SQL，数据列名与模型属性名不一致时，使用AS语句调整。此参数为null时按模型表名与属性名自动拼装简单SELECT语句。</param>
        /// <param name="skip">要跳过的数据行数。(-1表示不采用此机制)</param>
        /// <param name="take">要取的数据行数</param>
        /// <param name="tableName">如果自动拼装SQL，SELECT的表名，如果此参数为null，则使用T类型名作为表名</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandBehavior">执行方式</param>
        /// <returns>结果的强类型可遍历集。</returns>
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
        /// 执行一条SQL，返回一批执行结果的第一项，如果。(自动拼装SQL会有些许性能损耗，且只拼装简单的相等AND查询条件，如果对性能要求高或使用复杂查询条件，请手写要执行的SQL。)
        /// </summary>
        /// <typeparam name="T">结果集的元素类型</typeparam>
        /// <param name="sql">要执行的SQL，数据列名与模型属性名不一致时，使用AS语句调整。此参数为null时按模型表名与属性名自动拼装简单SELECT语句。</param>
        /// <param name="tableName">如果自动拼装SQL，SELECT的表名，如果此参数为null，则使用T类型名作为表名</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="commandBehavior">执行方式</param>
        /// <returns>结果的强类型。</returns>
        public T ExecuteModel<T>(string sql = null, object parameter = null, string tableName = null, CommandType commandType = CommandType.Text, CommandBehavior commandBehavior = CommandBehavior.Default)
            where T : new()
        {
            var models = ExecuteModels<T>(sql, parameter, tableName, 0, 1, commandType, commandBehavior);
            var model = models.FirstOrDefault();
            return model;
        }

        /// <summary>
        /// 执行一条SQL，返回执行结果的第一列第一行
        /// </summary>
        /// <typeparam name="T">返回结果的类型，通常为int、string、DateTime、Guid等。</typeparam>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">SQL语句的类型</param>
        /// <returns>执行结果第一列第一行的强类型</returns>
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
        /// 执行一条不返回任何结果的SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">SQL语句的类型</param>
        /// <returns>执行SQL影响的行数</returns>
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
        /// 执行SQL，获取数据表
        /// </summary>
        /// <param name="sql">要执行的SQL</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">SQL命令类型</param>
        /// <returns>返回的数据集</returns>
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
        /// 自动在数据库中插入一个实体模型。(如果此方法的自动简单Insert操作不能满足需要，可以使用ExecuteNone方法执行手动编写的Insert语句)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="tableName">数据表名(如果为空则使用数据模型的类型名)</param>
        /// <returns>操作结果。</returns>
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
        /// 自动更新数据库的一个实体模型。(如果此方法的自动简单Update操作不能满足需要，可以使用ExecuteNone方法执行手动编写的Update语句)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="condition">数据的键</param>
        /// <param name="tableName">数据表名(如果为空则使用数据模型的类型名)</param>
        /// <returns>是否更新成功。</returns>
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
        /// 如果不存在指定的实体就执行Insert操作，如果存在就Update。(如果此方法的自动简单InsertOrUpdate操作不能满足需要，可以使用ExecuteNone方法执行手动编写的InsertOrUpdate语句)
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="condition">实体的键</param>
        /// <param name="tableName">实体所在的表名</param>
        /// <returns>是否操作成功</returns>
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
        /// 自动化删除数据库中的模型。(如果此方法的自动简单DELETE操作不能满足需要，可以使用ExecuteNone方法执行手动编写的DELETE语句)
        /// </summary>
        /// <param name="tableName">被删除模型所在的表名</param>
        /// <param name="condition">删除条件(只支持简单的相等AND查询条件，使用复杂条件删除模型请使用ExecuteNone方法)</param>
        /// <returns>是否删除成功。</returns>
        public int DeleteModel(string tableName, object condition)
        {
            var conditionParamDic = GetObjectParameterDic(condition);
            var sql = _dialectBuilder.BuildDeleteSql(tableName, conditionParamDic);
            var rowCount = ExecuteNone(sql, condition);
            return rowCount;
        }

        /// <summary>
        /// 获取某个模型的原始属性名与数据列对应词典
        /// </summary>
        private static Dictionary<string, string> GetTypeParameterDic(Type modelType, string namePrefix = "")
        {
            var properties = ReflectHelper.GetProperties(modelType);
            var columnParameterDic = properties.ToDictionary(property => property.Name, property => namePrefix + property.Name);
            return columnParameterDic;
        }


        /// <summary>
        /// 获取某列与其查询参数的对应字典
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
        /// 组建一个DbCommand对象
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
