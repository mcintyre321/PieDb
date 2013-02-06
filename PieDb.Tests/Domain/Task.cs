using Lucene.Net.Linq.Mapping;

namespace PieDb.Tests
{
    public class Task
    {
        public string Description;
        [IgnoreField]
        public User Creator { get; set; }
        [IgnoreField]
        public User Assignee { get; set; }
    }
}