using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SimpleORM.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ForeignKey : Attribute, IEntityFieldAttribute
    {
        public string Target { get; }
        public string Referenced { get; }
        public bool Ignore { get; }

        public ForeignKey(string target, string referenced = "Id", bool ignore = false)
        {
            Target = target;
            Referenced = referenced;
            Ignore = ignore;
        }

        private void _CheckRecursiveReferences(Type entityType, Type referencedType)
        {
            var referencedTypeAttributes = EntityFieldAttributeReader.ReadEntityFieldAttributes(referencedType);
            var referencedTrackedFields = EntityFieldAttributeReader.ReadEntityTrackedFields(referencedType);

            if (referencedTypeAttributes.Count == 0 || referencedTrackedFields.Count == 0)
                return;

            var referencedTypeForeignKeys = referencedTypeAttributes
                .Select(kv => kv.Value)
                .SelectMany(attr => attr, (list, attribute) => attribute)
                .Where(attr => attr is ForeignKey)
                .ToList();
            
            var targetsOfForeignKeys = referencedTypeForeignKeys
                .Select(t => (t as ForeignKey).Target)
                .Select(t => referencedType.GetProperty(t).PropertyType).ToList();
            foreach (var targetType in targetsOfForeignKeys)
            {
                if (targetType == entityType)
                {
                    throw new Exception("Cykliczna zależność referencyjna");
                }
                _CheckRecursiveReferences(entityType, targetType);
            }
        }

        public void Validate(Type enityType)
        {
            var targetProp = enityType.GetProperty(Target);
            if (targetProp == null)
            {
                throw new Exception($"Pole {Target} nie istnieje w encji {enityType.Name}");
            }

            var targetType = targetProp.PropertyType;
            if (!EntityFieldAttributeReader.IsSimpleORMEntity(targetType))
            {
                throw new Exception($"{targetType.FullName} nie jest oznaczona atrybutem Entity");
            }

            var targetTypePrimaryKey = targetType.GetProperty(Referenced);
            if (targetTypePrimaryKey == null)
            {
                throw new Exception($"Brak pola {Target} w encji {targetType.FullName}");
            }

            var primaryKeyAttribute = targetTypePrimaryKey.GetCustomAttribute(typeof(PrimaryKey));
            if (primaryKeyAttribute == null)
            {
                throw new Exception($"Pole {targetTypePrimaryKey.Name} w {targetType.FullName} nie posiada atrybutu PrimaryKey");
            }

            _CheckRecursiveReferences(enityType, targetType);
        }

        public Type ReadTargetType(Type entityType)
        {
            return entityType.GetProperty(Target).PropertyType;
        }

        public object ReadTargetValue(object entity)
        {
            return entity.GetType().GetProperty(Target).GetValue(entity);
        }
    }
}
