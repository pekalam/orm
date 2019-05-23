using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SimpleORM.Attributes;

namespace SimpleORM.Tests
{
    [TestFixture]
    public class TableTests
    {
        [Entity]
        class Movie
        {
            [PrimaryKey]
            public int Id { get; set; }
            [ForeignKey("", "")]
            public string Title { get; set; }
        }

        class MovieDatabase : Database
        {
            public Table<Movie> Movies { get; set; }

        }

        [Test]
        public void Tables_are_properly_initialized()
        {
            var db = new MovieDatabase();

            Assert.IsNotNull(db.Movies);
            Assert.AreEqual(db.Movies.TableName, "Movies");
        }
    }
}
