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
        public void Creates_database()
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

        [Test]
        public void IsDatabaseCreated_if_not_created_returns_false()
        {
            var prov = new MsSqlDatabaseProvider(MsSqlDatabaseProviderUtils.GetConnectionString("xxx"),
                MsSqlDatabaseProviderUtils.masterConnectionString);
            var exists = prov.IsDatabaseCreated("xxx");
            prov.Disconnect();
            Assert.IsFalse(exists);
        }

        [Test]
        public void IsDatabaseCreated_if_created_returns_true()
        {
            MsSqlDatabaseProviderUtils.CreateDatabase("xxx");
            var prov = new MsSqlDatabaseProvider(MsSqlDatabaseProviderUtils.GetConnectionString("xxx"),
                MsSqlDatabaseProviderUtils.masterConnectionString);
            var exists = prov.IsDatabaseCreated("xxx");
            prov.Disconnect();
            MsSqlDatabaseProviderUtils.DropDatabase("xxx");
            Assert.IsTrue(exists);
        }
    }
}
