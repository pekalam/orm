using System.Collections.Generic;

namespace SimpleORM.Providers.InMemory
{
    public class InMemoryDatabaseProvider : IDatabaseProvider
    {
        private List<EntityEntry> _store = new List<EntityEntry>();

        private bool _connected = false;

        public string ConnectionString { get; }
        public bool Connected
        {
            get => _connected;
        }

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

        public void CreateTable(TableMetadata tableMetadata)
        {
            return;
        }

        public void DropDatabase(string name)
        {
            throw new System.NotImplementedException();
        }

        public void CreateSchema(string name)
        {
            throw new System.NotImplementedException();
        }

        public void CreateSchema(string dbName, string name)
        {
            throw new System.NotImplementedException();
        }

        public bool IsTableCreated(TableMetadata tableMetadata)
        {
            return true;
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
            _connected = true;
        }

        public void Disconnect()
        {
            _connected = false;
        }


        public int ItemsCount => _store.Count;
    }
}