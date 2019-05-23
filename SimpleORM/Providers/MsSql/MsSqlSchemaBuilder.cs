using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Providers.MsSql
{
    public class MsSqlSchemaBuilder
    {
        private string _name;
        public MsSqlSchemaBuilder(string name)
        {
            _name = name;
        }

        public string Build()
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE SCHEMA {_name};");
            
            return s.ToString();
        }
    }

    public class MsSqlDatabaseBuilder
    {
        private string _name;

        public MsSqlDatabaseBuilder(string name)
        {
            _name = name;
        }

        public string Build()
        {
            var s = new StringBuilder();
            s.AppendLine($"CREATE DATABASE {_name};");

            return s.ToString();
        }
    }
}
