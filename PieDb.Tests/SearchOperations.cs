using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using PieDb.Search;

namespace PieDb.Tests
{
    public class SearchOperations
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
        public void CanSearch()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);
            var user2 = db.Query<User>(u => u.Name == "Harry").Single();
            Assert.AreEqual("Harry", user2.Name);
            Assert.AreEqual(user.PieId(), user2.PieId(), "If these are different, user2 has not been loaded from the database (it has just been rebuilt by the lucene search)");
        }
        [Test]
        public void AnObjectSavedTwiceIsFoundOnce()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);
            db.Store(user);
            var user2 = db.Query<User>(u => u.Name == "Harry").Single();
        }


        [Test]
        public void CanSearchOnUpdatedData()
        {
            var user = new User() { Name = "Harry" };
            db.Store(user);

            var user2 = db.Query<User>(u => u.Name == "Harry").Single();
            user2.Name = "Tom";
            db.Store(user2);

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
            Assert.True(db.Query<User>(u => u.Name == "Harry").Any());
            
            db.Remove(user);

            var enumerable = db.Query<User>(u => u.Name == "Harry").ToArray();
            Assert.False(enumerable.Any());
        }
    }
}