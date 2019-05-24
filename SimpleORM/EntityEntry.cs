﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using FastDeepCloner;

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
            OriginalData = entity.Clone();
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
        /// Kopia obiektu powiazanego z encją w momencie powiazania z ORM
        /// </summary>
        public object OriginalData { get; }

        /// <summary>
        /// Metadane powiązanej z encją tabeli
        /// </summary>
        public TableMetadata TableMetadata { get; }
    }
}