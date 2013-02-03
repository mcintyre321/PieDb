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
                Tasks = new List<Task>() { new Task() { Description = "milk", } }
            };

            db.Store(taskList);

            var loadedTaskList = db.Get<TaskList>(taskList.PieId());
            Assert.AreEqual("Groceries", loadedTaskList.Title);
            Assert.AreEqual("milk", loadedTaskList.Tasks.Single().Description);
        }

        [Test]
        public void CanDelete()
        {
            var user = new User() { Name = "Harry" };
             
            db.Store(user);
            db.Remove(user);
            Assert.Throws<DocumentNotFoundException>(() => db.Get<User>(user.PieId()));
        }


        [Test]
        public void PreservesReferences()
        {
            var user = new User() { Name = "Harry" };
            var task = new Task() { Creator = user, Assignee = user };
            db.Store(task, "asdfg");

            var loadedTask = db.Get<Task>("asdfg");

            Assert.True(loadedTask.Assignee == loadedTask.Creator);
        }

        [Test]
        public void CanAutomaticallyAssignAnId()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);

            var loadedUser = db.Get<User>(user.PieId());
            Assert.AreEqual("Harry", loadedUser.Name);
        }
    }
}
