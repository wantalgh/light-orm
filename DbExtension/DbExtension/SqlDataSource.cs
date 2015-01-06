using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;

namespace WT.Data.DbExtension
{
    /// <summary>
    /// 数据库访问器(此访问器线程安全，可以单例化使用。)
    /// </summary>
    public class SqlDataSource
    {
        /// <summary>
        /// 通过数据库连接的配置初始化数据库访问器
        /// </summary>
        /// <param name="connConfigName"></param>
        /// <param name="autoSqlType">使用自动生成SQL脚本功能时，要生成的脚本类型。(设为null则使用默认设置)</param>
        public SqlDataSource(string connConfigName, AutoSqlTypeEnum? autoSqlType = null)
            : this(
                ConfigurationManager.ConnectionStrings[connConfigName].ConnectionString,
                ConfigurationManager.ConnectionStrings[connConfigName].ProviderName,
                autoSqlType)
        {
        }

        /// <summary>
        /// 通过连接字符串和数据提供者的名字初始化数据访问器
        /// </summary>
        /// <param name="connectStr">数据库连接字符串</param>
        /// <param name="providerName">
        /// 数据提供者的名字。可选项有：
        /// System.Data.SqlClient
        /// System.Data.OleDb
        /// System.Data.Odbc
        /// System.Data.OracleClient(暂时未完成)
        /// 注：参数为空时默认使用System.Data.SqlClient。
        /// </param>
        /// <param name="autoSqlType">使用自动生成SQL脚本功能时，要生成的脚本类型。(设为null则使用默认设置)</param>
        public SqlDataSource(string connectStr, string providerName, AutoSqlTypeEnum? autoSqlType = null)
        {
            if (string.IsNullOrEmpty(connectStr))
            {
                throw new ArgumentNullException("connectStr");
            }
            if (string.IsNullOrEmpty(providerName))
            {
                providerName = "System.Data.SqlClient";
            }

            _connectionString = connectStr;

            switch (providerName)
            {
                case null:
                case "":
                case "System.Data.SqlClient":
                    _clientType = DbClientTypeEnum.SqlClient;
                    break;
                case "System.Data.OleDb":
                    _clientType = DbClientTypeEnum.OleDb;
                    break;
                case "System.Data.Odbc":
                    _clientType = DbClientTypeEnum.Odbc;
                    break;
                case "System.Data.OracleClient":
                    _clientType = DbClientTypeEnum.OracleClient;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("providerName");
            }

            if (autoSqlType == null)
            {
                switch (_clientType)
                {
                    case DbClientTypeEnum.SqlClient:
                        _autoSqlType = AutoSqlTypeEnum.Tsql;
                        break;
                    case DbClientTypeEnum.OleDb:
                        _autoSqlType = AutoSqlTypeEnum.JetSql;
                        break;
                    case DbClientTypeEnum.Odbc:
                        _autoSqlType = AutoSqlTypeEnum.JetSql;
                        break;
                    case DbClientTypeEnum.OracleClient:
                        _autoSqlType = AutoSqlTypeEnum.PlSql;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _autoSqlType = autoSqlType.Value;
            }
        }

        /// <summary>
        /// 使用自动生成SQL脚本功能时，要生成的脚本类型
        /// </summary>
        private readonly AutoSqlTypeEnum _autoSqlType;

        /// <summary>
        /// 数据库连接字符器
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// 数据库访问客户端的类型
        /// </summary>
        private readonly DbClientTypeEnum _clientType;

        /// <summary>
        /// 建立一个数据库连接
        /// </summary>
        private IDbConnection GetConnection()
        {
            IDbConnection connection;

            switch (_clientType)
            {
                case DbClientTypeEnum.SqlClient:
                    connection = new SqlConnection(_connectionString);
                    break;
                case DbClientTypeEnum.OleDb:
                    connection = new OleDbConnection(_connectionString);
                    break;
                case DbClientTypeEnum.Odbc:
                    connection = new OdbcConnection(_connectionString);
                    break;
                case DbClientTypeEnum.OracleClient:
                    throw new NotImplementedException();
                    //break;
                default:
                    throw new InvalidOperationException();
            }

            return connection;
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
                var type = typeof (T);
                if (tableName == null)
                {
                    tableName = type.Name;                    
                }
                var columns = ReflectHelper.GetProperties(type).Select(p => p.Name);
                var keyColumnDic = GetKeyColumnDic(parameter);
                sql = SqlBuilderFactory.GetBuilder(_autoSqlType).BuildSelectSql(tableName, columns, keyColumnDic, skip, take);
            }

            IEnumerable<T> result;
            using (var connection = GetConnection())
            {
                connection.Open();
                var parameters = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(parameter);
                using (var command = BuildCommand(connection, sql, commandType, parameters))
                {
                    result = DbCommandExtension.ExecuteObjects<T>(command, commandBehavior);
                    result = result.ToArray();
                }
            }
            return result;
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
                var parameters = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(parameter);
                connection.Open();                
                using (var command = BuildCommand(connection, sql, commandType, parameters))
                {
                    result = DbCommandExtension.ExecuteObject<T>(command);
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
            var parameters = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(parameter);
            var result = ExecuteNone(sql, parameters, commandType);
            return result;
        }

        /// <summary>
        /// 执行一条不返回任何结果的SQL语句
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="parameters">SQL参数</param>
        /// <param name="commandType">SQL语句的类型</param>
        /// <returns>执行SQL影响的行数</returns>
        internal int ExecuteNone(string sql, IDataParameter[] parameters = null, CommandType commandType = CommandType.Text)
        {
            int result;
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = BuildCommand(connection, sql, commandType, parameters))
                {
                    result = command.ExecuteNonQuery();
                }
            }
            return result;
        }

