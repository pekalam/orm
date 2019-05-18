using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Providers.InMemory;

namespace SimpleORM.UsageTests
{
    class Movie
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Title { get; set; }
    }

    class MovieDatabase : Database
    {
        public MovieDatabase(DatabaseOptions opt) : base(opt)
        {

        }

        public Table<Movie> Movies { get; set; }

    }
     

    [TestFixture()]
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
        public void SaveChanges_saves_entites()
        {
            var dbProv = CreateDatabase();
            var db = dbProv.Item1;
            var provider = dbProv.Item2;
            var entity1 = new Movie() {Id = 1, Title = "Title1"};
            var entity2 = new Movie() { Id = 2, Title = "Title2" };

            db.Movies.Add(entity1);
            db.Movies.Add(entity2);
            db.SaveChanges();

            Assert.IsTrue(provider.ItemsCount == 2);
        }
    }
}
