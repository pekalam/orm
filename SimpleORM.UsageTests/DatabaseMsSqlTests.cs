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
            [ManyToOne("Actors", "MovieId")]
            public List<MovieActors> Actors { get; } = new List<MovieActors>();
        }

        [Entity]
        public class Actor
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTime DateOfBirth { get; set; }
        }

        [Entity]
        public class MovieActors
        {
            [PrimaryKey]
            public int Id { get; set; }
            [ForeignKey("Movie", "Id", true)]
            public int MovieId { get; set; }
            public Movie Movie { get; set; }
            [ForeignKey("Actor")]
            public int ActorId { get; set; }
            public Actor Actor { get; set; }
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

            public Table<Actor> Actors { get; set; }
            public Table<Cinema> Cinema { get; set; }
            public Table<Movie> Movies { get; set; }
            public Table<MovieActors> MovieActors { get; set; }
            
            
            
            
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
            bool tablesCreated = provider.IsTableCreated(db.GetTableMetadataForEntity(typeof(Movie)));
            tablesCreated = provider.IsTableCreated(db.GetTableMetadataForEntity(typeof(Cinema)));
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
            var meta = db.GetTableMetadataForEntity(typeof(Actor));

            db.RawSql("ALTER TABLE [orm.MovieActors] DROP CONSTRAINT fk_MovieActors_ActorId").Close();
            db.DropTable(meta);
            bool tablesCreated = provider.IsTableCreated(meta);
            db.Disconnect();

            Assert.IsNull(db.Actors);
            Assert.IsFalse(tablesCreated);
        }

        [Test]
        public void Inserts_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var actor = new Actor() {DateOfBirth = DateTime.Today, Id = 1, Name = "Jan'"};
            db.Actors.Add(actor);
            db.SaveChanges();

            var reader = db.RawSql("SELECT * FROM [orm.Actors]");
            var i = 0;
            while (reader.Read())
            {
                i++;
                TestContext.WriteLine($"{reader[0]} {reader[1]} {reader[2]}");
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
            var updatedName = "";
            while (reader.Read())
            {
                i++;
                TestContext.WriteLine($"{reader[0]} {reader[1]}");
                updatedName = reader[1].ToString();
            }
            reader.Close();

            var cin = db.Cinema.Find(1);
            cin.Name = "1000'";
            db.Cinema.Update(cin);
            db.SaveChanges();
            var cinema1 = db.Cinema.Find(1);

            Assert.IsTrue(updatedName == "Multikino");
            Assert.IsTrue(cinema1.Name == "1000'");
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

        [Test]
        public void FindsDb_entity()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() { Id = 1, Name = "Helios" };
            db.Cinema.Add(cinema);
            db.SaveChanges();

            var ci = 
                (Cinema)db.Find(db.GetTableMetadataForEntity(cinema), 1);

            Assert.IsTrue(ci.Name == "Helios");
        }

        [Test]
        public void Finds_entity_one_to_one()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() {Id = 1, Name="Helios"};
            var movie2 = new Movie() { CinemaId = 1, Id = 1, Title = "Movie2" };

            db.Cinema.Add(cinema);
            db.SaveChanges();
            db.Movies.Add(movie2);
            db.SaveChanges();

            movie2 = db.Movies.Find(1);

            Assert.IsTrue(movie2.Cinema != null);
            Assert.IsTrue(movie2.Cinema.Name == "Helios");
        }

        [Test]
        public void Finds_entity_many_to_one()
        {
            var db = CreateDatabase();
            db.EnsureCreated();

            var cinema = new Cinema() { Id = 1, Name = "Helios" };
            var movie2 = new Movie() { CinemaId = 1, Id = 1, Title = "Movie2" };
            var actor1 = new Actor() {Id = 1, Name = "11"};
            var actor2 = new Actor() { Id = 2, Name = "22" };

            db.Cinema.Add(cinema);
            db.SaveChanges();
            db.Movies.Add(movie2);
            db.SaveChanges();

            db.Actors.Add(actor1);
            db.Actors.Add(actor2);
            db.SaveChanges();
            var mvAc1 = new MovieActors() {Id = 1, ActorId = 1, MovieId = 1};
            var mvAc2 = new MovieActors() {Id = 2, ActorId = 2, MovieId = 1 };
            db.MovieActors.Add(mvAc1);
            db.MovieActors.Add(mvAc2);
            db.SaveChanges();

            movie2 = db.Movies.Find(1);

            Assert.IsTrue(movie2.Cinema != null);
            Assert.IsTrue(movie2.Actors != null);
            Assert.IsTrue(movie2.Actors.Count == 2);
            Assert.IsTrue(movie2.Actors[0].Actor.Name == "11");
            Assert.IsTrue(movie2.Actors[1].Actor.Name == "22");
            Assert.IsTrue(movie2.Cinema.Name == "Helios");
        }
    }
}
