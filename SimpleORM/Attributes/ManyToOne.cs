using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    /// <summary>
    /// Atrybut pozwalający na utworzenie relacji wiele do jednego między encjami.
    /// Wymagane jest aby w polu ReferencedFk znajdowała się nazwa pola będąca kluczem obcym
    /// powiązanym z kluczem głównym encji w której znajduje się atrybut
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class ManyToOne : Attribute, IEntityFieldAttribute
    {
        public ManyToOne(string target, string referencedFk)
        {
            ReferencedFk = referencedFk;
            Target = target;
        }

        public string ReferencedFk { get; }

        public string Target { get; }

        public void Validate(Type enityType){}
    }
}
