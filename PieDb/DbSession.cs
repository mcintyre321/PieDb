using System.Collections;
using System.Collections.Generic;

namespace PieDb
{
    public class DbSession
    {
        Hashtable cache = new Hashtable();
        HashSet<string> dirtyKeys = new HashSet<string>(); 

        private readonly Db _db;
        object deleteMarker = new object();
        public DbSession(Db db)
        {
            _db = db;
        }

        public T Get<T>(string id)
        {
            if (cache.ContainsKey(id))
            {
                return (T) cache[id] ;
            }
            cache[id] = _db.Get<T>(id);
            return (T) cache[id];
        }

        public T TryGet<T>(string id) where T : class
        {
            if (cache.ContainsKey(id))
            {
                return (T)cache[id];
            }
            cache[id] = _db.TryGet<T>(id);
            return (T)cache[id];
        }

        public void Store<T>(T t, string id = null)
        {
            id = t.PieDocument(id).Id;
            cache[id] = t;
            
            dirtyKeys.Add(id);
        }

        public void Commit()
        {
            foreach (var dirtyKey in dirtyKeys)
            {
                object o = cache[dirtyKey];
                if (o == deleteMarker)
                {
                    _db.Remove(dirtyKey);
                }
                else
                {
                    _db.Store<object>(o, dirtyKey);
                }
            }
        }

        public void Remove(string id)
        {
            cache[id] = deleteMarker;
            dirtyKeys.Add(id);
        }
    }
}