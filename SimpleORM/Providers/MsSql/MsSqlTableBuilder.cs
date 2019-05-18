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
        public string SQL { get; private set; } = "";

        public MsSqlTableBuilder(TableMetadata tableMetadata)
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE TABLE [{tableMetadata.Name}] (");
            
            foreach (var name in tableMetadata.EntityPropertyNameToType.Keys)
            {
                
                s.Append("  ");
                s.Append($"[{name}] ");
                // TODO: not scalar
                var typeStr = MsSqlTypeMapping.ScalarTypeMap[tableMetadata.EntityPropertyNameToType[name]];
                // TODO: option
                s.Append($"{typeStr} ");

                if (tableMetadata.EntityPropertyAttributes.ContainsKey(name))
                {
                    var attrs = tableMetadata.EntityPropertyAttributes[name];
                    if (attrs.Count > 0)
                    {
                        foreach (var entityFieldAttribute in attrs)
                        {
                            var attrStr = MsSqlTypeMapping.AttributeMap[entityFieldAttribute.GetType()];
                            s.Append($"{attrStr} ");
                        }
                    }
                }
                

                s.AppendLine("NOT NULL,");
;            }

            s.AppendLine(")");
            s.AppendLine("GO");

            SQL = s.ToString();
        }
    }
}
