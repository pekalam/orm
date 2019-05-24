using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace SimpleORM.Providers.MsSql
{
    public class MsSqlInsertBuilder
    {
        private EntityEntry _entityEntry;

        public MsSqlInsertBuilder()
        {
        }

        public MsSqlInsertBuilder With(EntityEntry entityEntry)
        {
            _entityEntry = entityEntry;
            return this;
        }

        public string Build()
        {
            var sql = new StringBuilder();
            int i = 1;
            sql.Append($"INSERT INTO [{_entityEntry.TableMetadata.Schema}.{_entityEntry.TableMetadata.Name}] VALUES (");
            foreach (var pair in _entityEntry.TableMetadata.EntityPropertyNameToType)
            {
                var value = _entityEntry.TrackedEntity.GetType().GetProperty(pair.Key)
                    .GetValue(_entityEntry.TrackedEntity).ToString();
                sql.Append($"'{value}'");
                if (i != _entityEntry.TableMetadata.EntityPropertyNameToType.Count)
                    sql.Append(", ");
                i++;
            }

            sql.Append(")");
            return sql.ToString();
        }
    }
}
