using System.Collections.Generic;
using Lucene.Net.Linq.Mapping;

namespace PieDb.Tests
{
    public class TaskList
    {
        public TaskList()
        {
            Tasks = new List<Task>();
        }

        
        public string Title { get; set; }
        [IgnoreField]
        public IList<Task> Tasks { get; set; } 
    }
}