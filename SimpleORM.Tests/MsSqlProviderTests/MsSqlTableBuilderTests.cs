using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.Tests.MsSqlProviderTests
{
    [TestFixture()]
    class MsSqlTableBuilderTests
    {
        class TestEntity
        {
            [PrimaryKey]
            public int Id { get; set; }

            public int Name { get; set; }
        }

        static TableMetadata stub1 = new TableMetadata("TestTable", new Dictionary<string, List<IEntityFieldAttribute>>()
        {
            ["Id"] = new List<IEntityFieldAttribute>() { new PrimaryKey() }
        }, new Dictionary<string, Type>()
        {
            ["Id"] = typeof(int),
            ["Name"] = typeof(string)
        });

        static TableMetadata stub2 = new TableMetadata("TestTable", new Dictionary<string, List<IEntityFieldAttribute>>()
        {
            ["Id"] = new List<IEntityFieldAttribute>() { new AutoIncrement(), new PrimaryKey() }
        }, new Dictionary<string, Type>()
        {
            ["Id"] = typeof(int),
            ["Name"] = typeof(string)
        });

        static TableMetadata GetStub(string name)
        {
            switch (name)
            {
                case "1":
                    return stub1;
                case "2":
                    return stub2;
            }

            return stub1;
        }

        [TestCase("1")]
        [TestCase("2")]
        public void TableBuilder_from_entity_returns_valid_sql(string name)
        {
            TableMetadata stubTableMetadata = GetStub(name);
            var sql = new MsSqlTableBuilder(stubTableMetadata).SQL;

            TestContext.WriteLine(sql);
        }
    }
}
