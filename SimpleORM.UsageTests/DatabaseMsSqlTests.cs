using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Providers;
using SimpleORM.Providers.MsSql;

namespace SimpleORM.UsageTests
{
    

    [TestFixture()]
    public class DatabaseMsSqlTests
    {
        public class Movie
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string Name { get; set; }

            [NotTracked]
            public string AdditionalField { get; set; }
        }

        public class MovieDatabase : Database
        {
            public MovieDatabase(DatabaseOptions opt) : base(opt){  }

            public Table<Movie> Movies { get; set; }
        }

        private MovieDatabase CreateDatabase()
        {
            var connectionString =
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SimpleORMTestDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var masterConnectionString =
                @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            var options = new MsSqlDatabaseOptionsBuilder()
                .WithConnectionString(connectionString)
                .WithMasterConnectionString(masterConnectionString)
                .Build();
            return new MovieDatabase(options);
        }

        [Test]
        public void Database_is_created()
        {
            var db = CreateDatabase();
        }
    }
}
