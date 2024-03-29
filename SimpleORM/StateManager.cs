﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM
{
    public interface IStateManager
    {
        EntityEntry Add(object entity);
        EntityEntry Update(object entity);
        EntityEntry Remove(object entity);
        void ApplyState(IReadOnlyList<EntityEntry> entities, EntityState newState);
        IReadOnlyList<EntityEntry> GetEntriesToSave();
        void DropEntriesFromTable(TableMetadata tableMetadata);
        IDatabase Database { set; }
        void SaveChanges();
        void AddOrUpdate(object[] entities, TableMetadata tableMetadata);
    }

    /// <summary>
    /// Zarządza stanem wszystkich encji które zostały powiązane z ORM.
    /// Metody Add, Update, Remove itd są wywoływane przez klasę Database
    /// </summary>
    public class StateManager : IStateManager
    {
        private Dictionary<object, EntityEntry> _entityEntries = new Dictionary<object, EntityEntry>();

        private object CheckIsNull(object ob) => ob ?? throw new NullReferenceException(); 

        public EntityEntry Add(object entity)
        {
            if(_entityEntries.ContainsKey(CheckIsNull(entity)))
                throw new Exception("Entity is already tracked");

            var entityEntry = new EntityEntry(entity, Database.GetTableMetadataForEntity(entity));
            entityEntry.State = EntityState.Added;
            _entityEntries[entity] = entityEntry;

            return entityEntry;
        }

        public EntityEntry Update(object entity)
        {
            if(!_entityEntries.ContainsKey(CheckIsNull(entity)))
                throw new Exception("Entity is not tracked");

            var entry = _entityEntries[entity];
            if (entry.State == EntityState.Added)
                return entry;
            entry.State = EntityState.Modified;

            return entry;
        }

        public EntityEntry Remove(object entity)
        {
            if (!_entityEntries.ContainsKey(CheckIsNull(entity)))
                throw new Exception("Entity is not tracked");

            var entry = _entityEntries[entity];
            if (entry.State == EntityState.Detached)
                return entry;

            entry.State = entry.State == EntityState.Added ? EntityState.Detached : EntityState.Deleted;

            return entry;
        }

        public void ApplyState(IReadOnlyList<EntityEntry> entities, EntityState newState)
        {
            foreach (var entity in entities)
            {
                if(!_entityEntries.ContainsKey(entity.TrackedEntity))
                    throw new Exception("Entity is not tracked");
                _entityEntries[entity.TrackedEntity].State = newState;
            }
        }

        public void AddOrUpdate(object[] entities, TableMetadata tableMetadata)
        {
            foreach (var entity in entities)
            {
                var key = _entityEntries.Keys
                    .SingleOrDefault(k => EntityFieldAttributeReader.ReadEntityPrimaryKeyValue(k) ==
                                          EntityFieldAttributeReader.ReadEntityPrimaryKeyValue(entity));
                if (key != null)
                {
                    _entityEntries.Remove(key);
                    var newEntry = new EntityEntry(entity, tableMetadata);
                    newEntry.State = EntityState.Unchanged;
                    _entityEntries.Add(entity, newEntry);
                }
                else
                {
                    var newEntry = new EntityEntry(entity, tableMetadata);
                    newEntry.State = EntityState.Unchanged;
                    _entityEntries.Add(entity, newEntry);
                }
            }
        }

        public void DropEntriesFromTable(TableMetadata tableMetadata)
        {
            var toRemove = _entityEntries.Where(v => v.Value.TableMetadata.Equals(tableMetadata)).ToArray();
            for (int i = 0; i < toRemove.Length; i++)
                _entityEntries.Remove(toRemove[i].Key);
        }

        private List<EntityEntry> _GetEntriesToSave()
        {
            var list = _entityEntries
                .Select(p => p.Value)
                .Where(p => p.State != EntityState.Unchanged && p.State != EntityState.Detached)
                .ToList();
            return list;
        }

        public IReadOnlyList<EntityEntry> GetEntriesToSave()
        {
            return _GetEntriesToSave();
        }

        public IDatabase Database { get; set; }

        public int EntryCount => _entityEntries.Count;

        public void SaveChanges()
        {
            var toSave = GetEntriesToSave();
            foreach (var entityEntry in toSave)
            {
                entityEntry.State = EntityState.Unchanged;
            }
        }

        
    }
}
