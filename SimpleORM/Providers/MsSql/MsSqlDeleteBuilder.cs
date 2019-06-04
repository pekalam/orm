using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Providers.MsSql
{
    public class MsSqlDeleteBuilder
    {
        private EntityEntry _entityEntry;
        public MsSqlDeleteBuilder(){}

        public MsSqlDeleteBuilder With(EntityEntry entityEntry)
        {
            _entityEntry = entityEntry;
            return this;
        }

        public string Build()
        {
            var sql = new StringBuilder();
            sql.Append($"DELETE FROM [{_entityEntry.TableMetadata.Schema}.{_entityEntry.TableMetadata.Name}] WHERE ");
            var pk = EntityFieldAttributeReader.ReadEntityPrimaryKeyName(_entityEntry.TrackedEntity.GetType());
            if (!string.IsNullOrEmpty(pk))
            {
                var value = EntityFieldAttributeReader.ReadEntityPrimaryKeyValue(_entityEntry.TrackedEntity);
                sql.Append($"{pk}={value}");
            }
            else
            {
                int i = 1;
                foreach (var pair in _entityEntry.TableMetadata.EntityPropertyNameToType)
                {
                    var value = _entityEntry.FieldValue(pair.Key);
                    if(i != _entityEntry.TableMetadata.EntityPropertyNameToType.Count)
                        sql.Append($"{pair.Key}='{value}' AND ");
                    else
                    {
                        sql.Append($"{pair.Key}='{value}'");
                    }
                    i++;
                }
            }

            return sql.ToString();
        }
    }
}
