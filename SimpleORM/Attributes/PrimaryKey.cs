using System;
using System.Reflection;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKey : Attribute, IEntityFieldAttribute
    {
        public void Validate(Type enityType)
        {
        }
    }
}