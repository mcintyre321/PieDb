namespace PieDb.Tests
{
    public class Task
    {
        public string Description;
        public User Creator { get; set; }
        public User Assignee { get; set; }
    }
}