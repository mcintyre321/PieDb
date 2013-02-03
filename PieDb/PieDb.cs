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
        public event EventHandler Clearing = (sender, args) => { };
        public event EventHandler Cleared = (sender, args) => { };

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
            var doc = obj.PieDocument(id);
            doc.Deleted = false;
            SaveDocument(doc);
        }

        private void SaveDocument(PieDocument doc)
        {
            bool wasNew = false;
            try
            {
                var prev = Get(doc.Id);
                var prevETag = prev.PieDocument(null).ETag;
                if (prevETag != doc.ETag)
                {
                    throw new ConcurrencyException();
                }
                doc.ETag = Guid.NewGuid().ToString();
            }
            catch (DocumentNotFoundException ex)
            {
                wasNew = true;
            }
            var json = JsonConvert.SerializeObject(doc, Formatting.Indented, SerializerSettings);
            File.WriteAllText(Path.Combine(Location, doc.Id + ".json"), json);
            if (wasNew) DocumentAdded(doc);
            else DocumentUpdated(doc);
        }

        public void Remove<T>(T obj)
        {
            Remove(obj.PieId());
        }
        public void Remove(string id)
        {
            var doc = GetPieDocument(id);
            doc.Deleted = true;
            SaveDocument(doc);
            DocumentRemoved(doc);

        }

        public object Get(string pieId)
        {
            try
            {
                var obj = GetPieDocument(pieId);
                if (obj.Deleted) throw new DocumentNotFoundException(pieId, null);
                return obj.Data;
            }
            catch (FileNotFoundException ex)
            {
                throw new DocumentNotFoundException(pieId, ex)
                {
                };
            }
        }

        public T Get<T>(string pieId)
        {
            return (T)Get(pieId);
        }

        public T TryGet<T>(string pieId) where T : class
        {
            try
            {
                return Get<T>(pieId);
            }
            catch (DocumentNotFoundException)
            {
                return null;
            }
        }

        private PieDocument GetPieDocument(string pieId)
        {
            var json = File.ReadAllText(Path.Combine(Location, pieId + ".json"));
            var doc = JsonConvert.DeserializeObject<PieDocument>(json, SerializerSettings);
            doc.SetDataPieDocument();
            return doc;
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
                _pieDb.Clearing(this, EventArgs.Empty);
                Directory.Delete(_pieDb.Location, true);
                Directory.CreateDirectory(_pieDb.Location);
                _pieDb.Cleared(this, EventArgs.Empty);
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


    public class ConcurrencyException : Exception
    {
    }

    public class DocumentNotFoundException : Exception
    {
        public DocumentNotFoundException(string pieId, Exception ex) :
            base("Document '" + pieId + "' not found", ex)
        {
        }
    }
}
