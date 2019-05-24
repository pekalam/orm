using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using NUnit.Framework;
using SimpleORM.Attributes;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.Tests.MsSqlProviderTests
{
    [TestFixture]
    class MsSqlDeleteBuilderTests
    {
        [Entity]
        class Ent
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Str { get; set; }
        }

        [Entity]
        class Ent2
        {
            public int Id { get; set; }
            public string Str { get; set; }
        }

        class Db : Database
        {
            public Table<Ent> Ents { get; set; }
            public Table<Ent2> Ents2 { get; set; }
        }

        [Test]
        public void DeleteSqlBuilder_returns()
        {
            var db = new Db();
            var ent = new Ent() { Id = 1, Str = "AS" };
            var entry = db.Ents.Add(ent);
            var builder = new MsSqlDeleteBuilder();

            var sql = builder.With(entry).Build();

            TestContext.WriteLine(sql);
            Assert.IsNotEmpty(sql);
        }
    }
}
