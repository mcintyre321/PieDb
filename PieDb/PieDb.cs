using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using Directory = System.IO.Directory;

namespace PieDb
{
    public class PieDb : IDisposable
    {
        public delegate void DbEvent(PieDocument document);

        public event DbEvent DocumentAdded = (o) => { };
        public event DbEvent DocumentUpdated = (o) => { };
        public event DbEvent DocumentRemoved = (o) => { }; 

        public string Location { get; private set; }
        public SerializerSettings SerializerSettings { get; set; }
        public AdvancedOptions Advanced { get; private set; }
        public Indexer Indexer { get; set; }
        public PieDb()
        {
            Location = Path.Combine(Helpers.GetAppDataPath(), "PieDb");
            Directory.CreateDirectory(Location);
            SerializerSettings = new SerializerSettings();
            Advanced = new AdvancedOptions(this);

            Indexer = new Indexer(this);
        }

        public void Store<T>(T obj, string id = null) where T : new()
        {
            var wasNew = false;
            var doc = obj.PieDocument(id, out wasNew);
            var json = JsonConvert.SerializeObject(doc, Formatting.Indented, SerializerSettings);
            File.WriteAllText(Path.Combine(Location, doc.Id + ".json"), json);
            if (wasNew) DocumentAdded(doc); else DocumentUpdated(doc);

        }

        public void Remove<T>(T obj)
        {
            Remove(obj.PieId());
            DocumentRemoved(obj.PieDocument(null));
        }
        public void Remove(string id)
        {
            File.Delete(Path.Combine(Location, id + ".json"));
        }

        public T Get<T>(string pieId)
        {
            try
            {
                var json = File.ReadAllText(Path.Combine(Location, pieId + ".json"));
                var obj = JsonConvert.DeserializeObject<PieDocument>(json, SerializerSettings);
                return (T) obj.Data;

            }
            catch (FileNotFoundException ex)
            {

                throw new DocumentNotFoundException(pieId, ex)
                {
                };
            }
        }

        public class AdvancedOptions
        {
            private readonly PieDb _pieDb;

            public AdvancedOptions(PieDb pieDb)
            {
                _pieDb = pieDb;
            }

            public void Clear()
            {
                Directory.Delete(_pieDb.Location, true);
                Directory.CreateDirectory(_pieDb.Location);
            }
        }

        public IQueryable<T> Query<T>(Expression<Func<T, bool>> where = null) where T : new()
        {
            return Indexer.Query<T>(where);
        }

        public void Dispose()
        {
            Indexer.Dispose();
        }

    }

    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string pieId, Exception ex):
            base("Document '" + pieId + "' not found", ex)
        {
        }
    }
}
