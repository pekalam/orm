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
        private string _schema;

        public MsSqlUpdateBuilder(EntityEntry entityEntry, string schema)
        {
            _entityEntry = entityEntry;
            _schema = schema;
        }

        public string Build()
        {
            var entityTrackedFields = _entityEntry.TableMetadata.EntityPropertyNameToType;
            var entityFieldAttributes = _entityEntry.TableMetadata.EntityPropertyAttributes;
            int i = 1;
            var sql = new StringBuilder();
            sql.Append($"UPDATE [{_schema}.{_entityEntry.TableMetadata.Name}] SET ");
            foreach (var entityTrackedField in entityTrackedFields)
            {
                var name = entityTrackedField.Key;
                var type = entityTrackedField.Value;
                var value = _entityEntry.TrackedEntity.GetType().GetProperty(name).GetValue(_entityEntry.TrackedEntity);
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

            var primaryKeyName = EntityFieldAttributeReader.ReadEntityPrimaryKey(_entityEntry.TrackedEntity.GetType());
            if (!string.IsNullOrEmpty(primaryKeyName))
            {
                var primaryKey = _entityEntry.TrackedEntity.GetType().GetProperty(primaryKeyName)
                    .GetValue(_entityEntry.TrackedEntity);
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
                    var value = _entityEntry.TrackedEntity.GetType().GetProperty(name).GetValue(_entityEntry.OriginalData);
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
