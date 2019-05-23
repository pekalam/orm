using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Providers;

namespace SimpleORM
{

    public interface IDatabase
    {
        TableMetadata GetTableMetadataForEntity(object entity);
    }

    public class DatabaseOptions
    {
        public IDatabaseProvider DatabaseProvider { get; set; }
    }

    /// <summary>
    /// Baza danych zarządzana przez ORM. Podstawowa klasa SimpleORM
    /// </summary>
    public class Database : IDatabase
    {
        private IDatabaseDepedencies _databaseDepedencies;

        private IDatabaseProvider _provider;

        private string _databaseName;

        /// <summary>
        /// Pary typ POCO przechowywany w tabeli - metadane tabeli
        /// </summary>
        private Dictionary<Type, TableMetadata> _typeToTableMetadata = new Dictionary<Type, TableMetadata>();

        public Database()
        {
            _Init(null);
        }

        public Database(DatabaseOptions options)
        {
            _Init(options);
        }

        private void _Init(DatabaseOptions options)
        {
            _databaseName = GetType().Name.ToString();
            InitTables();
            _databaseDepedencies = InternalDepedencyProvider.DatabaseDepedencies;
            _databaseDepedencies.StateManager.Database = this;
            _provider = options == null ? InternalDepedencyProvider.DefaultDatabaseProvider : options.DatabaseProvider;

            _provider.Connect();
        }

        /// <summary>
        /// Inicjalizacja podanych tabel przypisanych przez użytkownika do bazy
        /// </summary>
        private void InitTables()
        {
            int tables = 0;
            Type thisType = this.GetType();
            foreach (var propertyInfo in thisType.GetProperties())
            {
                if (propertyInfo.PropertyType.IsGenericType)
                {
                    if (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Table<>))
                    {
                        var tableType = propertyInfo.PropertyType.GetGenericTypeDefinition().MakeGenericType(
                            propertyInfo.PropertyType.GenericTypeArguments);
                        var tableEntityType = propertyInfo.PropertyType.GenericTypeArguments[0];
                        var tableInstance = Activator.CreateInstance(tableType,
                            BindingFlags.Public | BindingFlags.Instance,
                            null, new object[] {this, propertyInfo.Name}, null);
                        propertyInfo.SetValue(this, tableInstance);
                        var tableMetadata = (TableMetadata)tableInstance.GetType().GetProperty("Metadata").GetValue(tableInstance);
                        
                        _typeToTableMetadata.Add(tableEntityType, tableMetadata);

                        tables++;
                    }
                }
            }

            if (tables == 0)
            {
                throw new Exception("Baza danych nie zawiera tabel");
            }
        }

        /// <summary>
        /// Zapisuje zmiany w bazie danych
        /// </summary>
        /// <returns> Ilość zapisanych obiektów </returns>
        public int SaveChanges()
        {
            var entriesToSave = StateManager.GetEntriesToSave();
            if (entriesToSave.Count == 0)
                return 0;

            var toInsert = entriesToSave
                .Select(e => e).Where(e => e.State == EntityState.Added)
                .ToList();
            if(toInsert.Count > 0)
                _provider.InsertEntities(toInsert);

            var toUpdate = entriesToSave
                .Select(e => e).Where(e => e.State == EntityState.Modified)
                .ToList();
            if(toUpdate.Count > 0)
                _provider.UpdateEntities(toUpdate);

            StateManager.SaveChanges();
            return entriesToSave.Count;
        }

        /// <summary>
        /// Zwraca metadane tabeli dla podanej encji (POCO)
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> Metadane tabeli powiazanej z encją </returns>
        public TableMetadata GetTableMetadataForEntity(object entity)
        {
            if(!_typeToTableMetadata.ContainsKey(entity.GetType()))
                throw new Exception();

            return _typeToTableMetadata[entity.GetType()];
        }

        /// <summary>
        /// Tworzy bazę danych, schemat i tabele jeśli nie zostały one wcześniej utworzone
        /// </summary>
        /// <returns> Czy schemat lub tabele zostały utworzone </returns>
        public bool EnsureCreated()
        {
            bool created = true;

            foreach (var tableMetadata in _typeToTableMetadata.Values)
            {
                if (!_provider.IsTableCreated(tableMetadata))
                {
                    created = false;
                    _provider.CreateTable(tableMetadata);
                }
            }

            return !created;
        }

        public IStateManager StateManager => _databaseDepedencies.StateManager;
    }
}
