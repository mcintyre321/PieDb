using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Newtonsoft.Json;

namespace PieDb
{
    public class DataStore : IDisposable, INotifyCollectionChanged
    {
        public SerializerSettings SerializerSettings { get; private set; }
        private readonly Dictionary<string, StoredObject> _objects;

        public DataStore(SerializerSettings serializerSettings)
        {
            this.SerializerSettings = serializerSettings;
            _objects = new Dictionary<string, StoredObject>();
        }

        public void Dispose()
        {
            OnDispose();
        }

        public event Action OnDispose = () => { };
        public event NotifyCollectionChangedEventHandler CollectionChanged = (sender, args) => { };

        public StoredObject this[string pieId]
        {
            get
            {
                StoredObject storedObject;
                if (_objects.TryGetValue(pieId, out storedObject))
                {
                    return storedObject;
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException();
                }
                
                StoredObject prevValue;
                if (_objects.TryGetValue(pieId, out prevValue))
                {
                    var changeValue = JsonConvert.DeserializeObject(prevValue.SerializedObjectValue, SerializerSettings);
                    changeValue.PieId(pieId);
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changeValue));
                }

                if (value.SerializedObjectValue != null)
                {
                    var changeValue = JsonConvert.DeserializeObject(value.SerializedObjectValue, SerializerSettings);
                    changeValue.PieId(pieId);
                    CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changeValue));
                }
                _objects[pieId] = value;
            }
        }
    }
}