using System.Collections.Generic;

namespace PieDb.Tests
{
    public class TaskList
    {
        public TaskList()
        {
            Tasks = new List<Task>();
        }

        public string Title { get; set; }
        public IList<Task> Tasks { get; set; } 
    }
}