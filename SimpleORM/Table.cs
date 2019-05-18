using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy.Generators.Emitters;
using SimpleORM.Attributes;

namespace SimpleORM
{
    public class TableMetadata
    {
        public TableMetadata(string par1, 
            IReadOnlyDictionary<string, List<IEntityFieldAttribute>> par2,
            IReadOnlyDictionary<string, Type> par3)
        {
            Name = par1;
            EntityPropertyAttributes = par2;
            EntityPropertyNameToType = par3;
        }

        public string Name { get; }

        public IReadOnlyDictionary<string, List<IEntityFieldAttribute>> EntityPropertyAttributes { get; }

        public IReadOnlyDictionary<string, Type> EntityPropertyNameToType { get; }
    }

    public class Table<T> where T : class,new()
    {
        private Database _database;

        public Table(Database database, string name)
        {
            _database = database;
            var entityAttr = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(T));
            var entityTrackedFields = EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T));
            Metadata = new TableMetadata(name, entityAttr, entityTrackedFields);
        }

        public string TableName => Metadata.Name;

        public TableMetadata Metadata { get; }

        public EntityEntry Add(T entity)
        {
            return _database.StateManager.Add(entity);
        }

        public EntityEntry Update(T entity)
        {
            return _database.StateManager.Update(entity);
        }

        public EntityEntry Remove(T entity)
        {
            return _database.StateManager.Remove(entity);
        }


    }
}
