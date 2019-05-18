using System;
using System.Collections.Generic;

namespace SimpleORM
{

    public class EntityEntry
    {
        public EntityEntry(object entity, TableMetadata tableMetadata)
        {
            TrackedEntity = entity;
            TableMetadata = tableMetadata;
        }

        public List<object> GetTrackedValues(object entity)
        {
            var list = new List<object>();
            foreach (var propertyName in TableMetadata.EntityPropertyNameToType.Keys)
            {
                list.Add(entity.GetType().GetProperty(propertyName).GetValue(entity));
            }

            return list;
        }

        public EntityState State { get; set; }

        public object TrackedEntity { get; }

        public TableMetadata TableMetadata { get; }
    }
}