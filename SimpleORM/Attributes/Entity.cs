using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleORM.Attributes
{
    /// <summary>
    /// Atrybut obowiązkowy dla każdej klasy mapowanej przez ORM
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class Entity : Attribute, IEntityFieldAttribute
    {
        public void Validate(Type enityType)
        {
        }
    }
}
