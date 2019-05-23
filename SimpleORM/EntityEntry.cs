using System;
using System.Collections.Generic;

namespace SimpleORM
{
    /// <summary>
    /// Reprezentuje encję dodana do ORM.
    /// </summary>
    public class EntityEntry
    {
        public EntityEntry(object entity, TableMetadata tableMetadata)
        {
            TrackedEntity = entity;
            TableMetadata = tableMetadata;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<object> GetTrackedValues()
        {
            var list = new List<object>();
            foreach (var propertyName in TableMetadata.EntityPropertyNameToType.Keys)
            {
                list.Add(TrackedEntity.GetType().GetProperty(propertyName).GetValue(TrackedEntity));
            }

            return list;
        }

        /// <summary>
        /// Stan EntityEntry
        /// </summary>
        public EntityState State { get; set; }

        /// <summary>
        /// POCO reprezentowane przez EntityEntry
        /// </summary>
        public object TrackedEntity { get; }

        /// <summary>
        /// Metadane powiązanej z encją tabeli
        /// </summary>
        public TableMetadata TableMetadata { get; }
    }
}