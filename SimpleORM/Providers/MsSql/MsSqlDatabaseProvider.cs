using System;
using System.Collections.Generic;
using System.Data;

namespace SimpleORM.Providers.MsSql
{
    public class MsSqlDatabaseProvider : IDatabaseProvider
    {
        private IDbConnection _dbConnection;

        

        public string ConnectionString { get; set; }

        public bool Connected { get; }

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
            var sql = new MsSqlTableBuilder(tableMetadata).SQL;
        }

        public bool IsTableCreated(TableMetadata tableMetadata)
        {
            throw new NotImplementedException();
        }

        public void Connect()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }
    }
}
