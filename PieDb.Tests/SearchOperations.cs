using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace PieDb.Tests
{
    public class SearchOperations
    {
        private PieDb db;

        [SetUp]
        public void SetUp()
        {
            db = new PieDb();
            db.Advanced.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            db.Dispose();
        }

        [Test]
        public void CanSearch()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);
            var user2 = db.Query<User>(u => u.Name == "Harry").Single();
            Assert.AreEqual("Harry", user2.Name);
            Assert.AreEqual(user.PieId(), user2.PieId(), "If these are different, user2 has not been loaded from the database (it has just been rebuilt by the lucene search)");
        }

        [Test]
        public void CanSearchOnUpdatedData()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);

            var user2 = db.Query<User>(u => u.Name == "Harry").Single();
            user2.Name = "Tom";
            db.Store(user);

            var enumerable = db.Query<User>(u => u.Name == "Harry").ToArray();
            Assert.Null(enumerable.SingleOrDefault());

            var foundUsingNewName = db.Query<User>(u => u.Name == "Tom").Single();
            Assert.AreEqual("Tom", foundUsingNewName.Name);
        }

        [Test]
        public void CantFindDeletedData()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);
            db.Remove(user);

            var enumerable = db.Query<User>(u => u.Name == "Harry").ToArray();
            Assert.False(enumerable.Any());
        }
    }
}