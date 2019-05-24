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
        [ForeignKey("Film", "Id")]
        public int FilmId { get; set; }
        public Film Film { get; set; }
        [ForeignKey("CzlonekKlubu", "Id")]
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
        public int LokalizacjaId { get; set; }
        /*Pole wiązane z rekordem w tabeli Lokalizacja z kluczem
        głównym równym LokalizacjaId podczas pobierania rekordu z tabeli CzlonekKlubu*/
        public Lokalizacja MiejsceZamieszkania { get; set; }
    }

    /*Reprezentuje bazę danych KlubFilmowy i zawiera deklaracje tabel z tej bazy*/
    public class KlubFilmowy : Database
    {
        public KlubFilmowy(DatabaseOptions options) : base(options) {}
        public Table<Lokalizacja> Lokalizacja { get; set; }
        public Table<CzlonekKlubu> CzlonekKlubu { get; set; }
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

            var lokalizacja1 = new Lokalizacja()
                {Id = 1, Miasto = "Rzeszów", Ulica = "Cieplińskiego 8", Wojewodztwo = "Podkarpackie"};
            var czlonekKlubu1 = new CzlonekKlubu() {Id = 1, Imie = "Jan", LokalizacjaId = 1, Nazwisko = "Nowak"};

            db.Lokalizacja.Add(lokalizacja1);
            db.CzlonekKlubu.Add(czlonekKlubu1);
            db.SaveChanges();



            db.Disconnect();
            Console.ReadKey();
        }
    }
}
