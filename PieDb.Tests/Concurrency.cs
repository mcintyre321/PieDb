using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class Concurrency
    {
        private PieDb db;

        [SetUp]
        public void SetUp()
        {
            db = new PieDb();
            db.Advanced.Clear();
        }

        [Test]
        public void ExceptionIsThrownIfOtherChangesMade()
        {
            var task = new Task()
            {
                Description = "milk",
            };

            db.Store(task);

            var task2 = db.Get<Task>(task.PieId());
            task2.Description = "Milk";
            db.Store(task2, task.PieId());

            Assert.Throws<ConcurrencyException>(() => db.Store(task));


        }
    }
}
