namespace PieDb
{
    public class PieDocument
    {
        public object Data { get; set; }

        public string Id { get; set; }

        public string ETag { get; set; }

        public bool Deleted { get; set; }
    }
}