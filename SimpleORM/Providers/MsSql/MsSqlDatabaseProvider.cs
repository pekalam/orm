using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace SimpleORM.Providers.MsSql
{
    public abstract class DatabaseOptionsBuilder
    {
        protected string _databaseName;
        protected string _schemaName;

        public DatabaseOptionsBuilder WithDatabaseName(string name)
        {
            _databaseName = name;
            return this;
        }
        public DatabaseOptionsBuilder WithSchemaName(string name)
        {
            _schemaName = name;
            return this;
        }
        public abstract DatabaseOptions Build();
    }

    public class MsSqlDatabaseOptionsBuilder : DatabaseOptionsBuilder
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

        public override DatabaseOptions Build()
        {
            if (String.IsNullOrEmpty(_connectionString) || String.IsNullOrEmpty(_masterConnectionString))
            {
                throw new Exception();
            }

            return new DatabaseOptions()
            {
                DatabaseProvider = new MsSqlDatabaseProvider(_connectionString, _masterConnectionString),
                DatabaseName = _databaseName,
                SchemaName = _schemaName
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

        private void _CreateNewConnection(string connectionString)
        {
            if (_dbConnection != null)
            {
                _dbConnection.Dispose();
            }
            _dbConnection = new SqlConnection(connectionString);
            _dbConnection.Open();
        }

        public string ConnectionString { get; set; }

        public string MasterConnectionString { get; set; }

        public bool Connected
        {
            get => _dbConnection != null && _dbConnection.State == ConnectionState.Open;
        }

        public void InsertEntities(IReadOnlyList<EntityEntry> entries)
        {
            if (!Connected)
                _CreateNewConnection(ConnectionString);
            foreach (var entry in entries)
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = new MsSqlInsertBuilder(entry).Build();
                command.ExecuteNonQuery();
            }
        }

        public void UpdateEntities(IReadOnlyList<EntityEntry> entries)
        {
            if (!Connected)
                _CreateNewConnection(ConnectionString);
            foreach (var entry in entries)
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = new MsSqlUpdateBuilder(entry, entry.TableMetadata.Schema).Build();
                command.ExecuteNonQuery();
            }
        }

        public void CreateTable(TableMetadata tableMetadata, string schema)
        {
            if(!Connected)
                _CreateNewConnection(ConnectionString);
        
            var sql = new MsSqlTableBuilder(tableMetadata, schema).Build();

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = sql;

            sqlCommand.ExecuteNonQuery();
        }

        public void DeleteEntities(IReadOnlyList<EntityEntry> entries)
        {
            if (!Connected)
                _CreateNewConnection(ConnectionString);
            foreach (var entry in entries)
            {
                var command = _dbConnection.CreateCommand();
                command.CommandText = new MsSqlDeleteBuilder(entry).Build();
                command.ExecuteNonQuery();
            }
        }

        public void DropTable(TableMetadata tableMetadata)
        {
            if (!Connected)
                _CreateNewConnection(ConnectionString);

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"DROP TABLE [{tableMetadata.Schema}.{tableMetadata.Name}]";

            sqlCommand.ExecuteNonQuery();
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

        public IDataReader RawSql(string sql)
        {
            if(!Connected)
                _CreateNewConnection(ConnectionString);

            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            
            var reader = command.ExecuteReader();
            return reader;
        }

        public IDataReader Find(object primaryKey, TableMetadata tableMetadata)
        {
            if(!Connected)
                _CreateNewConnection(ConnectionString);

            var command = _dbConnection.CreateCommand();
            command.CommandText 
                = $"SELECT * FROM [{tableMetadata.Schema}.{tableMetadata.Name}] WHERE {EntityFieldAttributeReader.ReadEntityPrimaryKey(tableMetadata.EntityType)}='{primaryKey}'";

            var reader = command.ExecuteReader();
            return reader;
        }

        public void CreateDatabase(string name)
        {
            if (Connected)
            {
                return;
            }
            _CreateNewConnection(MasterConnectionString);

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"CREATE DATABASE {name};";
            
            sqlCommand.ExecuteNonQuery();
            _dbConnection.Dispose();
            _dbConnection = null;
        }

        public void CreateSchema(string name)
        {
            if(!Connected || (Connected && _dbConnection.ConnectionString == MasterConnectionString))
                _CreateNewConnection(ConnectionString);

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"CREATE SCHEMA {name};";

            sqlCommand.ExecuteNonQuery();
        }

        public bool IsTableCreated(TableMetadata tableMetadata, string schema)
        {
            bool disconnect = !Connected;
            if (!Connected)
            {
                _CreateNewConnection(ConnectionString);
            }

            SqlCommand sqlCommand = _dbConnection.CreateCommand();
            sqlCommand.CommandText = $"if OBJECT_ID('[{schema}.{tableMetadata.Name}]') is not null\r\n\tprint 1\r\nelse\r\n\tprint 0";

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
            var created = false;
            var result = sqlCommand.ExecuteNonQuery();
            if (semaphore.Wait(0))
            {
                created = true;
            }

            if (disconnect)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }

            return created;
        }

        public bool IsDatabaseCreated(string name)
        {
            if (Connected && _dbConnection.ConnectionString == ConnectionString)
            {
                return true;
            }
            _CreateNewConnection(MasterConnectionString);

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
            var created = false;
            var result = sqlCommand.ExecuteNonQuery();
            if (semaphore.Wait(0))
            {
                created = true;
            }
            _dbConnection.Dispose();
            _dbConnection = null;
            return created;
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
                return;
            }
            if (Connected)
            {
                _dbConnection.Dispose();
                _dbConnection = null;
            }
        }
    }
}
