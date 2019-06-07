using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using FastDeepCloner;
using SimpleORM.Attributes;

namespace SimpleORM
{
    public static class EntityEntryExtensions
    {
        private static string _SqlStr(object ob)
        {
            return ob.ToString().Replace("'", "''");
        }

        public static string FieldValue(this object obj, string propName)
        {
            return _SqlStr(obj.GetType().GetProperty(propName).GetValue(obj));
        }

        public static string FieldValue(this EntityEntry entityEntry, string propName) =>
            FieldValue(entityEntry.TrackedEntity, propName);

        public static string OriginalFieldValue(this EntityEntry entityEntry, string propName) =>
            FieldValue(entityEntry.OriginalData, propName);

    }

    /// <summary>
    /// Reprezentuje encję obserwowaną przez ORM. Przechowuje referencje do utworzonego obiektu oraz jego stan EntityState
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
        /// Obiekt klasy reprezentowanej przez EntityEntry
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