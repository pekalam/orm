using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Providers;
using SimpleORM.Providers.MsSql;
using SimpleORM.UsageTests.TestUtils;

namespace SimpleORM.UsageTests
{
    

    [TestFixture()]
    public class DatabaseMsSqlTests
    {
        [Entity]
        public class Movie
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string Title { get; set; }

            [NotTracked]
            public string AdditionalField { get; set; }
            [ForeignKey("Cinema", "Id")]
            public int CinemaId { get; set; }
            public Cinema Cinema { get; set; }
        }

        [Entity]
        public class Cinema
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class MovieDatabase : Database
        {
            public MovieDatabase(DatabaseOptions opt) : base(opt){  }

            public Table<Cinema> Cinema { get; set; }
            public Table<Movie> Movies { get; set; }
            
        }

        private MovieDatabase CreateDatabase()
        {
            var connectionString =
                MsSqlDatabaseProviderUtils.GetConnectionString("test");
            var masterConnectionString =
                MsSqlDatabaseProviderUtils.masterConnectionString;
            var options = new MsSqlDatabaseOptionsBuilder()
                .WithConnectionString(connectionString)
                .WithMasterConnectionString(masterConnectionString)
                .WithDatabaseName("test")
                .Build();
            return new MovieDatabase(options);
        }

        [TearDown]
        public void TearDown()
        {
            MsSqlDatabaseProviderUtils.DropDatabase("test");
        }

        [Test]
        public void Creates_database()
        {
            var db = CreateDatabase();
            db.EnsureCreated();
            
            MsSqlDatabaseProvider provider = new MsSqlDatabaseProvider(MsSqlDatabaseProviderUtils.GetConnectionString("test"),
                MsSqlDatabaseProviderUtils.masterConnectionString);
            bool created = provider.IsDatabaseCreated("test");
            bool tablesCreated = provider.IsTableCreated(db.GetTableMetadataForEntity(typeof(Movie)), db.Schema);
            tablesCreated = provider.IsTableCreated(db.GetTableMetadataForEntity(typeof(Cinema)), db.Schema);
            db.Disconnect();

            Assert.IsTrue(created);
            Assert.IsTrue(tablesCreated);
        }

        [Test]
        public void Drops_table()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            MsSqlDatabaseProvider provider = new MsSqlDatabaseProvider(MsSqlDatabaseProviderUtils.GetConnectionString("test"),
                MsSqlDatabaseProviderUtils.masterConnectionString);
            var meta = db.GetTableMetadataForEntity(typeof(Movie));

            db.DropTable(meta);
            bool tablesCreated = provider.IsTableCreated(meta, db.Schema);
            db.Disconnect();

            Assert.IsNull(db.Movies);
            Assert.IsFalse(tablesCreated);
        }

        [Test]
        public void Inserts_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() {Id = 1, Name = "Helios"};
            db.Cinema.Add(cinema);
            db.SaveChanges();

            var reader = db.RawSql("SELECT * FROM [orm.Cinema]");
            var i = 0;
            while (reader.Read())
            {
                i++;
                TestContext.WriteLine($"{reader[0]} {reader[1]}");
            }

            Assert.IsTrue(i != 0);
        }

        [Test]
        public void Deletes_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() { Id = 1, Name = "Helios" };
            db.Cinema.Add(cinema);
            db.SaveChanges();

            db.Cinema.Remove(cinema);
            db.SaveChanges();

            var reader = db.RawSql("SELECT * FROM [orm.Cinema]");
            var i = 0;
            while (reader.Read())
            {
                i++;
                TestContext.WriteLine($"{reader[0]} {reader[1]}");
            }

            Assert.IsTrue(i == 0);
        }

        [Test]
        public void Updates_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() { Id = 1, Name = "Helios" };
            db.Cinema.Add(cinema);
            db.SaveChanges();

            cinema.Name = "Multikino";
            db.Cinema.Update(cinema);
            db.SaveChanges();

            var reader = db.RawSql("SELECT * FROM [orm.Cinema]");
            var i = 0;
            var upd = "";
            while (reader.Read())
            {
                i++;
                TestContext.WriteLine($"{reader[0]} {reader[1]}");
                upd = reader[1].ToString();
            }

            Assert.IsTrue(upd == "Multikino");
        }


        [Test]
        public void Finds_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() { Id = 1, Name = "Helios" };
            db.Cinema.Add(cinema);
            db.SaveChanges();

            cinema.Name = "Multikino";
            db.Cinema.Update(cinema);
            db.SaveChanges();

            var ci = db.Cinema.Find(1);

            Assert.IsTrue(ci.Name == "Multikino");
        }
    }
}
