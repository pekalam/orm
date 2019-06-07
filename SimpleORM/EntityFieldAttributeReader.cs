using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Attributes;

namespace SimpleORM
{
    /// <summary>
    /// Przy pomocy tej klasy odbywa się odczyt informacji powiązanych z encją za pomocą atrybutów.
    /// </summary>
    public static class EntityFieldAttributeReader
    {
        public static bool IsSimpleType(Type type)
        {
            return
                type.IsPrimitive ||
                new Type[]
                {
                    typeof(Enum),
                    typeof(String),
                    typeof(Decimal),
                    typeof(DateTime),
                    typeof(DateTimeOffset),
                    typeof(TimeSpan),
                    typeof(Guid)
                }.Contains(type) ||
                Convert.GetTypeCode(type) != TypeCode.Object;
        }

        public static bool IsSimpleORMEntity(Type type) => type.GetCustomAttributes(typeof(Entity), false).Length == 1;

        public static bool IsListOfSimpleORMEntities(PropertyInfo prop)
            => (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
               && IsSimpleORMEntity(prop.PropertyType.GenericTypeArguments[0]);

        public static Dictionary<string, List<IEntityFieldAttribute>> ReadEntityFieldAttributes(object obj) =>
            ReadEntityFieldAttributes(obj.GetType());

        /// <summary>
        /// Odczytuje atrybuty SimpleORM powiązane z encją. Jeśli typ pola encji nie jest
        /// typem obsługiwanym przez ORM wyrzuca wyjątek.
        /// </summary>
        /// <param name="objType">Typ encji</param>
        /// <returns> Pary pole klasy encji - lista powiązanych atrybutów z polem </returns>
        public static Dictionary<string, List<IEntityFieldAttribute>> ReadEntityFieldAttributes(Type objType)
        {
            var propNameToAttribute = new Dictionary<string, List<IEntityFieldAttribute>>();

            if (!IsSimpleORMEntity(objType))
            {
                throw new Exception($"Klasa {objType.FullName} nie jest oznaczona atrybutem Entity");
            }

            foreach (var property in objType.GetProperties())
            {
                var propertyT = property.PropertyType;

                var manyToOne = property.GetCustomAttribute<ManyToOne>();
                if (manyToOne != null && IsListOfSimpleORMEntities(property))
                {
                    propNameToAttribute[property.Name] = new List<IEntityFieldAttribute>(){manyToOne};
                    continue;
                }
                if (IsSimpleORMEntity(propertyT))
                {
                    continue;
                }
                if (!IsSimpleType(propertyT))
                {
                    throw new Exception($"Typ {property.PropertyType.FullName} nie jest obsługiwany");
                }
                var customAttributes = property.GetCustomAttributes<Attribute>().OfType<IEntityFieldAttribute>();
                if(customAttributes.Any())
                    propNameToAttribute[property.Name] = new List<IEntityFieldAttribute>();
                foreach (var attribute in customAttributes)
                {
                    propNameToAttribute[property.Name].Add(attribute);
                }               
            }

            return propNameToAttribute;
        }

        public static Dictionary<string, Type> ReadEntityTrackedFields(object obj) =>
            ReadEntityTrackedFields(obj.GetType());

        /// <summary>
        /// Odczytuje nazwy pól klasy które mogą być obserwowane przez ORM
        /// </summary>
        /// <param name="objType">Typ encji</param>
        /// <returns>Pary nazwa pola - typ pola</returns>
        public static Dictionary<string, Type> ReadEntityTrackedFields(Type objType)
        {
            var trackedProperties = new Dictionary<string, Type>();

            if (!IsSimpleORMEntity(objType))
            {
                throw new Exception($"Klasa {objType.Name} nie jest oznaczona atrybutem Entity");
            }
            foreach (var property in objType.GetProperties())
            {
                var propertyT = property.PropertyType;
                if (IsSimpleORMEntity(propertyT) || IsListOfSimpleORMEntities(property))
                {
                    continue;
                }
                if (!IsSimpleType(propertyT))
                {
                    throw new Exception($"Typ {property.PropertyType.Name} nie jest obsługiwany");
                }
                var customAttributes = property.GetCustomAttribute<NotTracked>();
                if(customAttributes == null)
                    trackedProperties.Add(property.Name, property.PropertyType);
            }

            return trackedProperties;
        }

        /// <summary>
        /// Zwraca nazwę pola które jest kluczem głównym dla encji
        /// </summary>
        /// <param name="type">Typ encji</param>
        /// <returns>Nazwa pola będąca kluczem głównym encji</returns>
        public static string ReadEntityPrimaryKeyName(Type type)
        {
            if(!IsSimpleORMEntity(type))
                throw new Exception();

            var pk = type.GetProperties()
                .Select(p => p)
                .Where(p => p.GetCustomAttribute(typeof(PrimaryKey)) != null)
                .Select(p => p.Name)
                .ToList();
            if(pk.Count == 1)
                return pk[0];
            if(pk.Count > 1)
                throw new Exception($"Ilość kluczy głównych encji {type.FullName} jest większa niż 1");

            return string.Empty;
        }

        public static object ReadEntityPrimaryKeyValue(object entity)
        {
            var pkName = ReadEntityPrimaryKeyName(entity.GetType());
            return entity.FieldValue(pkName);
        }

    }
}
