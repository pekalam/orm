using System.Collections.Generic;

namespace SimpleORM.Providers
{
    public interface IDatabaseProvider
    {
        string ConnectionString { get; }
        bool Connected { get; }
        void InsertEntities(IReadOnlyList<EntityEntry> entries);
        void UpdateEntities(IReadOnlyList<EntityEntry> entries);
        void CreateTable(TableMetadata tableMetadata);
        bool IsTableCreated(TableMetadata tableMetadata);
        void Connect();
        void Disconnect();
    }
}