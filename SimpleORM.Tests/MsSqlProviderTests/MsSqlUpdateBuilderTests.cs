using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SimpleORM.Attributes;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.Tests.MsSqlProviderTests
{


    [TestFixture]
    class MsSqlUpdateBuilderTests
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
        public void Build_creates_sql()
        {
            var db = new Db();
            var ent = new Ent() {Id = 1, Str = "Test"};
            var entry = db.Ents.Add(ent);
            var ent2 = new Ent2() { Id = 2, Str = "Test" };
            var entry2 = db.Ents2.Add(ent2);
            var builder = new MsSqlUpdateBuilder();
            var builder2 = new MsSqlUpdateBuilder();
            ent2.Str = "c";
            entry2.State = EntityState.Unchanged;
            db.Ents2.Update(ent2);

            var sql = builder.With(entry).Build();
            var sql2 = builder2.With(entry2).Build();
            TestContext.WriteLine(sql);
            TestContext.WriteLine(sql2);

            Assert.IsNotEmpty(sql);
        }
    }
}
