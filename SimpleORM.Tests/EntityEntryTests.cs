using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Attributes;
using SimpleORM.Tests.TestUtil;

namespace SimpleORM.Tests
{
    [TestFixture]
    public class EntityEntryTests
    {
        class TestEntity1
        {
            [PrimaryKey]
            public int Id { get; set; }
            [ForeignKey]
            public int F2 { get; set; }
            [NotTracked]
            public string Name { get; set; }
        }

        class TestEntity2
        {
            [PrimaryKey]
            [ForeignKey]
            public int Id { get; set; }
            [NotTracked]
            public string Name { get; set; }
        }

        [Test]
        public void EntityEntry_does_not_track_NotTracked()
        {
            var entity = new TestEntity1();
            var entry = new EntityEntry(entity, TestObjectFactory.CreateTableMetadata(entity));

            Assert.IsTrue(entry.TableMetadata.EntityPropertyNameToType.Count == 2);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyNameToType.ContainsKey("Id"));
            Assert.IsTrue(entry.TableMetadata.EntityPropertyNameToType.ContainsKey("F2"));
            Assert.IsFalse(entry.TableMetadata.EntityPropertyNameToType.ContainsKey("Name"));
        }

        [Test]
        public void EntityEntry_has_IEntityFieldAttributes_mapped_to_properties()
        {
            var entity = new TestEntity1();
            var entry = new EntityEntry(entity, TestObjectFactory.CreateTableMetadata(entity));

            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes.Count == 2);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes["Id"][0] is PrimaryKey);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes["F2"][0] is ForeignKey);
        }

        [Test]
        public void EntityEntry_has_more_than_one_IEntityFieldAttribute_mapped_to_property()
        {
            var entity = new TestEntity2();
            var entry = new EntityEntry(entity, TestObjectFactory.CreateTableMetadata(entity));

            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes.Count == 1);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes["Id"].Count == 2);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes["Id"][0] is PrimaryKey);
            Assert.IsTrue(entry.TableMetadata.EntityPropertyAttributes["Id"][1] is ForeignKey);
        }
    }
}
