using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleORM.Tests.TestUtil
{
    static class TestObjectFactory
    {
        public static TableMetadata CreateTableMetadata(object obj) => CreateTableMetadata(obj.GetType());

        public static TableMetadata CreateTableMetadata(Type obj)
        {
            var entityAttr = EntityFieldAttributeReader.ReadEntityFieldAttributes(obj);
            var entityTrackedFields = EntityFieldAttributeReader.ReadEntityTrackedFields(obj);
            return new TableMetadata("Test", entityAttr, entityTrackedFields, typeof(string), "orm");
        }
    }
}
