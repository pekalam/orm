using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;

namespace SimpleORM.Tests
{

    [TestFixture()]
    public class EntityFieldAttributeReaderTests
    {
        class TestEntity1
        {
            [PrimaryKey]
            [ForeignKey]
            public int Id { get; set; }
            [ForeignKey]
            public string Name { get; set; }
            [NotTracked]
            public string F1 { get; set; }
        }

        [Test]
        public void ReadEntityFieldAttributes_called_on_entity_returns()
        {
            var entity = new TestEntity1();

            var attrs = EntityFieldAttributeReader.ReadEntityFieldAttributes(entity);

            Assert.IsTrue(attrs["Id"].Count == 2);
            Assert.IsTrue(attrs["Id"][0].GetType() == typeof(PrimaryKey));
            Assert.IsTrue(attrs["Id"][1].GetType() == typeof(ForeignKey));
            Assert.IsTrue(attrs["Name"][0].GetType() == typeof(ForeignKey));
            Assert.IsTrue(attrs.Count == 2);
        }

        [Test]
        public void ReadEntityTrackedFields_called_on_entity_returns()
        {
            var entity = new TestEntity1();

            var fields = EntityFieldAttributeReader.ReadEntityTrackedFields(entity);

            Assert.IsTrue(fields.Count == 2);
            Assert.IsTrue(fields.ContainsKey("Id"));
            Assert.IsTrue(fields.ContainsKey("Name"));
            Assert.IsTrue(fields["Id"] == typeof(int));
            Assert.IsTrue(fields["Name"] == typeof(string));
        }
    }
}
