using System;
using System.Collections.Generic;
using System.Linq;
using SimpleORM.Attributes;

namespace SimpleORM
{
    public partial class Table<T>
    {
        
        private T[] _FindWhere(string field, object value)
        {
            var reader = _database.DatabaseProvider.FindWhere(field, value, Metadata);

            var trackedProps = EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T));
            var attributes = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(T));

            if (trackedProps.Count != reader.FieldCount)
                throw new Exception($"Tabela w bazie danych różni się od {Metadata.Name}");

            List<T> list = new List<T>();
            int i = 0;
            while (reader.Read())
            {
                T obj = new T();
                int j = 0;
                foreach (var kv in trackedProps)
                {
                    var fieldVal = reader[j];
                    obj.GetType().GetProperty(kv.Key).SetValue(obj, fieldVal);
                    j++;
                }
                i++;
                list.Add(obj);
            }
            reader.Close();

            foreach (var obj in list)
            {
                foreach (var propName in trackedProps.Keys)
                {
                    if (attributes.ContainsKey(propName))
                    {
                        var fk = attributes[propName].OfType<ForeignKey>().SingleOrDefault();
                        if (fk == null || fk.Ignore)
                            continue;
                        var targetProp = obj.GetType().GetProperty(fk.Target);
                        var refId = obj.GetType().GetProperty(propName).GetValue(obj);
                        var refEntity = _database.Find(_database.GetTableMetadataForEntity(targetProp.PropertyType), refId);

                        targetProp.SetValue(obj, refEntity);
                    }
                }

                var manyToOne = attributes.SelectMany(kv => kv.Value).OfType<ManyToOne>().ToList();
                if (manyToOne.Count > 0)
                {
                    foreach (var attribute in manyToOne)
                    {
                        var targetProp = obj.GetType().GetProperty(attribute.Target);
                        var refType = targetProp.PropertyType
                            .GenericTypeArguments[0];
                        var refTableMetadata = _database
                            .GetTableMetadataForEntity(refType);

                        var pk = EntityFieldAttributeReader.ReadEntityPrimaryKeyValue(obj);
                        var fetched = _database.FindAllWhere(refTableMetadata, attribute.ReferencedPk, pk);

                        var objList = targetProp.GetValue(obj);

                        foreach (var ob in fetched)
                        {
                            objList.GetType().GetMethod("Add").Invoke(objList, new[] { ob });
                        }
                    }

                }
            }
            
            return list.ToArray();
        }
    }
}