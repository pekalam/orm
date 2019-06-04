using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleORM;
using SimpleORM.Attributes;
using SimpleORM.Providers.MsSql;

namespace Aplikacja
{
    [Entity]
    public class Film
    {
        [PrimaryKey] // oznacza pole (właściwość) Id jako klucz główny
        public int Id { get; set; }
        public string Tytul { get; set; }
    }

    [Entity]
    public class UlubioneFilmy
    {
        [PrimaryKey]
        public int Id { get; set; }
        [ForeignKey("Film", "Id")]
        public int FilmId { get; set; }
        public Film Film { get; set; }
        [ForeignKey("CzlonekKlubu", "Id", true)]
        public int CzlonekKlubuId { get; set; }
        public CzlonekKlubu CzlonekKlubu { get; set; }
    }

    [Entity]
    public class Lokalizacja
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Miasto { get; set; }
        public string Ulica { get; set; }
        public string Wojewodztwo { get; set; }
    }

    [Entity]
    public class CzlonekKlubu
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Imie { get; set; }
        public string Nazwisko { get; set; }
        /*Oznacza pole LokalizacjaId jako klucz obcy powiązany z kluczem
         głównym Id w tabeli Lokalizacja.*/
        [ForeignKey(target:"MiejsceZamieszkania", referenced:"Id")]
        [OnDelete("CASCADE")]
        [OnUpdate("CASCADE")]
        public int LokalizacjaId { get; set; }

        /*Pole wiązane z rekordem w tabeli Lokalizacja z kluczem
        głównym równym LokalizacjaId podczas pobierania rekordu z tabeli CzlonekKlubu*/
        public Lokalizacja MiejsceZamieszkania { get; set; }
        
        /*Lista posiadająca ten atrybut jest wypelniana tymi rekordami z tabeli UlubioneFilmy,
         ktorych klucze CzlonekKlubuId odpowiadają kluczowi CzlonekKlubu.Id*/
        [ManyToOne("UlubioneFilmy", "CzlonekKlubuId")]
        public List<UlubioneFilmy> UlubioneFilmy { get; } = new List<UlubioneFilmy>();
    }

    /*Reprezentuje bazę danych KlubFilmowy i zawiera deklaracje tabel z tej bazy*/
    public class KlubFilmowy : Database
    {
        public KlubFilmowy(DatabaseOptions options) : base(options) {}
        public Table<Lokalizacja> Lokalizacja { get; set; }
        public Table<CzlonekKlubu> CzlonekKlubu { get; set; }
        public Table<Film> Filmy { get; set; }
        public Table<UlubioneFilmy> UlubioneFilmy { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var options = new MsSqlDatabaseOptionsBuilder()
                .WithMasterConnectionString(
                    @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
                .WithConnectionString(
                    @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=KlubFilmowy;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False")
                .WithSchemaName("orm")
                .WithDatabaseName("KlubFilmowy")
                .Build();

            var db = new KlubFilmowy(options);
            db.EnsureCreated();

            var lokalizacja1 = new Lokalizacja()
            {
                Id = 1,
                Miasto = "Rzeszów",
                Ulica = "Cieplińskiego 8",
                Wojewodztwo = "Podkarpackie"
            };
            var czlonekKlubu1 = new CzlonekKlubu()
            {
                Id = 1,
                Imie = "Jan",
                LokalizacjaId = 1,
                Nazwisko = "Nowak"
            };

            //Dodawanie
            /*
            db.Lokalizacja.Add(lokalizacja1); //powiązanie obiektu z ORM
            db.CzlonekKlubu.Add(czlonekKlubu1);
            db.SaveChanges(); // zapisanie obiektów w bazie danych
            */

            //Usuwanie
            /*
            db.Lokalizacja.Remove(lokalizacja1);
            db.SaveChanges();
            db.UlubioneFilmy.Drop();
            db.CzlonekKlubu.Drop(); */


            //Wyszukiwanie
            /*var cz1 = db.CzlonekKlubu.Find(1);
            Console.WriteLine($"Imie: {cz1.Imie} Nazwisko: {cz1.Nazwisko}");
            Console.WriteLine("Ulubione filmy: ");
            foreach (var ulubioneFilmy in cz1.UlubioneFilmy)
            {
                Console.WriteLine($"{ulubioneFilmy.Film.Tytul}");
            }*/

            /*
            var cz2 = db.CzlonekKlubu.FindWhere("Imie", "Mateusz");
            Console.WriteLine($"Imie: {cz2.Imie} Nazwisko: {cz2.Nazwisko}");
            Console.WriteLine("Ulubione filmy: ");
            foreach (var ulubioneFilmy in cz2.UlubioneFilmy)
            {
                Console.WriteLine($"{ulubioneFilmy.Film.Tytul}");
            }*/


            //Wykonanie dowolnego SQL
            /*var sql =
                "select Imie, Nazwisko, Miasto, LokalizacjaId from [orm.CzlonekKlubu] inner join [orm.Lokalizacja] on [orm.CzlonekKlubu].Id = [orm.Lokalizacja].Id";
            var reader = db.RawSql(sql);
            while (reader.Read())
            {
                Console.WriteLine($"Imie: {reader.GetString(0)} Nazwisko: {reader.GetString(1)}" +
                                  $" Miasto: {reader.GetString(2)} Id lokalizacji: {reader.GetInt32(3)}");
            }
            reader.Close()*/



            db.Disconnect();
            Console.ReadKey();
        }
    }
}
