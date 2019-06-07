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
            [typeof(int)] = "int",
            [typeof(DateTime)] = "datetime2",
            [typeof(double)] = "float"
        };

        public static Dictionary<Type, string> AttributeMap = new Dictionary<Type, string>()
        {
            [typeof(PrimaryKey)] = "PRIMARY KEY",
            [typeof(NotNull)] = "NOT NULL"
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
        private Dictionary<Type, TableMetadata> _typeToMetadata;

        public MsSqlTableBuilder(){}

        public MsSqlTableBuilder With(TableMetadata tableMetadata, Dictionary<Type, TableMetadata> typeToMetadata)
        {
            _tableMetadata = tableMetadata;
            _typeToMetadata = typeToMetadata;
            return this;
        }

        public string Build()
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE TABLE [{_tableMetadata.Schema}.{_tableMetadata.Name}] (");
            var fk = new StringBuilder();
            foreach (var name in _tableMetadata.EntityPropertyNameToType.Keys)
            {
                s.Append("  ");
                s.Append($"[{name}] ");
                // TODO: not scalar
                var typeStr = MsSqlTypeMapping.ScalarTypeMap[_tableMetadata.EntityPropertyNameToType[name]];
                // TODO: option
                s.Append($"{typeStr}");

                if (_tableMetadata.EntityPropertyAttributes.ContainsKey(name))
                {
                    var attrs = _tableMetadata.EntityPropertyAttributes[name];
                    if (attrs.Count > 0)
                    {
                        bool hasfk = false;
                        foreach (var entityFieldAttribute in attrs)
                        {
                            entityFieldAttribute.Validate(_tableMetadata.EntityType);

                            var attribute = entityFieldAttribute.GetType();

                            if (attribute == typeof(ForeignKey))
                            {
                                hasfk = true;
                                var foreignKey = entityFieldAttribute as ForeignKey;
                                var foreignTableName =
                                    _typeToMetadata[foreignKey.ReadTargetType(_tableMetadata.EntityType)].Name;
                                fk.AppendLine($"CONSTRAINT fk_{_tableMetadata.Name}_{name}");
                                fk.AppendLine($" FOREIGN KEY ({name})");
                                fk.AppendLine($" REFERENCES [{_tableMetadata.Schema}.{foreignTableName}] ({foreignKey.Referenced})");
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
                                s.Append($" {attrStr}");
                            }
                        }
                        if(hasfk)
                            fk.Append(",");

                    }
                }
                s.AppendLine(",");
            }

            if (fk.Length > 0)
            {
                s.AppendLine(fk.ToString().Substring(0, fk.Length - 1));
            }
            
            s.AppendLine(")");
            return s.ToString();
        }
    }
}
