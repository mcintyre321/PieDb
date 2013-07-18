using System;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class SessionTests
    {
        private Db db;

        [SetUp]
        public void SetUp()
        {
            db = new Db();
            db.Advanced.Clear();
            var task = new Task()
            {
                Description = "milk",
            };

            db.Store(task, "task");
        }
        [TearDown]
        public void TearDown()
        {
            db.Advanced.Clear();
            db.Dispose();
        }

        [Test]
        public void WillNotDirtyRead()
        {
            var session = new DbSession(db);
            var task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);

            {
                var taskLoadedDirectly = db.Get<Task>("task");
                taskLoadedDirectly.Description = "glue";
                db.Store<Task>(taskLoadedDirectly, "task");
            }

            task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);
        }

        [Test]
        public void WillNotNonRepeatableRead()
        {
            var session = new DbSession(db);
            var task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);

            {
                db.Remove("task");
            }

            task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);
        }
        [Test]
        public void WillNotPhantomRead()
        {
            var session = new DbSession(db);
            var exceptionThrown = false;
            try
            {
                var task = session.Get<Task>("notask");
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
            {
                db.Store(new Task(), "notask");
            }

            try
            {
                var task = session.Get<Task>("notask");
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert.True(exceptionThrown);
        }

        [Test]
        public void WillNotPhantomTryRead()
        {
            var session = new DbSession(db);
            var task = session.TryGet<Task>("notask");
            Assert.Null(task);

            {
                db.Store(new Task(), "notask");
            }

            task = session.TryGet<Task>("notask");
            Assert.Null(task);
        }

        [Test]
        public void ChangesAreNotSavedUntilCommit()
        {
            var session = new DbSession(db);
            var task = session.Get<Task>("task");
            task.Description = "glue";
            session.Store(task);

            Assert.AreEqual("milk", db.Get<Task>("task").Description);

            session.Commit();

            Assert.AreEqual("glue", db.Get<Task>("task").Description);
        }

        [Test]
        public void DeletesAreNotSavedUntilCommit()
        {
            var session = new DbSession(db);
            session.Remove("task");

            Assert.AreEqual("milk", db.Get<Task>("task").Description);

            session.Commit();
            try
            {
                var task = db.Get<Task>("task");
                Assert.Fail("Should not reach here as task is deleted");
            }
            catch (Exception)
            {

            }
        }

    }
}
