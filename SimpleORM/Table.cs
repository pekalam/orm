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
            IReadOnlyDictionary<string, Type> par3, Type entityType)
        {
            Name = par1;
            EntityPropertyAttributes = par2;
            EntityPropertyNameToType = par3;
            EntityType = entityType;
        }

        public string Name { get; }

        public Type EntityType { get; }

        public IReadOnlyDictionary<string, List<IEntityFieldAttribute>> EntityPropertyAttributes { get; }

        public IReadOnlyDictionary<string, Type> EntityPropertyNameToType { get; }
    }

    /// <summary>
    /// Reprezentuje tabelę w bazie danych. Podstawowa klasa SimpleORM
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Table<T> where T : class,new()
    {
        /// <summary>
        /// Powiązany z tabelą obiekt Database
        /// </summary>
        private Database _database;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"> Nazwa tabeli </param>
        public Table(Database database, string name)
        {
            _database = database;
            var entityAttr = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(T));
            var entityTrackedFields = EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T));
            Metadata = new TableMetadata(name, entityAttr, entityTrackedFields, typeof(T));
        }

        public string TableName => Metadata.Name;

        public TableMetadata Metadata { get; }

        /// <summary>
        /// Wiąże encję z ORM
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
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
