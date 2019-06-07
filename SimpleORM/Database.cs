using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Providers;
using SimpleORM.Providers.MsSql;

namespace SimpleORM
{
    public class DatabaseOptions
    {
        public IDatabaseProvider DatabaseProvider { get; set; }
        public string DatabaseName { get; set; }
        public string SchemaName { get; set; }
    }

    public interface IDatabase
    {
        TableMetadata GetTableMetadataForEntity(object entity);
        IStateManager StateManager { get; }
        IDatabaseProvider DatabaseProvider { get; }
        object Find(TableMetadata tableMetadata, object primaryKey);
        object FindWhere(TableMetadata tableMetadata, string field, object value);
        object[] FindAll(TableMetadata tableMetadata, object primaryKeys);
        object[] FindAllWhere(TableMetadata tableMetadata, string field, object values);
        TableMetadata GetTableMetadataForEntity(Type entity);
        string Schema { get; }
        bool DropTable(TableMetadata tableMetadata);
    }

    /// <summary>
    /// Baza danych zarządzana przez ORM. Podstawowa klasa SimpleORM
    /// </summary>
    public class Database : IDatabase
    {
        private IDatabaseDepedencies _databaseDepedencies;

        private string _databaseName;
        private string _schemaName;

        /// <summary>
        /// Pary typ przechowywany w tabeli - metadane tabeli
        /// </summary>
        private Dictionary<Type, TableMetadata> _typeToTableMetadata = new Dictionary<Type, TableMetadata>();

        /// <summary>
        /// Tylko do celów testowych - brak domyślnego providera
        /// </summary>
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
            _databaseName = (options == null || string.IsNullOrEmpty(options.DatabaseName)) ? GetType().Name.ToString() : options.DatabaseName;
            _schemaName = (options == null || string.IsNullOrEmpty(options.SchemaName)) ? "orm" : options.SchemaName;
            InitTables();
            _databaseDepedencies = InternalDepedencyProvider.DatabaseDepedencies;
            _databaseDepedencies.StateManager.Database = this;
            DatabaseProvider = options == null ? null : options.DatabaseProvider;
        }

        /// <summary>
        /// Inicjalizacja tabel przypisanych przez użytkownika do bazy
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

        public string Schema => _schemaName;
        public string Name => _databaseName;

        /// <summary>
        /// Rozlaczenie polaczenia z baza danych
        /// </summary>
        public void Disconnect()
        {
            DatabaseProvider.Disconnect();
        }

        /// <summary>
        /// Wykonanie dowolnego zapytania przy pomocy SQL
        /// </summary>
        /// <param name="sql">Wyrazenie SQL</param>
        /// <returns></returns>
        public IDataReader RawSql(string sql)
        {
            var reader = DatabaseProvider.RawSql(sql);
            return reader;
        }

        public List<T> RawSql<T>(string sql) where T:class,new()
        {
            var reader = DatabaseProvider.RawSql(sql);
            var res = reader.MapResults<T>(EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T)));
            reader.Close();
            return res;
        }

        public object Find(TableMetadata tableMetadata, object primaryKey)
        {
            var table = GetType().GetProperty(tableMetadata.Name).GetValue(this);
            return table.GetType().GetMethod("Find").Invoke(table,new object[]{primaryKey});
        }

        public object[] FindAll(TableMetadata tableMetadata, object primaryKey)
        {
            var table = GetType().GetProperty(tableMetadata.Name).GetValue(this);
            return (object[]) table.GetType().GetMethod("FindAll").Invoke(table, new object[] { primaryKey });
        }

        public object FindWhere(TableMetadata tableMetadata, string field, object value)
        {
            var table = GetType().GetProperty(tableMetadata.Name).GetValue(this);
            return table.GetType().GetMethod("FindWhere").Invoke(table, new object[] { field, value });
        }

        public object[] FindAllWhere(TableMetadata tableMetadata, string field, object value)
        {
            var table = GetType().GetProperty(tableMetadata.Name).GetValue(this);
            return (object[]) table.GetType().GetMethod("FindAllWhere").Invoke(table, new object[] { field, value});
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
                DatabaseProvider.InsertEntities(toInsert);

            var toUpdate = entriesToSave
                .Select(e => e).Where(e => e.State == EntityState.Modified)
                .ToList();
            if(toUpdate.Count > 0)
                DatabaseProvider.UpdateEntities(toUpdate);


            var toDelete = entriesToSave
                .Select(e => e).Where(e => e.State == EntityState.Deleted)
                .ToList();
            if (toDelete.Count > 0)
                DatabaseProvider.DeleteEntities(toDelete);

            StateManager.SaveChanges();
            return entriesToSave.Count;
        }

        /// <summary>
        /// Zwraca metadane tabeli dla podanej encji
        /// </summary>
        /// <param name="entity"></param>
        /// <returns> Metadane tabeli powiazanej z encją </returns>
        public TableMetadata GetTableMetadataForEntity(object entity) => GetTableMetadataForEntity(entity.GetType());
        public TableMetadata GetTableMetadataForEntity(Type entityType)
        {
            if (!_typeToTableMetadata.ContainsKey(entityType))
                throw new Exception();

            return _typeToTableMetadata[entityType];
        }

        /// <summary>
        /// Tworzy bazę danych, schemat i tabele jeśli nie zostały one wcześniej utworzone
        /// </summary>
        /// <returns> Jeżeli schemat lub tabele zostały utworzone zwraca true. W przeciwnym wypadku false. </returns>
        public bool EnsureCreated()
        {
            bool created = true;
            if (!DatabaseProvider.IsDatabaseCreated(_databaseName))
            {
                DatabaseProvider.CreateDatabase(_databaseName);
                DatabaseProvider.CreateSchema(_schemaName);
                created = false;
            }
            foreach (var tableMetadata in _typeToTableMetadata.Values)
            {
                if (!DatabaseProvider.IsTableCreated(tableMetadata))
                {
                    created = false;
                    DatabaseProvider.CreateTable(tableMetadata, _typeToTableMetadata);
                }
            }
            DatabaseProvider.Disconnect();
            return !created;
        }

        /// <summary>
        /// Usuwa podaną tabele
        /// </summary>
        /// <param name="tableMetadata">Metadane usuwanej tabeli</param>
        /// <returns>Jesli tabela zostala usunieta zwraca true</returns>
        public bool DropTable(TableMetadata tableMetadata)
        {
            if (_typeToTableMetadata.Values.Contains(tableMetadata))
            {
                DatabaseProvider.DropTable(tableMetadata);
                StateManager.DropEntriesFromTable(tableMetadata);

                GetType().GetProperty(tableMetadata.Name).SetValue(this,null);
                _typeToTableMetadata.Remove(tableMetadata.EntityType);
                return true;
            }
            else
            {
                return false;
            }
        }

        public IDatabaseProvider DatabaseProvider { get; private set; }

        public IStateManager StateManager => _databaseDepedencies.StateManager;
    }
}
