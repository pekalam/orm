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
    [Entity] //wymagany atrybut dla klasy reprezentujacej tabele
    public class Film
    {
        [PrimaryKey] // oznacza pole (właściwość) Id jako klucz główny
        public int Id { get; set; }
        [NotNull] // dodaje ograniczenie NOT NULL dla kolumny
        public string Tytul { get; set; }
        public double Ocena { get; set; }
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
        [NotNull]
        public string Miasto { get; set; }
        public string Ulica { get; set; }
        public string Wojewodztwo { get; set; }
    }

    [Entity]
    public class CzlonekKlubu
    {
        [PrimaryKey]
        public int Id { get; set; }
        [NotNull]
        public string Imie { get; set; }
        [NotNull]
        public string Nazwisko { get; set; }
        [NotNull]
        public DateTime DataUrodzenia { get; set; }
        /*Oznacza pole LokalizacjaId jako klucz obcy powiązany z kluczem
         głównym Id w tabeli Lokalizacja.*/
        [ForeignKey(target:"MiejsceZamieszkania", referenced:"Id")]
        [OnDelete("CASCADE")]
        [OnUpdate("CASCADE")]
        [NotNull]
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

    [Entity]
    public class Wynik //klasa przechowująca wynik zapytania
    {
        public int liczba { get; set; }
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

            //Dodawanie
            /*
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
                Nazwisko = "Nowak",
                DataUrodzenia = new DateTime(1998, 1, 1)
            };

            
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
            #region Potrzebne_dane
            /*
            var lokalizacja2 = new Lokalizacja()
            {
                Id = 2, Miasto = "Kraków", Ulica = "Chopina 2", Wojewodztwo = "Małopolskie"
            };
            var czlonekKlubu2 = new CzlonekKlubu()
            {
                Id = 2, Imie = "Mateusz", Nazwisko = "Kowalski", LokalizacjaId = 2, DataUrodzenia = DateTime.Today
            };
            var film1 = new Film()
            {
                Id = 1, Tytul = "Shrek", Ocena = 4.89f
            };
            var film2 = new Film()
            {
                Id = 2,
                Tytul = "Pluton",
                Ocena = 5.00f
            };
            var film3 = new Film()
            {
                Id = 3,
                Tytul = "Taxi 2",
                Ocena = 2.51f
            };
            var ulub1 = new UlubioneFilmy()
            {
                Id = 1, CzlonekKlubuId = 1, FilmId = 1
            };
            var ulub2 = new UlubioneFilmy()
            {
                Id = 2,
                CzlonekKlubuId = 1,
                FilmId = 3
            };
            var ulub3 = new UlubioneFilmy()
            {
                Id = 3,
                CzlonekKlubuId = 2,
                FilmId = 2
            };

            db.Lokalizacja.Add(lokalizacja2);
            db.CzlonekKlubu.Add(czlonekKlubu2);
            db.Filmy.Add(film1);
            db.Filmy.Add(film2);
            db.Filmy.Add(film3);
            db.UlubioneFilmy.Add(ulub1);
            db.UlubioneFilmy.Add(ulub2);
            db.UlubioneFilmy.Add(ulub3);
            db.SaveChanges();
            */
            #endregion

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
            }
            */

            //Wykonanie dowolnego SQL
            /*
            var sql =
                "select Imie, Nazwisko, Miasto, LokalizacjaId from [orm.CzlonekKlubu] inner join [orm.Lokalizacja] on [orm.CzlonekKlubu].Id = [orm.Lokalizacja].Id";
            var reader = db.RawSql(sql);
            while (reader.Read())
            {
                Console.WriteLine($"Imie: {reader.GetString(0)} Nazwisko: {reader.GetString(1)}" +
                                  $" Miasto: {reader.GetString(2)} Id lokalizacji: {reader.GetInt32(3)}");
            }

            reader.Close();

            var wynik = db.RawSql<Wynik>("SELECT COUNT(*) FROM [orm.CzlonekKlubu]");
            Console.WriteLine($"Ilość wierszy w tabeli orm.CzlonekKlubu: {wynik[0].liczba}");
            */

            db.Disconnect();
            Console.ReadKey();
        }
    }
}
