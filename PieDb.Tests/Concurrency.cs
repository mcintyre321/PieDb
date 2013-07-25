using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericTransactionLog;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class Concurrency
    {
        private PieDatabase pieDatabase;

        [SetUp]
        public void SetUp()
        {
            var store = new FileTransactionStore(Path.GetRandomFileName());
            pieDatabase = new PieDatabase(store);
            using (var dbSession = pieDatabase.OpenSession())
            {
                var task = new Task()
                {
                    Description = "milk",
                };

                dbSession.Store(task, "sometask");
                dbSession.Commit();
            }
        }

        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CannotSaveAnUnversionedTaskOverAVersionedOne()
        {
            
            var otherSession = pieDatabase.OpenSession();
            var otherTask = new Task() {Description = "shoes"};

            otherSession.Store(otherTask, "sometask"); //...and saves it

            Assert.Throws<ConcurrencyException>(otherSession.Commit);
        }

        [Test]
        public void CannotSaveAnIncorrectlyVersionedTask()
        {

            var firstSession = pieDatabase.OpenSession();
            var firstSessionsTask = firstSession.Get<Task>("sometask");
            firstSessionsTask.Description = "gorilla";

            var secondSession = pieDatabase.OpenSession();
            var secondSessionsTask = secondSession.Get<Task>("sometask");
            secondSessionsTask.Description = "monkeys";
            secondSession.Commit();

            Assert.Throws<ConcurrencyException>(firstSession.Commit);
        }
    }
}
