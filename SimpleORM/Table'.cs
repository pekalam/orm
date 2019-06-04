using System;
using System.Collections.Generic;
using System.Linq;
using SimpleORM.Attributes;

namespace SimpleORM
{
    public partial class Table<T>
    {
        /// <summary>
        /// Główna funkcja służaca do wyszukiwania w bazie danych
        /// </summary>
        /// <param name="field">Nazwa kolumny</param>
        /// <param name="value">Wartość kolumny</param>
        /// <returns>Znalezione rekordy</returns>
        private T[] _FindWhere(string field, object value)
        {
            //Wykonanie zapytania do tabeli w bazie danych
            var reader = _database.DatabaseProvider.FindWhere(field, value, Metadata);

            var trackedProps = EntityFieldAttributeReader.ReadEntityTrackedFields(typeof(T));
            var attributes = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(T));

            if (trackedProps.Count != reader.FieldCount)
                throw new Exception($"Tabela w bazie danych różni się od {Metadata.Name}");

            //Przypisanie uzyskanych danych do obiektu

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

            
            //Dla kazdego otrzymanego rekordu wykonywane jest zapytanie
            //jeżeli posiada klucze obce
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
                        //Wykonanie zapytania
                        var refEntity = _database.Find(_database.GetTableMetadataForEntity(targetProp.PropertyType), refId);

                        targetProp.SetValue(obj, refEntity);
                    }
                }


                //Wykonanie zapytan zwiazanych z relacją jeden do wielu
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
                        //Wykonanie zapytania
                        var fetched = _database.FindAllWhere(refTableMetadata, attribute.ReferencedFk, pk);

                        var objList = targetProp.GetValue(obj);

                        //Dodanie do listy zwracanego obiektu
                        foreach (var ob in fetched)
                        {
                            objList.GetType().GetMethod("Add").Invoke(objList, new[] { ob });
                        }
                    }

                }
            }

            var fetchedArray = list.ToArray();
            //Aktualizacja menedzera stanu
            _database.StateManager.AddOrUpdate(fetchedArray, Metadata);
            return fetchedArray;
        }
    }
}