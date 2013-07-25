namespace PieDb
{
    public class StoredObject
    {
        public StoredObject(string serializedObjectValue, string etag)
        {
            SerializedObjectValue = serializedObjectValue;
            ETag = etag;
        }

        public string SerializedObjectValue { get; private set; }
        public string ETag { get; set; }

        
    }
}