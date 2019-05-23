using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SimpleORM.Providers.MsSql
{
    public class MsSqlDatabaseOptionsBuilder
    {
        private string _connectionString;
        private string _masterConnectionString;

        public MsSqlDatabaseOptionsBuilder WithConnectionString(string str)
        {
            _connectionString = str;
            return this;
        }

        public MsSqlDatabaseOptionsBuilder WithMasterConnectionString(string str)
        {
            _masterConnectionString = str;
            return this;
        }

        public DatabaseOptions Build()
        {
            if (String.IsNullOrEmpty(_connectionString) || String.IsNullOrEmpty(_masterConnectionString))
            {
                throw new Exception();
            }

            return new DatabaseOptions()
            {
                DatabaseProvider = new MsSqlDatabaseProvider(_connectionString, _masterConnectionString)
            };
        }
    }

    public class MsSqlDatabaseProvider : IDatabaseProvider
    {
        private SqlConnection _dbConnection;

        public MsSqlDatabaseProvider(string connectionString, string masterConnectionString)
        {
            ConnectionString = connectionString;
            MasterConnectionString = masterConnectionString;
        }

        private void _ThrowIfNotConnected() {if(!Connected) throw new Exception();}

        public string ConnectionString { get; set; }

        public string MasterConnectionString { get; set; }

        public bool Connected
        {
            get => _dbConnection != null && _dbConnection.State == ConnectionState.Open;
        }

        public void InsertEntities(IReadOnlyList<EntityEntry> entries)
        {
            throw new NotImplementedException();
        }

        public void UpdateEntities(IReadOnlyList<EntityEntry> entries)
        {
            throw new NotImplementedException();
        }

        public void CreateTable(TableMetadata tableMetadata)
        {
            _ThrowIfNotConnected();
        
            var sql = new MsSqlTableBuilder(tableMetadata).Build();

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = _dbConnection;
            sqlCommand.CommandText = sql;

            var ra = sqlCommand.ExecuteNonQuery();
        }

        //TODO: uprosczenie
        public void DropDatabase(string name)
        {
            if (Connected)
            {
                Disconnect();
            }

            _dbConnection = new SqlConnection(MasterConnectionString);
            _dbConnection.Open();;

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"alter database [{name}] set single_user with rollback immediate";
            var x = sqlCommand.ExecuteNonQuery();

            sqlCommand.CommandText = $"DROP DATABASE {name}";
            var ra = sqlCommand.ExecuteNonQuery();

            _dbConnection.Dispose();
            _dbConnection = null;
        }

        public void DropSchema(string name)
        {
            _ThrowIfNotConnected();

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"DROP SCHEMA {name}";

            var ra = sqlCommand.ExecuteNonQuery();
        }

        public void CreateDatabase(string name)
        {
            if (Connected)
            {
                return;
            }
            _dbConnection = new SqlConnection(MasterConnectionString);
            _dbConnection.Open();

            var sql = new MsSqlDatabaseBuilder(name).Build();

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = sql;

            var ra = sqlCommand.ExecuteNonQuery();
            _dbConnection.Dispose();
            _dbConnection = null;
        }

        public void CreateSchema(string name)
        {
            _ThrowIfNotConnected();

            var sql = new MsSqlSchemaBuilder(name).Build();

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = sql;

            var ra = sqlCommand.ExecuteNonQuery();
        }

        public bool IsTableCreated(TableMetadata tableMetadata)
        {
            throw new NotImplementedException();
        }

        public bool IsDatabaseCreated(string name)
        {
            _ThrowIfNotConnected();

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"if DB_ID('{name}') IS NOT NULL\r\n\tprint 1\r\nelse\r\n\tprint 0";

            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            _dbConnection.InfoMessage += (sender, args) =>
            {
                if (args.Message == "1")
                    semaphore.Release();
                else
                {
                    return;
                }
            };
            var result = sqlCommand.ExecuteNonQuery();
            if(semaphore.Wait(0))
                return true;
            else
            {
                return false;
            }
        }

        public bool IsSchemaCreated(string name)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            if (Connected)
            {
                return;
            }
            if (_dbConnection == null)
            {
                if (ConnectionString == null)
                {
                    throw new Exception();
                }
                _dbConnection = new SqlConnection(ConnectionString);
            }
            _dbConnection.Open();
        }

        public void Disconnect()
        {
            if (_dbConnection == null)
            {
                throw new Exception();
            }
            if (Connected)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }
    }
}