        /// <summary>
        /// 执行SQL，获取数据集
        /// </summary>
        /// <param name="sql">要执行的SQL</param>
        /// <param name="parameter">SQL参数</param>
        /// <param name="commandType">SQL命令类型</param>
        /// <returns>返回的数据集</returns>
        public DataSet ExecuteDataSet(string sql, object parameter = null, CommandType commandType = CommandType.Text)
        {
            var dataSet = new DataSet();
            using (var connection = GetConnection())
            {
                connection.Open();
                var parameters = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(parameter);
                using (var command = BuildCommand(connection, sql, commandType, parameters))
                {
                    var dataAdapter = DataAdapterBuilderFactory.BuildDataAdapter(command, _clientType);
                    dataAdapter.Fill(dataSet);
                }
            }
            return dataSet;
        }

        /// <summary>
        /// 获取某个模型的原始属性名与数据列对应词典
        /// </summary>
        private static Dictionary<string, string> GetPropertyColumnDic(Type modelType, IEnumerable<KeyValuePair<string, string>> propertyColumnMap, string namePrefix = "")
        {
            var properties = ReflectHelper.GetProperties(modelType);
            var propertyColumnDic = properties.ToDictionary(property =>namePrefix + property.Name, property => property.Name);
            if (propertyColumnMap != null)
            {
                foreach (var propertyColumn in propertyColumnMap)
                {
                    propertyColumnDic[namePrefix + propertyColumn.Key] = propertyColumn.Value;
                }
            }

            return propertyColumnDic;
        }

