﻿using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SimpleORM.Attributes;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.Tests.MsSqlProviderTests
{
    [TestFixture]
    class MsSqlBuildersTests
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
        public void InsertBuilder_returns_valid_sql()
        {
            var db = new Db();
            var ent = new Ent(){Id = 1, Str = "AS"};
            var entry = db.Ents.Add(ent);
            var insertBuilder = new MsSqlInsertBuilder(entry);

            var sql = insertBuilder.Build();

            TestContext.WriteLine(sql);
        }
    }
}