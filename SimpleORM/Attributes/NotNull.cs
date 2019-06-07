using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NotNull : Attribute, IEntityFieldAttribute
    {
        public void Validate(Type enityType)
        {
            return;
        }
    }
}
