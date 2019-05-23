using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.Tests.MsSqlProviderTests
{
    [TestFixture()]
    class MsSqlSchemaBuilderTests
    {
        [Test]
        public void f()
        {
            var sqlBuilder = new MsSqlSchemaBuilder("sch");
            var sql = sqlBuilder.Build();
            TestContext.WriteLine(sql);
        }
    }
}
