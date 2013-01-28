using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class BasicOperations
    {
        private PieDb db;

        [SetUp]
        public void SetUp()
        {
            db = new PieDb();
            db.Advanced.Clear();
        }

        [Test]
        public void CanSaveAndLoadData()
        {
            var taskList = new TaskList()
            {
                Title = "Groceries",
                Tasks = new List<Task>()
                {
                   
                    new Task()
                    {
                        Description = "milk",
                    }
                }
            };

            db.Store(taskList);

            var taskList2 = db.Get<TaskList>(taskList.PieId());
            Assert.AreEqual("Groceries", taskList2.Title);
            Assert.AreEqual("milk", taskList2.Tasks.Single().Description);
        }
        [Test]
        public void PreservesReferences()
        {
            var user = new User() {Name = "Harry"};
            var task = new Task()
            {
                Creator = user,
                Assignee = user
            };
            db.Store(task);

            var task2 = db.Get<Task>(task.PieId());
            Assert.AreEqual("Harry", task2.Assignee.Name);
            Assert.True(task2.Assignee == task2.Creator);
        }

        [Test]
        public void CanAssignAnId()
        {
            var user = new User() { Name = "Harry" };
            var task = new Task()
            {
                Creator = user,
                Assignee = user
            };
            db.Store(task, "asdfg");

            var task2 = db.Get<Task>("asdfg");
            Assert.AreEqual("Harry", task2.Assignee.Name);
            Assert.True(task2.Assignee == task2.Creator);
        }
    }
}
