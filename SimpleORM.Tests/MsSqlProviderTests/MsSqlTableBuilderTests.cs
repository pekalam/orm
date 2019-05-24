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
        [Entity]
        class TestEntity
        {
            [PrimaryKey]
            public int Id { get; set; }

            public int Name { get; set; }

            [ForeignKey("FkRef")]
            [OnDelete("CASCADE")]
            public int Fk { get; set; }
            public X FkRef { get; set; }

            [ForeignKey("FkRef2")]
            [OnDelete("CASCADE")]
            public int Fk2 { get; set; }
            public X FkRef2 { get; set; }
        }

        [Entity]
        class X
        {
            [PrimaryKey]
            public int Id { get; set; }
        }

        static TableMetadata stub1 = new TableMetadata("TestTable", new Dictionary<string, List<IEntityFieldAttribute>>()
        {
            ["Id"] = new List<IEntityFieldAttribute>() { new PrimaryKey() }
        }, new Dictionary<string, Type>()
        {
            ["Id"] = typeof(int),
            ["Name"] = typeof(string)
        }, typeof(TestEntity), "orm");

        static TableMetadata stub2 = new TableMetadata("TestTable", new Dictionary<string, List<IEntityFieldAttribute>>()
        {
            ["Id"] = new List<IEntityFieldAttribute>() { new AutoIncrement(), new PrimaryKey() }
        }, new Dictionary<string, Type>()
        {
            ["Id"] = typeof(int),
            ["Name"] = typeof(string)
        }, typeof(TestEntity), "orm");

        static TableMetadata stub3 = new TableMetadata("TestTable", new Dictionary<string, List<IEntityFieldAttribute>>()
        {
            ["Id"] = new List<IEntityFieldAttribute>() { new AutoIncrement(), new PrimaryKey() },
            ["Fk"] = new List<IEntityFieldAttribute>() { new ForeignKey("FkRef"), new OnDelete("CASCADE"), new OnUpdate("CASCADE") },
            ["Fk2"] = new List<IEntityFieldAttribute>() { new ForeignKey("FkRef2"), new OnDelete("CASCADE"), new OnUpdate("CASCADE") }
        }, new Dictionary<string, Type>()
        {
            ["Id"] = typeof(int),
            ["Fk"] = typeof(int),
            ["Fk2"] = typeof(int),
            ["Name"] = typeof(int)
        }, typeof(TestEntity), "orm");

        static TableMetadata GetStub(string name)
        {
            switch (name)
            {
                case "1":
                    return stub1;
                case "2":
                    return stub2;
                case "3":
                    return stub3;
            }

            return stub1;
        }

        [TestCase("1")]
        [TestCase("2")]
        [TestCase("3")]
        public void TableBuilder_from_entity_returns_valid_sql(string name)
        {
            TableMetadata stubTableMetadata = GetStub(name);
            var sql = new MsSqlTableBuilder().With(stubTableMetadata).Build();

            TestContext.WriteLine(sql);
        }
    }
}
