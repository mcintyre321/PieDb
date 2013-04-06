using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class Concurrency
    {
        private Db db;

        [SetUp]
        public void SetUp()
        {
            db = new Db();
            db.Advanced.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public void ConcurrencyExceptionIsThrownIfAnotherInstanceHasBeenSavedInTheMeantime()
        {
            var task = new Task()
            {
                Description = "milk",
            };

            db.Store(task);

            //now another session loads the task... 
            var otherTask = db.Get<Task>(task.PieId());
            otherTask.Description = "Milk";
            db.Store(otherTask, task.PieId()); //...and saves it

            Assert.Throws<ConcurrencyException>(() => db.Store(task));
        }
    }
}
