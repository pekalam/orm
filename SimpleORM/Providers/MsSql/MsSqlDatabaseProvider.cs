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
        private MsSqlInsertBuilder _insertBuilder = new MsSqlInsertBuilder();
        private MsSqlUpdateBuilder _updateBuilder = new MsSqlUpdateBuilder();
        private MsSqlDeleteBuilder _deleteBuilder = new MsSqlDeleteBuilder();
        private MsSqlTableBuilder _tableBuilder = new MsSqlTableBuilder();

        public MsSqlDatabaseProvider(string connectionString, string masterConnectionString)
        {
            ConnectionString = connectionString;
            MasterConnectionString = masterConnectionString;
        }

        private void _ThrowIfNotConnected() {if(!Connected) throw new Exception();}

        private void _CreateNewConnection(string connectionString)
        {
            _dbConnection?.Dispose();
            _dbConnection = new SqlConnection(connectionString);
            _dbConnection.Open();
        }

        private void _ExecuteSQL(string sql, bool masterSchema = false)
        {
            if (!Connected)
            {
                _CreateNewConnection(!masterSchema ? ConnectionString : MasterConnectionString);
            }
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        private IDataReader _ExecuteReader(string sql)
        {
            if (!Connected)
                _CreateNewConnection(ConnectionString);
            var command = _dbConnection.CreateCommand();
            command.CommandText = sql;
            var reader = command.ExecuteReader();
            return reader;
        }

        public string ConnectionString { get; set; }

        public string MasterConnectionString { get; set; }

        public bool Connected
        {
            get => _dbConnection != null && _dbConnection.State == ConnectionState.Open;
        }

        public void InsertEntities(IReadOnlyList<EntityEntry> entries)
        {
            foreach (var entry in entries)
                _ExecuteSQL(_insertBuilder.With(entry).Build());
        }

        public void UpdateEntities(IReadOnlyList<EntityEntry> entries)
        {
            foreach (var entry in entries)
                _ExecuteSQL(_updateBuilder.With(entry).Build());
        }

        public void CreateTable(TableMetadata tableMetadata, Dictionary<Type, TableMetadata> typeToMetadata)
        {
            _ExecuteSQL(_tableBuilder.With(tableMetadata, typeToMetadata).Build());
        }

        public void DeleteEntities(IReadOnlyList<EntityEntry> entries)
        {
            foreach (var entry in entries)
                _ExecuteSQL(_deleteBuilder.With(entry).Build());
        }

        public void DropTable(TableMetadata tableMetadata)
        {
            _ExecuteSQL($"DROP TABLE [{tableMetadata.Schema}.{tableMetadata.Name}]");
        }

        public void DropDatabase(string name)
        {
            if (Connected)
            {
                Disconnect();
            }
            _ExecuteSQL($"alter database [{name}] set single_user with rollback immediate", true);
            _ExecuteSQL($"DROP DATABASE {name}");
            _dbConnection.Dispose();
            _dbConnection = null;
        }

        public void DropSchema(string name)
        {
            _ThrowIfNotConnected();
            _ExecuteSQL($"DROP SCHEMA {name}");
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
            var sql =
                $"SELECT * FROM [{tableMetadata.Schema}.{tableMetadata.Name}] WHERE {EntityFieldAttributeReader.ReadEntityPrimaryKeyName(tableMetadata.EntityType)}='{primaryKey}'";
            var reader = _ExecuteReader(sql);
            return reader;
        }

        public IDataReader FindWhere(string field, object value, TableMetadata tableMetadata)
        {
            string op;
            if(value.GetType() == typeof(string))
            {
                op = "LIKE";
            }else
            {
                op = "=";
            }
            var sql
                = $"SELECT * FROM [{tableMetadata.Schema}.{tableMetadata.Name}] WHERE {field} {op} '{value}'";
            var reader = _ExecuteReader(sql);
            return reader;
        }

        public void CreateDatabase(string name)
        {
            if (Connected)
            {
                return;
            }
            _ExecuteSQL($"CREATE DATABASE {name};", true);
            _dbConnection.Dispose();
            _dbConnection = null;
        }

        public void CreateSchema(string name)
        {
            if(!Connected || (Connected && _dbConnection.ConnectionString == MasterConnectionString))
                _CreateNewConnection(ConnectionString);
            _ExecuteSQL($"CREATE SCHEMA {name};");
        }

        public bool IsTableCreated(TableMetadata tableMetadata)
        {
            bool disconnect = !Connected;
            if(!Connected)
                _CreateNewConnection(ConnectionString);
            var sql =
                $"if OBJECT_ID('[{tableMetadata.Schema}.{tableMetadata.Name}]') is not null\r\n\tprint 1\r\nelse\r\n\tprint 0";

            
            
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            _dbConnection.InfoMessage += (sender, args) =>
            {
                if (args.Message == "1")
                    semaphore.Release();
            };
            var created = false;
            _ExecuteSQL(sql);
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

            var sql = $"if DB_ID('{name}') IS NOT NULL\r\n\tprint 1\r\nelse\r\n\tprint 0";
            
            SemaphoreSlim semaphore = new SemaphoreSlim(0);
            _dbConnection.InfoMessage += (sender, args) =>
            {
                if (args.Message == "1")
                    semaphore.Release();
            };
            var created = false;
            _ExecuteSQL(sql, true);
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
