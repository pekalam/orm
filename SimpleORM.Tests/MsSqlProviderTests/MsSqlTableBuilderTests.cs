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
    [Entity]
    class TestEntity
    {
        [PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public string Name { get; set; }
    }

    [TestFixture()]
    class MsSqlTableBuilderTests
    {
        [Test]
        public void TableBuilder_returns_valid_sql()
        {
            var builder = new MsSqlTableBuilder();
            TableMetadata tb = new TableMetadata("TestTable", 
                new Dictionary<string, List<IEntityFieldAttribute>>()
                {
                    ["Id"] = new List<IEntityFieldAttribute>()
                    {
                        new PrimaryKey()
                    },
                    ["Name"] = new List<IEntityFieldAttribute>()
                    {
                        new NotNull()
                    }
                }, 
                new Dictionary<string, Type>()
                {
                    ["Id"] = typeof(int), ["Name"] = typeof(string)
                }, typeof(TestEntity), "testSchema");

            builder.With(tb, new Dictionary<Type, TableMetadata>());
            var sql = builder.Build();

            TestContext.WriteLine(sql);
        }
    }
}
