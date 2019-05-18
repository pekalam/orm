using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Providers;
using SimpleORM.Providers.InMemory;

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

    public class DatabaseOptionsBuilder
    {
    }

    public class Database : IDatabase
    {
        private IDatabaseDepedencies _databaseDepedencies;

        private IDatabaseProvider _provider;

        private Dictionary<Type, TableMetadata> _typeTableMetadatas = new Dictionary<Type, TableMetadata>();

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
            InitTables();
            _databaseDepedencies = InternalDepedencyProvider.DatabaseDepedencies;
            _databaseDepedencies.StateManager.Database = this;
            _provider = options == null ? InternalDepedencyProvider.DefaultDatabaseProvider : options.DatabaseProvider;

            _provider.Connect();
        }

        private void InitTables()
        {
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
                        
                        _typeTableMetadatas.Add(tableEntityType, tableMetadata);
                    }
                }
            }
        }

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

        public TableMetadata GetTableMetadataForEntity(object entity)
        {
            if(!_typeTableMetadatas.ContainsKey(entity.GetType()))
                throw new Exception();

            return _typeTableMetadatas[entity.GetType()];
        }

        public bool EnsureCreated()
        {
            bool created = true;
            foreach (var tableMetadata in _typeTableMetadatas.Values)
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
