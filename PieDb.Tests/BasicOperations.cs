using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GenericTransactionLog;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class BasicOperations
    {
        private PieDatabase _database;
        private FileTransactionStore store;

        [SetUp]
        public void SetUp()
        {
            store = new FileTransactionStore(Path.GetRandomFileName());
            _database = new PieDatabase(store);

        }
        [TearDown]
        public void TearDown()
        {
        }

        [Test]
        public void CanSaveAndLoadData()
        {
            var taskList = new TaskList()
            {
                Title = "Groceries",
                Tasks = new List<Task>() { new Task() { Description = "milk", } }
            };

            var dbSession = _database.OpenSession();
            dbSession.Store(taskList);
            dbSession.Commit();

            var loadedTaskList = dbSession.Get<TaskList>(taskList.PieId());
            Assert.AreEqual("Groceries", loadedTaskList.Title);
            Assert.AreEqual("milk", loadedTaskList.Tasks.Single().Description);
        }

        [Test]
        public void CanDetectChangedObjects()
        {
            {//seed the database with data
                var dbSession = _database.OpenSession();
                dbSession.Store(new TaskList() { Title = "Groceries" }, "tasklist");
                dbSession.Commit();
            }

            {//load the data, change it, commit it
                var newSession = _database.OpenSession();

                var loadedTaskList = newSession.Get<TaskList>("tasklist");
                loadedTaskList.Title = "Shopping";
                newSession.Commit();
            }

            { //reload the data - the changes are saved
                var anotherSession = _database.OpenSession();
                var changedTaskList = anotherSession.Get<TaskList>("tasklist");
                Assert.AreEqual("Shopping", changedTaskList.Title);
            }
        }


        [Test]
        public void CanDelete()
        {
            var user = new User() { Name = "Harry" };

            var dbSession = _database.OpenSession(); dbSession.Store(user);
            Assert.Null(_database.OpenSession().Get<User>(user.PieId()));
            dbSession.Commit();
            Assert.NotNull(_database.OpenSession().Get<User>(user.PieId()));

            dbSession.Remove(user);
            dbSession.Commit();

            Assert.Null(_database.OpenSession().Get<User>(user.PieId()));
        }


        [Test]
        public void PreservesReferences()
        {
            var user = new User() { Name = "Harry" };
            var task = new Task() { Creator = user, Assignee = user };
            var dbSession = _database.OpenSession();
            dbSession.Store(task, "asdfg");

            var loadedTask = dbSession.Get<Task>("asdfg");

            Assert.True(loadedTask.Assignee == loadedTask.Creator);
        }

        [Test]
        public void CanAutomaticallyAssignAnId()
        {
            var user = new User() { Name = "Harry" };
            var dbSession = _database.OpenSession();
            dbSession.Store(user);

            var loadedUser = dbSession.Get<User>(user.PieId());
            Assert.AreEqual("Harry", loadedUser.Name);
        }

        [Test]
        public void DoesNotWriteAllChangesToTheTransactionLog()
        {
            {//seed the database with data
                var dbSession = _database.OpenSession();
                dbSession.Store(new TaskList() { Title = "Groceries" }, "tasklist");
                dbSession.Commit();
            }
            
            Assert.AreEqual(1, NumberOfEntriesInLog());

            {//load the data, change it, commit it without changes
                var newSession = _database.OpenSession();

                newSession.Get<TaskList>("tasklist");
                newSession.Commit();
            }

            Assert.AreEqual(1, NumberOfEntriesInLog());

        }
        public int NumberOfEntriesInLog()
        {
            using (var stream = store.OpenRead())
            using (var sr = new StreamReader(stream))
            {
                var lines = sr.ReadToEnd().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToArray();
                return lines.Length;
            }
        }

    }
}
