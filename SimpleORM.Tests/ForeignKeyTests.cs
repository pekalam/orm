using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using SimpleORM.Attributes;
using SimpleORM.Tests.TestUtil;

namespace SimpleORM.Tests
{
    [TestFixture]
    class ForeignKeyTests
    {

        [Entity]
        class Entity1
        {
            [PrimaryKey]
            public int Id { get; set; }
            [ForeignKey("Entity2")]
            public int Fk { get; set; }
            public Entity2 Entity2 { get; set; }
        }

        [Entity]
        class Entity2
        {
            [PrimaryKey]
            public int Id { get; set; }
        }

        [Test]
        public void T1()
        {
            var attrs = EntityFieldAttributeReader.ReadEntityFieldAttributes(typeof(Entity1));

            Assert.IsTrue(attrs.ContainsKey("Fk"));
            Assert.IsTrue(attrs["Fk"].Count == 1);
            Assert.IsTrue(attrs["Fk"][0] is ForeignKey);
            Assert.IsTrue((attrs["Fk"][0] as ForeignKey).Target == "Entity2");
            Assert.IsTrue((attrs["Fk"][0] as ForeignKey).Validate(typeof(Entity1), typeof(int), "Fk"));
        }

        

        [Test]
        [TestCase(typeof(RecurrentEntities.NotRecurrent1.Entry), "Fk", false)]
        [TestCase(typeof(RecurrentEntities.Recurrent1.Entry), "Fk", true)]
        [TestCase(typeof(RecurrentEntities.Recurrent2.Entry), "Fk", true)]
        [TestCase(typeof(RecurrentEntities.Recurrent3.Entry), "Fk", true)]
        public void Validate_when_recursive_depedency_throws(Type entityType, string fkName, bool throws)
        {
            var attrs = EntityFieldAttributeReader.ReadEntityFieldAttributes(entityType);

            var fk = attrs[fkName][0] as ForeignKey;
            try
            {
                fk.Validate(entityType, typeof(int), fkName);
            }
            catch (Exception e)
            {
                TestContext.WriteLine(e.ToString());
                if (e.Message != "Cykliczna zależność referencyjna")
                    throw e;
                if(throws)
                    Assert.IsTrue(true);
                else
                {
                    Assert.IsTrue(false);
                }
                return;
            }
            if(throws)
                Assert.IsTrue(false);
            else
            {
                Assert.IsTrue(true);
            }
        }

        

    }
}
