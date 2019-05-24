using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ManyToOne : Attribute, IEntityFieldAttribute
    {
        public ManyToOne(string target, string referencedPk)
        {
            ReferencedPk = referencedPk;
            Target = target;
        }

        public string ReferencedPk { get; }

        public string Target { get; }

        public void Validate(Type enityType){}
    }
}
