using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SimpleORM.Attributes;

namespace SimpleORM
{
    public static class EntityFieldAttributeReader
    {
        public static Dictionary<string, List<IEntityFieldAttribute>> ReadEntityFieldAttributes(object obj) =>
            ReadEntityFieldAttributes(obj.GetType());

        public static Dictionary<string, List<IEntityFieldAttribute>> ReadEntityFieldAttributes(Type objType)
        {
            var propNameToAttribute = new Dictionary<string, List<IEntityFieldAttribute>>();

            foreach (var property in objType.GetProperties())
            {
                var customAttributes = property.GetCustomAttributes<Attribute>();
                foreach (var attribute in customAttributes)
                {
                    if (attribute is IEntityFieldAttribute item)
                    {
                        if(!propNameToAttribute.ContainsKey(property.Name))
                            propNameToAttribute[property.Name] = new List<IEntityFieldAttribute>();
                        propNameToAttribute[property.Name].Add(item);
                    }
                }               
            }

            return propNameToAttribute;
        }

        public static Dictionary<string, Type> ReadEntityTrackedFields(object obj) =>
            ReadEntityTrackedFields(obj.GetType());


        public static Dictionary<string, Type> ReadEntityTrackedFields(Type objType)
        {
            var trackedProperties = new Dictionary<string, Type>();

            foreach (var property in objType.GetProperties())
            {
                var customAttributes = property.GetCustomAttribute<NotTracked>();
                if(customAttributes == null)
                    trackedProperties.Add(property.Name, property.PropertyType);
            }

            return trackedProperties;
        }

    }
}
