using System.Linq;
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

        public void CanSearch()
        {
            var user = new User() {Name = "Harry"};
            db.Store(user);
            var users = db.Query<User>(u => u.Name == "Harry").Single();

        }
    }
}