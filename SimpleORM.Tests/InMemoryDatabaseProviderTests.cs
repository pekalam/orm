using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Providers.InMemory;
using SimpleORM.Tests.TestUtil;

namespace SimpleORM.Tests
{
    class Movie
    {
        [PrimaryKey]
        public int Id { get; set; }
        [ForeignKey]
        public string Title { get; set; }
    }

    class MovieDatabase : Database
    {
        public MovieDatabase(DatabaseOptions opt) : base(opt)
        {
            
        }

        public Table<Movie> Movies { get; set; }

    }

    [TestFixture]
    public class InMemoryDatabaseProviderTests
    {
        private Tuple<MovieDatabase, InMemoryDatabaseProvider> CreateDatabase()
        {
            var options = new DatabaseOptions();
            var provider = new InMemoryDatabaseProvider();
            options.DatabaseProvider = provider;
            var db = new MovieDatabase(options);
            return new Tuple<MovieDatabase, InMemoryDatabaseProvider>(db, provider);
        }

        [Test]
        public void InsertEntities_inserts()
        {
            var provider = new InMemoryDatabaseProvider();
            var ent = new Movie();
            var entry = new EntityEntry(ent, TestObjectFactory.CreateTableMetadata(ent.GetType()));

            provider.InsertEntities(new List<EntityEntry>(){entry});

            Assert.IsTrue(provider.ItemsCount == 1);
        }
    }
}
