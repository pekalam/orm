using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Attributes;

namespace SimpleORM.Providers.MsSql
{
    /// <summary>
    /// Klasa pomocnizca sluzaca do budowania wyrazen SQL aktualizujacych
    /// wiersz/wiersze w tabeli
    /// </summary>
    public class MsSqlUpdateBuilder
    {
        private EntityEntry _entityEntry;

        public MsSqlUpdateBuilder()
        {
        }

        public MsSqlUpdateBuilder With(EntityEntry entityEntry)
        {
            _entityEntry = entityEntry;
            return this;
        }

        public string Build()
        {
            var entityTrackedFields = _entityEntry.TableMetadata.EntityPropertyNameToType;
            int i = 1;
            var sql = new StringBuilder();
            sql.Append($"UPDATE [{_entityEntry.TableMetadata.Schema}.{_entityEntry.TableMetadata.Name}] SET ");
            foreach (var entityTrackedField in entityTrackedFields)
            {
                var name = entityTrackedField.Key;
                var type = entityTrackedField.Value;
                var value = _entityEntry.FieldValue(name);
                if (type == typeof(string))
                {
                    sql.Append($"[{name}]='{value}'");
                }
                else
                {
                    sql.Append($"[{name}]={value}");
                }
                if (i != entityTrackedFields.Count)
                    sql.Append(", ");

                i++;
            }

            sql.Append(" WHERE ");

            var primaryKeyName = EntityFieldAttributeReader.ReadEntityPrimaryKeyName(_entityEntry.TrackedEntity.GetType());
            if (!string.IsNullOrEmpty(primaryKeyName))
            {
                var primaryKey = _entityEntry.FieldValue(primaryKeyName);
                if (entityTrackedFields[primaryKeyName] == typeof(string))
                {
                    sql.Append($"[{primaryKeyName}]='{primaryKey}'");
                }
                else
                {
                    sql.Append($"[{primaryKeyName}]={primaryKey}");
                }
            }
            else
            {
                i = 1;
                foreach (var entityTrackedField in entityTrackedFields)
                {
                    var name = entityTrackedField.Key;
                    var value = _entityEntry.OriginalFieldValue(name);
                    sql.Append($"[{name}]={value}");
                    if (i != entityTrackedFields.Count)
                        sql.Append(" AND ");
                    i++;
                }
            }


            return sql.ToString();
        }
    }
}
