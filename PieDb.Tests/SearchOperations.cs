using System;
using System.IO;
using System.Linq;
using System.Threading;
using GenericTransactionLog;
using NUnit.Framework;
using PieDb.Search;

namespace PieDb.Tests
{
    public class SearchOperations
    {
        private PieDb.DbSession dbSession;
        private PieDatabase database;
        private Indexer indexer;

        [SetUp]
        public void SetUp()
        {
            var serializerSettings = new SerializerSettings();

            Func<DataStore> createDataStore = () =>
            {
                var ds = new DataStore(serializerSettings);
                indexer = new Indexer(ds);
                return ds;
            };
            var store = new FileTransactionStore(Path.GetRandomFileName());
            database = new PieDatabase(store, createDataStore: createDataStore);
            dbSession = database.OpenSession();
        }

        [TearDown]
        public void TearDown()
        {
            dbSession.Dispose();
        }

        [Test]
        public void CanSearch()
        {
            var user = new User() { Name = "Harry" };
            dbSession.Store(user);
            dbSession.Commit();

            var user2 = this.indexer.Query<User>(dbSession, u => u.Name == "Harry").Single();
            

            Assert.AreEqual("Harry", user2.Name);
            Assert.AreEqual(user.PieId(), user2.PieId(), "If these are different, user2 has not been loaded from the database (it has just been rebuilt by the lucene search)");
        }
        [Test]
        public void AnObjectSavedTwiceIsFoundOnce()
        {
            var user = new User() { Name = "Harry" };
            dbSession.Store(user);
            dbSession.Store(user);
            dbSession.Commit();

            var user2 = indexer.Query<User>(dbSession, u => u.Name == "Harry").Single();
        }


        [Test]
        public void CanSearchOnUpdatedData()
        {
            var user = new User() { Name = "Harry" };
            dbSession.Store(user);
            dbSession.Commit();

            var user2 = indexer.Query<User>(dbSession, u => u.Name == "Harry").Single();
            user2.Name = "Tom";
            dbSession.Store(user2);
            dbSession.Commit();


            var enumerable = indexer.Query<User>(dbSession, u => u.Name == "Harry").ToArray();
            Assert.Null(enumerable.SingleOrDefault());

            var foundUsingNewName = indexer.Query<User>(dbSession, u => u.Name == "Tom").Single();
            Assert.AreEqual("Tom", foundUsingNewName.Name);
        }

        [Test]
        public void CantFindDeletedData()
        {
            var user = new User() { Name = "Harry" };
            dbSession.Store(user);
            dbSession.Commit();

            Assert.True(indexer.Query<User>(dbSession, u => u.Name == "Harry").Any());
            
            dbSession.Remove(user);
            dbSession.Commit();

            var enumerable = indexer.Query<User>(dbSession, u => u.Name == "Harry").ToArray();
            Assert.False(enumerable.Any());
        }
    }
}