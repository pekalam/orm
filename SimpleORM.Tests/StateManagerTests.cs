using System;
using System.Collections.Generic;
using System.Text;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal;
using SimpleORM.Tests.TestUtil;

namespace SimpleORM.Tests
{
    class TestEntity1
    {
        public string Name { get; set; }
    }

    [TestFixture()]
    class StateManagerTests
    {
        private StateManager CreateStateManager()
        {
            Mock<IDatabase> dbStub = new Mock<IDatabase>();
            dbStub.Setup(
                m => m.GetTableMetadataForEntity(
                    It.IsAny<object>()
                    )    
            ).Returns(TestObjectFactory.CreateTableMetadata(typeof(TestEntity1)));

            var stateManager = new StateManager();
            stateManager.Database = dbStub.Object;
            return stateManager;
        }


        [Test]
        public void Add_calls_adds_new_EntityEntry()
        {
            var stateManager = CreateStateManager();

            var entry = stateManager.Add(new TestEntity1());

            Assert.IsTrue(entry.State == EntityState.Added);
            Assert.IsTrue(stateManager.EntryCount == 1);
        }

        [Test]
        public void Add_called_on_same_entity_throws()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();
            
            stateManager.Add(entity);

            var ex = Assert.Throws<Exception>(() => stateManager.Add(entity));
            Assert.NotNull(ex);
        }

        [Test]
        public void Add_called_on_null_entity_throws()
        {
            var stateManager = CreateStateManager();

            var ex = Assert.Throws<NullReferenceException>(() => stateManager.Add(null));

            Assert.NotNull(ex);
        }

        [Test]
        public void Update_called_on_null_throws()
        {
            var stateManager = CreateStateManager();

            var ex = Assert.Throws<NullReferenceException>(() => stateManager.Update(null));

            Assert.NotNull(ex);
            Assert.IsTrue(stateManager.EntryCount == 0);
        }

        [Test]
        public void Update_called_on_not_tracked_throws()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            var ex = Assert.Throws<Exception>(() => stateManager.Update(entity));

            Assert.NotNull(ex);
            Assert.IsTrue(stateManager.EntryCount == 0);
        }

        [Test]
        public void Update_called_on_added_not_changing_state()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            stateManager.Add(entity);
            var entry = stateManager.Update(entity);

            Assert.IsTrue(entry.State == EntityState.Added);
            Assert.IsTrue(stateManager.EntryCount == 1);
        }

        [Test]
        public void Update_called_on_unmodified_changes_state()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            var entryA = stateManager.Add(entity);
            stateManager.ApplyState(new List<EntityEntry>(){entryA}, EntityState.Unchanged);
            var entry = stateManager.Update(entity);

            Assert.IsTrue(entry.State == EntityState.Modified);
            Assert.IsTrue(stateManager.EntryCount == 1);
        }

        [Test]
        public void Remove_called_on_added_returns_detached_entry()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            var entry = stateManager.Add(entity);
            stateManager.Remove(entity);

            Assert.IsTrue(entry.State == EntityState.Detached);
        }

        [Test]
        public void Remove_called_on_unchanged_returns_deleted_entry()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            var added = stateManager.Add(entity);
            stateManager.ApplyState(stateManager.GetEntriesToSave(), EntityState.Unchanged);
            var entry = stateManager.Remove(entity);

            Assert.IsTrue(entry.State == EntityState.Deleted);
        }

        [Test]
        public void Remove_called_on_changed_returns_deleted_entry()
        {
            var stateManager = CreateStateManager();
            var entity = new TestEntity1();

            stateManager.Add(entity);
            // save in db
            stateManager.ApplyState(stateManager.GetEntriesToSave(), EntityState.Unchanged);
            stateManager.Update(entity);
            var entry = stateManager.Remove(entity);

            Assert.IsTrue(entry.State == EntityState.Deleted);
        }

        [Test]
        public void GetEntriesToSave_when_entries_are_added_returns()
        {
            var stateManager = CreateStateManager();
            var entities = new TestEntity1[3]
            {
                new TestEntity1(), new TestEntity1(), new TestEntity1()
            };

            for (int i = 0; i < 3; i++)
                stateManager.Add(entities[i]);

            var toSave = stateManager.GetEntriesToSave();
            Assert.IsTrue(toSave.Count == 3);
            foreach (var entry in toSave)
            {
                Assert.IsTrue(entry.State == EntityState.Added);
            }
        }

        [Test]
        public void GetEntriesToSave_when_entries_are_removed_returns0()
        {
            var stateManager = CreateStateManager();
            var entities = new TestEntity1[3]
            {
                new TestEntity1(), new TestEntity1(), new TestEntity1()
            };

            for (int i = 0; i < 3; i++)
                stateManager.Add(entities[i]);
            for (int i = 0; i < 3; i++)
                stateManager.Remove(entities[i]);

            var toSave = stateManager.GetEntriesToSave();
            Assert.IsTrue(toSave.Count == 0);
            foreach (var entry in toSave)
            {
                Assert.IsTrue(entry.State == EntityState.Detached);
            }
        }

        [Test]
        public void SaveChanges_when_has_entries_to_save_changes_state()
        {
            var stateManager = CreateStateManager();

            var entities = new TestEntity1[3]
            {
                new TestEntity1(), new TestEntity1(), new TestEntity1()
            };

            var entries = new EntityEntry[3];

            for (int i = 0; i < 3; i++)
                entries[i] = stateManager.Add(entities[i]);
            stateManager.Update(entities[0]);

            stateManager.SaveChanges();

            foreach (var entityEntry in entries)
            {
                Assert.IsTrue(entityEntry.State == EntityState.Unchanged);
            }
        }
    }
}
