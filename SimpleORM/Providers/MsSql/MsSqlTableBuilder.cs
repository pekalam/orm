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
            [typeof(AutoIncrement)] = "IDENTITY"
        };

        public static List<string> OnUpdateDelete = new List<string>()
        {
            "NO ACTION", "CASCADE", "SET NULL", "SET DEFAULT"
        };
    }

    /// <summary>
    /// Klasa pomocnicza sluzaca do budowania wyrazen SQL tworzacych tabele
    /// </summary>
    public class MsSqlTableBuilder
    {
        private TableMetadata _tableMetadata;
        private string _schema;

        public MsSqlTableBuilder(TableMetadata tableMetadata, string schema)
        {
            _tableMetadata = tableMetadata;
            _schema = schema;
        }

        public string Build()
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE TABLE [{_schema}.{_tableMetadata.Name}] (");
            StringBuilder fk = new StringBuilder();
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
                            entityFieldAttribute.Validate(_tableMetadata.EntityType);

                            var attribute = entityFieldAttribute.GetType();

                            if (attribute == typeof(ForeignKey))
                            {
                                var foreignKey = entityFieldAttribute as ForeignKey;
                                var refTable = _tableMetadata.EntityType.GetProperty(foreignKey.Target).PropertyType;
                                fk.AppendLine($"CONSTRAINT fk_{_tableMetadata.Name}_{name}");
                                fk.AppendLine($" FOREIGN KEY ({name})");
                                fk.AppendLine($" REFERENCES [{_schema}.{refTable.Name}] ({foreignKey.Referenced})");
                            }
                            else if (attribute == typeof(OnUpdate))
                            {
                                var onUpdate = (entityFieldAttribute as OnUpdate).OnUpdateStr;
                                if (MsSqlTypeMapping.OnUpdateDelete.Contains(onUpdate))
                                {
                                    fk.AppendLine("ON UPDATE " + onUpdate);
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else if (attribute == typeof(OnDelete))
                            {
                                var onDeleteStr = (entityFieldAttribute as OnDelete).OnDeleteStr;
                                if (MsSqlTypeMapping.OnUpdateDelete.Contains(onDeleteStr))
                                {
                                    fk.AppendLine("ON DELETE " + onDeleteStr);
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                            else
                            {
                                var attrStr = MsSqlTypeMapping.AttributeMap[attribute];
                                s.Append($"{attrStr} ");
                            }
                        }
                    }
                }
                s.AppendLine("NOT NULL,");
            }

            s.AppendLine(fk.ToString());
            s.AppendLine(")");
            return s.ToString();
        }
    }
}
