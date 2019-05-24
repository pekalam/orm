using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SimpleORM.Providers.InMemory
{
    public class InMemoryDatabaseProvider : IDatabaseProvider
    {
        private List<EntityEntry> _store = new List<EntityEntry>();

        public string ConnectionString { get; }
        public bool Connected { get; private set; } = false;

        public void InsertEntities(IReadOnlyList<EntityEntry> entries)
        {
            foreach (var entry in entries)
            {
                _store.Add(entry);
            }
        }

        public void UpdateEntities(IReadOnlyList<EntityEntry> entries)
        {
            return;
        }

        public void CreateTable(TableMetadata tableMetadata, string schema)
        {
            throw new System.NotImplementedException();
        }

        public void DeleteEntities(IReadOnlyList<EntityEntry> entries)
        {
            _store.RemoveAll(ent => entries.Contains(ent));
        }

        public void DropTable(TableMetadata tableMetadata)
        {
            throw new System.NotImplementedException();
        }

        public void CreateDatabase(string name)
        {
            throw new System.NotImplementedException();
        }

        public void DropDatabase(string name)
        {
            throw new System.NotImplementedException();
        }

        public void CreateSchema(string name)
        {
            throw new System.NotImplementedException();
        }

        public void DropSchema(string name)
        {
            throw new System.NotImplementedException();
        }

        public IDataReader RawSql(string sql)
        {
            throw new System.NotImplementedException();
        }

        public IDataReader Find(object primaryKey, TableMetadata tableMetadata)
        {
            throw new System.NotImplementedException();
        }

        public bool IsTableCreated(TableMetadata tableMetadata, string schema)
        {
            throw new System.NotImplementedException();
        }

        public bool IsDatabaseCreated(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool IsSchemaCreated(string name)
        {
            throw new System.NotImplementedException();
        }

        public void Connect()
        {
            Connected = true;
        }

        public void Disconnect()
        {
            Connected = false;
        }


        public int ItemsCount => _store.Count;
    }
}