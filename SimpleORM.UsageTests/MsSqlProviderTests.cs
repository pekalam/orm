using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.UsageTests.TestUtils;
using System.Data.SqlClient;
using System.Threading;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.UsageTests
{
    

    [TestFixture()]
    public class MsSqlProviderTests
    {
        [Test]
        public void f()
        {
            MsSqlDatabaseProviderUtils.CreateDatabase("teeest");
            Thread.Sleep(2000);
            MsSqlDatabaseProviderUtils.DropDatabase("teeest");
        }

        [Test]
        public void Checks_is_database_created()
        {
            MsSqlDatabaseProviderUtils.CreateDatabase("testowa");
            var prov = new MsSqlDatabaseProvider(MsSqlDatabaseProviderUtils.GetConnectionString("testowa"),
                MsSqlDatabaseProviderUtils.masterConnectionString);
            prov.Connect();
            var exists = prov.IsDatabaseCreated("testowa");
            prov.Disconnect();
            MsSqlDatabaseProviderUtils.DropDatabase("testowa");

            Assert.IsTrue(exists);
        }
    }
}
