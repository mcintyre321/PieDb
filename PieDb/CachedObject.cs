namespace PieDb
{
    public class CachedObject
    {
        public CachedObject(object value, string etag)
        {
            Value = value;
            ETag = etag;
        }

        public object Value { get; set; }
        public string ETag { get; set; }
    }
}