using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.UsageTests.TestUtils
{
    public class MsSqlDatabaseProviderUtils
    {
        public static string masterConnectionString =
            @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static string GetConnectionString(string dbName)
        {
            return $"Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog={dbName};Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
        }
            

        public static void CreateDatabase(string dbName)
        {
            var provider = new MsSqlDatabaseProvider(GetConnectionString(dbName), masterConnectionString);
            provider.CreateDatabase(dbName);
            provider.Connect();
            provider.CreateSchema(dbName + "c");
            provider.Disconnect();
        }

        public static void DropDatabase(string dbName)
        {
            var provider = new MsSqlDatabaseProvider(GetConnectionString(dbName), masterConnectionString);
            provider.Connect();
            provider.DropSchema(dbName + "c");
            provider.DropDatabase(dbName);
        }
    }
}
