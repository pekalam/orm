using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleORM.Attributes;

namespace SimpleORM.Providers.MsSql
{
    public static class MsSqlTypeMapping
    {
        public static Dictionary<Type, string> ScalarTypeMap = new Dictionary<Type, string>()
        {
            [typeof(string)] = "text",
            [typeof(int)] = "int"
        };

        public static Dictionary<Type, string> AttributeMap = new Dictionary<Type, string>()
        {
            [typeof(PrimaryKey)] = "PRIMARY KEY",
            [typeof(ForeignKey)] = "FOREIGN KEY",
            [typeof(AutoIncrement)] = "IDENTITY"
        };
    }

    public class MsSqlTableBuilder
    {
        private TableMetadata _tableMetadata;

        public MsSqlTableBuilder(TableMetadata tableMetadata)
        {
            _tableMetadata = tableMetadata;
        }

        public string Build()
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE TABLE [{_tableMetadata.Name}] (");

            foreach (var name in _tableMetadata.EntityPropertyNameToType.Keys)
            {
                s.Append("  ");
                s.Append($"[{name}] ");
                // TODO: not scalar
                var typeStr = MsSqlTypeMapping.ScalarTypeMap[_tableMetadata.EntityPropertyNameToType[name]];
                // TODO: option
                s.Append($"{typeStr} ");

                if (_tableMetadata.EntityPropertyAttributes.ContainsKey(name))
                {
                    var attrs = _tableMetadata.EntityPropertyAttributes[name];
                    if (attrs.Count > 0)
                    {
                        foreach (var entityFieldAttribute in attrs)
                        {
                            try
                            {
                                entityFieldAttribute.Validate(_tableMetadata.EntityType, _tableMetadata.EntityPropertyNameToType[name], name);
                            }
                            catch (Exception)
                            {
                                throw;
                            }
                            var attrStr = MsSqlTypeMapping.AttributeMap[entityFieldAttribute.GetType()];
                            s.Append($"{attrStr} ");
                        }
                    }
                }
                s.AppendLine("NOT NULL,");
            }

            s.AppendLine(")");
            s.AppendLine("GO");
            return s.ToString();
        }
    }
}
