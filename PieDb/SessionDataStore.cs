using System;
using System.Collections.Generic;
using GenericTransactionLog;
using Newtonsoft.Json;

namespace PieDb
{
    public class SessionDataStore
    {
        private readonly Func<DataStore> _getDataStore;
        private DataStore _dataStore;
        DataStore DataStore { get { return _dataStore ?? (_dataStore = _getDataStore()); } }

        public SessionDataStore(Func<DataStore> getDataStore)
        {
            _getDataStore = getDataStore;
        }

        readonly Dictionary<string, CachedObject> _cachedObjectGetters = new Dictionary<string, CachedObject>();
        

        public object Get(string pieId)
        {
            if (_cachedObjectGetters.ContainsKey(pieId))
            {
                var storedObject = _cachedObjectGetters[pieId];
                if (storedObject != null)
                {
                    return storedObject.Value;
                }
                else return null;
            }
            else
            {
                var storedObject = DataStore[pieId] ?? new StoredObject(null, Guid.NewGuid().ToString());

                object value = null;
                if (storedObject.SerializedObjectValue != null)
                {
                    value = JsonConvert.DeserializeObject(storedObject.SerializedObjectValue,
                                                          DataStore.SerializerSettings);
                    value.PieId(pieId);
                }

                var cachedObject = new CachedObject(value, storedObject.ETag);
                _cachedObjectGetters[pieId] = cachedObject;
                return cachedObject.Value;
            }
            
        }

        public void Store(object o, string pieId)
        {
            if (_cachedObjectGetters.ContainsKey(pieId))
            {
                var currentCachedObject = _cachedObjectGetters[pieId];
                
                _cachedObjectGetters[pieId] = new CachedObject(o, currentCachedObject.ETag);
            }
            else
            {
                _cachedObjectGetters[pieId] = new CachedObject(o, Guid.NewGuid().ToString());
            }
            
            
        }

        public void Remove(string id)
        {
            var current = Get(id);
            var currentCachedObject = _cachedObjectGetters[id];
            _cachedObjectGetters[id] = new CachedObject(null, currentCachedObject.ETag);
        }

        internal IEnumerable<DatabaseTransactionAction> GetActions()
        {
            foreach (var pair in _cachedObjectGetters)
            {
                var cachedValue = pair.Value;
                var storedValue = DataStore[pair.Key] ?? new StoredObject(null, cachedValue.ETag);
                if (cachedValue.ETag != storedValue.ETag)
                {
                    throw new ConcurrencyException("E-tag differs for id " + pair.Key);
                }

                if (cachedValue.Value == null)
                {
                    yield return new RemoveDocumentTransactionAction(pair.Key);
                }
                else
                {
                    var doc = JsonConvert.SerializeObject(cachedValue.Value, DataStore.SerializerSettings);
                    if (doc != storedValue.SerializedObjectValue)
                    {
                        yield return new SaveDocumentTransactionAction(pair.Key, doc);
                    }
                }
            }
        }
    }
}