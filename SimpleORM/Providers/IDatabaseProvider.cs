using System.Collections.Generic;

namespace SimpleORM.Providers
{
    /// <summary>
    /// Interfejs implementowany przez dostawcę bazy danych
    /// </summary>
    public interface IDatabaseProvider
    {
        string ConnectionString { get; }
        bool Connected { get; }
        void InsertEntities(IReadOnlyList<EntityEntry> entries);
        void UpdateEntities(IReadOnlyList<EntityEntry> entries);
        void CreateTable(TableMetadata tableMetadata);
        void DropDatabase(string name);
        void CreateSchema(string name);
        bool IsTableCreated(TableMetadata tableMetadata);
        bool IsDatabaseCreated(string name);
        bool IsSchemaCreated(string name);
        void Connect();
        void Disconnect();
    }
}