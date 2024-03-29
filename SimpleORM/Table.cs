﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Attributes;

namespace SimpleORM
{
    /// <summary>
    /// Pzechowuje metadane tabeli
    /// </summary>
    public class TableMetadata
    {
        public TableMetadata(string par1, 
            IReadOnlyDictionary<string, List<IEntityFieldAttribute>> par2,
            IReadOnlyDictionary<string, Type> par3, Type entityType, string schema)
        {
            Name = par1;
            EntityPropertyAttributes = par2;
            EntityPropertyNameToType = par3;
            EntityType = entityType;
            Schema = schema;
        }

        /// <summary>
        /// Nazwa schematu powiazanego z tabelą
        /// </summary>
        public string Schema { get;}

        /// <summary>
        /// Nazwa tabeli
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Typ encji przechowywanych w tabeli
        /// </summary>
        public Type EntityType { get; }

        /// <summary>
        /// Pary nazwa pola encji - lista powiązanych z nim atrubutów
        /// </summary>
        public IReadOnlyDictionary<string, List<IEntityFieldAttribute>> EntityPropertyAttributes { get; }

        /// <summary>
        /// Pary nazwa pola encji - typ encji
        /// </summary>
        public IReadOnlyDictionary<string, Type> EntityPropertyNameToType { get; }
    }

    /// <summary>
    /// Reprezentuje tabelę w bazie danych. Podstawowa klasa SimpleORM.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public partial class Table<T> where T : class,new()
    {
        /// <summary>
        /// Powiązana z tabelą baza
        /// </summary>
        private IDatabase _database;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="database"></param>
        /// <param name="name"> Nazwa tabeli </param>
        public Table(IDatabase database, string name)
        {
            _database = database;
            var entityAttr = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(T));
            var entityTrackedFields = EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T));
            Metadata = new TableMetadata(name, entityAttr, entityTrackedFields, typeof(T), database.Schema);
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

        /// <summary>
        /// Zmienia stan encji na umożliwiający zapisanie zmian w bazie danych
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityEntry Update(T entity)
        {
            return _database.StateManager.Update(entity);
        }

        /// <summary>
        /// Zmienia stan encji na umożliwiający usunięcie z bazy
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public EntityEntry Remove(T entity)
        {
            return _database.StateManager.Remove(entity);
        }

        
        public void Drop()
        {
            _database.DropTable(Metadata);
        }

        /// <summary>
        /// Zwraca wszystkie rekordy w dla których wartosc klucza głównego jest równa
        /// wartości argumentu value
        /// </summary>
        /// <param name="field">Nazwa porównywanego pola</param>
        /// <param name="value">Wartość pola</param>
        /// <returns>Znaleziony rekord</returns>
        public T Find(object primaryKey)
        {
            var pkName = EntityFieldAttributeReader.ReadEntityPrimaryKeyName(Metadata.EntityType);
            return _FindWhere(pkName, primaryKey).SingleOrDefault();
        }

        /// <summary>
        /// Zwraca wszystkie rekordy w dla których wartosc pola o nazwie field jest równa
        /// wartości argumentu value
        /// </summary>
        /// <param name="field">Nazwa porównywanego pola</param>
        /// <param name="value">Wartość pola</param>
        /// <returns>Znalezione rekord</returns>
        public T FindWhere(string field, object value)
        {
            return _FindWhere(field, value).SingleOrDefault();
        }

        
        public T[] FindAll(object primaryKey)
        {
            var pkName = EntityFieldAttributeReader.ReadEntityPrimaryKeyName(Metadata.EntityType);
            return _FindWhere(pkName, primaryKey);
        }

        
        public T[] FindAllWhere(string field, object value)
        {
            return _FindWhere(field, value);
        }
    }
}
