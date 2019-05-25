using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class AutoIncrement : Attribute, IEntityFieldAttribute
    {
        public void Validate(Type enityType)
        {
        }
    }
}
