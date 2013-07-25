using System;
using System.IO;
using System.Linq;
using System.Text;
using GenericTransactionLog;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class SessionTests
    {
        private PieDatabase pieDatabase;

        [SetUp]
        public void SetUp()
        {
            var store = new FileTransactionStore(Path.GetRandomFileName());
            pieDatabase = new PieDatabase(store);
            var session = pieDatabase.OpenSession();
            var task = new Task()
            {
                Description = "milk",
            };

            session.Store(task, "task");
            session.Commit();
        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void WillNotDirtyRead()
        {
            var session = pieDatabase.OpenSession();
            var task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);

            {
                var otherSession = pieDatabase.OpenSession();
                var taskLoadedInOtherSession = otherSession.Get<Task>("task");
                taskLoadedInOtherSession.Description = "glue";
                otherSession.Store<Task>(taskLoadedInOtherSession, "task");
                otherSession.Commit();
            }

            task = session.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);
        }

        [Test]
        public void WillNotNonRepeatableRead()
        {
            var firstSession = pieDatabase.OpenSession();
            var task = firstSession.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);

            {
                var otherSession = pieDatabase.OpenSession();
                otherSession.Remove("task");
                otherSession.Commit();
            }

            task = firstSession.Get<Task>("task");
            Assert.AreEqual("milk", task.Description);

            task.Description = "setting desc on removed task";
            Assert.Throws<ConcurrencyException>(firstSession.Commit);
        }
        

        [Test]
        public void WillNotPhantomTryRead()
        {
            var session = pieDatabase.OpenSession();
            var task = session.Get<Task>("notask");

            {
                var otherSession = pieDatabase.OpenSession();
                otherSession.Store(new Task(), "notask");
                otherSession.Commit();
            }

            task = session.Get<Task>("notask");
            Assert.Null(task);
        }

        [Test]
        public void ChangesAreNotSavedUntilCommit()
        {
            var session = pieDatabase.OpenSession();
            var task = session.Get<Task>("task");
            task.Description = "glue";
            session.Store(task);

            Assert.AreEqual("milk", pieDatabase.OpenSession().Get<Task>("task").Description);

            session.Commit();

            Assert.AreEqual("glue", pieDatabase.OpenSession().Get<Task>("task").Description);
        }

        [Test]
        public void DeletesAreNotSavedUntilCommit()
        {
            var session = pieDatabase.OpenSession();
            session.Remove("task");

            Assert.AreEqual("milk", pieDatabase.OpenSession().Get<Task>("task").Description);

            session.Commit();


            try
            {
                Assert.Null(pieDatabase.OpenSession().Get<Task>("task").Description);
                Assert.Fail("Should not reach here as task is deleted");
            }
            catch (Exception)
            {

            }
        }

    }
}
