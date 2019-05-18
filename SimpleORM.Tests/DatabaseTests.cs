using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;

namespace SimpleORM.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        class Movie
        {
            [PrimaryKey]
            public int Id { get; set; }
            [ForeignKey]
            public string Title { get; set; }
        }

        class TestEntity{}

        class MovieDatabase : Database
        {
            public Table<Movie> Movies { get; set; }

        }

        [Test]
        public void Tables_are_inititalized()
        {
            var db = new MovieDatabase();

            Assert.IsNotNull(db.Movies);
        }

        [Test]
        public void Database_constructor_when_table_without_get_set_throws()
        {
            Assert.IsTrue(false);
        }

        [Test]
        public void GetTableMetadataForEntity_when_proper_entity_returns_metadata()
        {
            var db = new MovieDatabase();
            var entity = new Movie();

            db.StateManager.Add(entity);
            var tableMetadata = db.GetTableMetadataForEntity(entity);

            Assert.AreEqual(tableMetadata.Name, "Movies");
            Assert.AreEqual(tableMetadata.EntityPropertyAttributes.Count, 2);
            Assert.AreEqual(tableMetadata.EntityPropertyNameToType.Count, 2);
        }

        [Test]
        public void GetTableMetadataForEntity_when_untracked_entity_throws()
        {
            var db = new MovieDatabase();
            var entity = new TestEntity();

            var ex = Assert.Throws<Exception>(() => db.GetTableMetadataForEntity(entity));
            Assert.NotNull(ex);
        }
    }
}