        /// <summary>
        /// 自动在数据库中插入一个实体模型。(如果此方法的自动简单Insert操作不能满足需要，可以使用ExecuteNone方法执行手动编写的Insert语句)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="tableName">数据表名(如果为空则使用数据模型的类型名)</param>
        /// <param name="propertyColumnMap">实体属性与数据列的映射关系字典(如果为空则认为实体属性与数据列名完全对应)</param>
        /// <returns>操作结果。</returns>
        public bool InsertModel<T>(T model, string tableName = null, IEnumerable<KeyValuePair<string,string>> propertyColumnMap = null)
        {
            var modelType = typeof (T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            var propertyColumnDic = GetPropertyColumnDic(modelType, propertyColumnMap);
            var sql = SqlBuilderFactory.GetBuilder(_autoSqlType).BuildInsertSql(tableName, propertyColumnDic);
            var rowCount = ExecuteNone(sql, model);
            return rowCount > 0;
        }

        /// <summary>
        /// 获取某个ID的属性与数据列对应字典
        /// </summary>
        private static Dictionary<string, string> GetKeyColumnDic(object ids, string namePrefix = "")
        {
            if (ids == null)
            {
                return null;
            }
            var properties = ReflectHelper.GetProperties(ids.GetType());
            var result = properties.ToDictionary(p => namePrefix + p.Name, p => p.Name);
            return result;
        }

        /// <summary>
        /// 自动更新数据库的一个实体模型。(如果此方法的自动简单Update操作不能满足需要，可以使用ExecuteNone方法执行手动编写的Update语句)
        /// </summary>
        /// <typeparam name="T">实体类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="keys">数据的键</param>
        /// <param name="tableName">数据表名(如果为空则使用数据模型的类型名)</param>
        /// <param name="propertyColumnMap">实体属性与数据列的映射关系(如果为空则认为实体属性与数据列名完全对应)</param>
        /// <returns>是否更新成功。</returns>
        public bool UpdateModel<T>(T model, object keys = null, string tableName = null, IEnumerable<KeyValuePair<string, string>> propertyColumnMap = null)
        {
            var modelType = typeof(T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            const string paramPrefix = "p_";
            const string keyPrefix = "k_";
            IEnumerable<KeyValuePair<string, string>> propertyColumnDic = GetPropertyColumnDic(modelType, propertyColumnMap, paramPrefix);
            var keyColumnDic = GetKeyColumnDic(keys, keyPrefix);
            var sql = SqlBuilderFactory.GetBuilder(_autoSqlType).BuildUpdateSql(tableName, propertyColumnDic, keyColumnDic);
            var bodyParam = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(model, paramPrefix);
            var keyParam = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(keys, keyPrefix);
            var updateParam = bodyParam.Union(keyParam).ToArray();
            var rowCount = ExecuteNone(sql, updateParam);
            return rowCount > 0;
        }

        /// <summary>
        /// 如果不存在指定的实体就执行Insert操作，如果存在就Update。(如果此方法的自动简单InsertOrUpdate操作不能满足需要，可以使用ExecuteNone方法执行手动编写的InsertOrUpdate语句)
        /// </summary>
        /// <typeparam name="T">实体的类型</typeparam>
        /// <param name="model">实体</param>
        /// <param name="keys">查询实体的键</param>
        /// <param name="tableName">实体所在的表名</param>
        /// <param name="propertyColumnMap">实体属性与数据列名的对应</param>
        /// <returns>是否操作成功</returns>
        public bool InsertOrUpdateModel<T>(T model, object keys = null, string tableName = null, IEnumerable<KeyValuePair<string, string>> propertyColumnMap = null)
        {
            var modelType = typeof(T);
            if (tableName == null)
            {
                tableName = modelType.Name;
            }

            const string paramPrefix = "p_";
            const string keyPrefix = "k_";
            var propertyColumnDic = GetPropertyColumnDic(modelType, propertyColumnMap, paramPrefix);
            var keyColumnDic = GetKeyColumnDic(keys, keyPrefix);
            var sql = SqlBuilderFactory.GetBuilder(_autoSqlType).BuildMergeSql(tableName, propertyColumnDic, keyColumnDic);
            var bodyParam = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(model, paramPrefix);
            var keyParam = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(keys, keyPrefix);
            var updateParam = bodyParam.Union(keyParam).ToArray();
            var rowCount = ExecuteNone(sql, updateParam);
            return rowCount > 0;
        }

        /// <summary>
        /// 自动化删除数据库中的模型。(如果此方法的自动简单DELETE操作不能满足需要，可以使用ExecuteNone方法执行手动编写的DELETE语句)
        /// </summary>
        /// <param name="tableName">被删除模型所在的表名</param>
        /// <param name="keys">删除条件(只支持简单的相等AND查询条件，使用复杂条件删除模型请使用ExecuteNone方法)</param>
        /// <returns>是否删除成功。</returns>
        public bool DeleteModel(string tableName, object keys = null)
        {
            var keyColumnDic = GetKeyColumnDic(keys);
            var sql = SqlBuilderFactory.GetBuilder(_autoSqlType).BuildDeleteSql(tableName, keyColumnDic);
            var rowCount = ExecuteNone(sql, keys);
            return rowCount > 0;
        }

        /// <summary>
        /// 事务性的执行一批SQL
        /// </summary>
        /// <param name="sqls">SQL语句及其参数</param>
        /// <param name="isolationLevel">事务的锁定行为</param>
        /// <returns>SQL影响的总行数</returns>
        public int ExecuteTransaction(IEnumerable<KeyValuePair<string, object>> sqls, IsolationLevel isolationLevel = IsolationLevel.Serializable)
        {
            var result = 0;
            using (var connection = GetConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction(isolationLevel);
                try
                {
                    foreach (var sql in sqls)
                    {
                        var parameters = DbParameterBuilderFactory.GetBuilder(_clientType).BuildParameters(sql.Value);
                        using (var command = BuildCommand(connection, sql.Key, CommandType.Text, parameters))
                        {
                            result += command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    result = 0;
                }
            }
            return result;
        }


        /// <summary>
        /// 组建一个DbCommand对象
        /// </summary>
        private static IDbCommand BuildCommand(IDbConnection conn, string sqlText, CommandType commandType = CommandType.Text, params IDataParameter[] parameters)
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = sqlText;
            cmd.CommandType = commandType;
            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(parameter);
                }
            }
            return cmd;
        }
    }
}
