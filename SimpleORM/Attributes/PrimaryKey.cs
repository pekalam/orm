using System;
using System.Reflection;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKey : Attribute, IEntityFieldAttribute
    {
        public bool Validate(Type enityType, Type decoratedPropType, string decoratedPropName)
        {
            return true;
        }
    }
}