using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using UDBA.Exceptions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UDBA
{
    public class DatabaseAccess
    {
        #region Private fields

        private readonly DataTable FactoriesTable = DbProviderFactories.GetFactoryClasses();

        #endregion

        #region Internals fields

        internal IDbConnection dbConnection;
        internal IDbTransaction dbTransaction;
        internal DbProviderFactory _Factory;

        #endregion

        #region Properties

        public DbConnection GetConnection
        {
            get
            {
                return (DbConnection)dbConnection;
            }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Returns the factory from the ConnectionStringSettings
        /// </summary>
        /// <param name="ConnectionStringSetting"></param>
        /// <returns></returns>
        private DbProviderFactory GetFactory(ConnectionStringSettings ConnectionStringSetting)
        {
            foreach (DataRow row in FactoriesTable.Rows)
            {
                if (String.Compare(Convert.ToString(row["InvariantName"]), ConnectionStringSetting.ProviderName, true) == 0)
                {
                    return DbProviderFactories.GetFactory(ConnectionStringSetting.ProviderName);
                }
            }
            return null;
        }

        /// <summary>
        /// Creates a DbCommand object
        /// </summary>
        /// <param name="sql">The command text.</param>
        /// <param name="parameters">Parameters list for the query.</param>
        /// <returns></returns>
        private IDbCommand CreateCommand(string sql, List<DbParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            IDbCommand cmd = dbConnection.CreateCommand();
            cmd.CommandText = sql;
            cmd.CommandType = commandType;
            if (dbTransaction != null)
            {
                cmd.Transaction = dbTransaction;
            }
            if ((parameters != null))
            {
                foreach (DbParameter Parameter in parameters)
                {
                    cmd.Parameters.Add(Parameter);
                }
            }

            return cmd;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Creates a DbParameter for the current database connection
        /// </summary>
        /// <param name="name">Parameter's name.</param>
        /// <param name="value">Parameter's value.</param>
        /// <returns></returns>
        public DbParameter CreateParameter(string name, object value)
        {
            DbParameter parameter = _Factory.CreateParameter();
            parameter.Value = value;
            parameter.ParameterName = name;
            return parameter;
        }

        /// <summary>
        /// Create a DB connection
        /// </summary>
        /// <param name="ConnectionStringSetting">Connection string to open the connection to the database.</param>
        /// <returns></returns>
        public void CreateDbConnection(ConnectionStringSettings ConnectionStringSetting)
        {
            DbProviderFactory Factory = GetFactory(ConnectionStringSetting);
            if ((Factory != null))
            {
                DbConnection Connection = Factory.CreateConnection();
                if ((Connection != null))
                {
                    Connection.ConnectionString = ConnectionStringSetting.ConnectionString;
                    dbConnection = Connection;
                    _Factory = Factory;
                    return;
                }
            }
            throw new ConnectionCreationException("Factory for ConnectionString not found !");
        }

        /// <summary>
        /// Open the database connection
        /// </summary>
        public void OpenDatabase()
        {
            try
            {
                dbConnection.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Open the database connection
        /// </summary>
        public void OpenDatabase(ConnectionStringSettings connectionStringSettings)
        {
            try
            {
                CreateDbConnection(connectionStringSettings);
                dbConnection.Open();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Close the database connection
        /// </summary>
        public void CloseDatabase()
        {
            try
            {
                dbConnection.Close();
            }
            catch (Exception)
            {

                throw;
            }
        }

        #endregion

        #region Transactions

        /// <summary>
        /// Start a transaction
        /// </summary>
        public void BeginTransaction()
        {
            dbTransaction = dbConnection.BeginTransaction();
        }

        /// <summary>
        /// Returns the current transaction
        /// </summary>
        /// <returns></returns>
        public IDbTransaction GetTransaction()
        {
            return dbTransaction;
        }

        /// <summary>
        /// Rollback a transaction
        /// </summary>
        public void RollbackTransaction()
        {
            if (dbTransaction != null)
            {
                dbTransaction.Rollback();
                dbTransaction.Dispose();
                dbTransaction = null;
            }
        }

        /// <summary>
        /// Commit a transaction
        /// </summary>
        public void CommitTransaction()
        {
            if (dbTransaction != null)
            {
                dbTransaction.Commit();
                dbTransaction.Dispose();
                dbTransaction = null;
            }
        }

        #endregion

        #region Execute methods

        /// <summary>
        /// Executes a Transact-SQL statement against the connection and returns the number of rows affected.
        /// </summary>
        /// <param name="sql">The command text.</param>
        /// <param name="parameters">Parameters list for the query.</param>
        /// <returns></returns>
        public int ExecuteNonQuery(string sql, List<DbParameter> parameters = null)
        {
            IDbCommand cmd = CreateCommand(sql, parameters);

            return cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <param name="sql">The command text.</param>
        /// <param name="parameters">Parameters list for the query.</param>
        /// <returns></returns>
        public T ExecuteScalar<T>(string sql, List<DbParameter> parameters = null)
        {
            IDbCommand cmd = CreateCommand(sql, parameters);

            return ExecuteScalar<T>(cmd);
        }

        /// <summary>
        /// Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="Command"></param>
        /// <returns></returns>
        private T ExecuteScalar<T>(IDbCommand Command)
        {
            if (Command == null) { throw new ArgumentNullException("Command is null"); }

            using (Command)
            {
                IDbConnection Connection = Command.Connection;
                if (Command.Connection == null) { throw new ArgumentNullException("Connection is null"); }

                if (Connection.State != ConnectionState.Open)
                {
                    Connection.Open();
                }
                object Value = Command.ExecuteScalar();
                if ((object.ReferenceEquals(Value, DBNull.Value)) || (Value == null))
                {
                    return default(T);
                }
                else if (object.ReferenceEquals(typeof(T), Value.GetType()) || typeof(T).IsAssignableFrom(Value.GetType()))
                {
                    return (T)Value;
                }
                else if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    return (T)Convert.ChangeType(Value, typeof(T).GetGenericArguments()[0]);
                }
                else
                {
                    return (T)Convert.ChangeType(Value, typeof(T));
                }
            }
        }
        #endregion

        #region Reader

        /// <summary>
        ///Returns a Reader from a query
        /// </summary>
        /// <param name="sql">The command text.</param>
        /// <param name="parameters">Parameters list for the query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns>A DbDataReader.</returns>
        /// <url>https://csharp-extension.com/en/method/1002758/dbconnection-executereader</url>
        public DbDataReader ExecuteReader(string sql, List<DbParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            using (IDbCommand command = CreateCommand(sql, parameters, commandType))
            {
                return (DbDataReader)command.ExecuteReader();
            }
        }

        #endregion

        #region DataTable

        /// <summary>
        /// Returns a DataTable from a query
        /// </summary>
        /// <param name="sql">The command text.</param>
        /// <param name="parameters">Parameters list for the query.</param>
        /// <param name="commandType">Type of the command.</param>
        /// <returns></returns>
        public DataTable GetDataTable(string sql, List<DbParameter> parameters = null, CommandType commandType = CommandType.Text)
        {
            DataTable dataTable = new DataTable();
            IDbCommand command = CreateCommand(sql, parameters, commandType);
            dataTable.Load(command.ExecuteReader());
            return dataTable;
        }

        #endregion
    }
}
