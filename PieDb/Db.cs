using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;
using PieDb.Search;
using Directory = System.IO.Directory;

namespace PieDb
{
    public class Db : IDisposable, INotifyCollectionChanged
    {
        private string DocumentLocation;

        public delegate void DbEvent(PieDocument document);

        public string DbLocation { get; private set; }
        public SerializerSettings SerializerSettings { get; set; }
        public AdvancedOptions Advanced { get; private set; }
        public Indexer Indexer { get; set; }
        public Db()
        {
            DbLocation = Path.Combine(Helpers.GetAppDataPath(), "PieDb");
            Directory.CreateDirectory(DbLocation);
            DocumentLocation =  Path.Combine(DbLocation, "documents");
            Directory.CreateDirectory(DocumentLocation);
            SerializerSettings = new SerializerSettings();
            Advanced = new AdvancedOptions(this);

            Indexer = new Indexer(this);
        }

        public void Store<T>(T obj, string id = null)
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
            File.WriteAllText(Path.Combine(DocumentLocation, doc.Id + ".json"), json);
            if (!wasNew)
                CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, doc)); ;
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, doc));
            
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
            CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, doc)); ;
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
                throw new DocumentNotFoundException(pieId, ex) { };
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
            var json = File.ReadAllText(Path.Combine(DocumentLocation, pieId + ".json"));
            var doc = JsonConvert.DeserializeObject<PieDocument>(json, SerializerSettings);
            doc.SetDataPieDocument();
            return doc;
        }

        public class AdvancedOptions
        {
            private readonly Db _db;

            public AdvancedOptions(Db db)
            {
                _db = db;
            }

            public void Clear()
            {
                foreach (var file in Directory.GetFiles(_db.DocumentLocation))
                {
                    File.Delete(file);
                }
                _db.CollectionChanged(_db, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> where = null)
        {
            return Indexer.Query<T>(where);
        }

        public void Dispose()
        {
            Indexer.Dispose();
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => { };
    }
    public static class DbExtensions
    {
        public static T GetOrCreate<T>(this Db db, Func<T> create, string id = null) where T : class
        {
            id = id ?? typeof (T).Name;
            var t = db.TryGet<T>(id);
            if (t == null)
            {
                t = create();
                db.Store(t, id);
            }
            return t;
        }
        public static T GetOrCreate<T>(this Db db, string id = null) where T : class, new()
        {
            return GetOrCreate(db, () => new T(), id);
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
