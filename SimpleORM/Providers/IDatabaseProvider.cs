using System.Collections.Generic;
using System.Data;

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
        void DeleteEntities(IReadOnlyList<EntityEntry> entries);
        void DropTable(TableMetadata tableMetadata);
        void CreateDatabase(string name);
        void DropDatabase(string name);
        void CreateSchema(string name);
        void DropSchema(string name);
        IDataReader RawSql(string sql);
        IDataReader Find(object primaryKey, TableMetadata tableMetadata);
        IDataReader FindWhere(string field, object value, TableMetadata tableMetadata);
        bool IsTableCreated(TableMetadata tableMetadata);
        bool IsDatabaseCreated(string name);
        bool IsSchemaCreated(string name);
        void Connect();
        void Disconnect();
    }
}