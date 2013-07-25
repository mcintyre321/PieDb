using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using GenericTransactionLog;
using PieDb.Search;
using Directory = System.IO.Directory;

namespace PieDb
{
    public class DbSession : IDisposable //, INotifyCollectionChanged
    {
        private readonly TransactionLog<DataStore> _log;
        private SessionDataStore _sessionDataStore;

        public DbSession(TransactionLog<DataStore> log)
        {
            _log = log;
            _sessionDataStore = new SessionDataStore(() => _log.Value);

        }

        public void Store<T>(T obj, string id = null)
        {
            _sessionDataStore.Store(obj, obj.PieId(id));
        }

        public void Remove<T>(T obj)
        {
            Remove(obj.PieId());
        }

        public void Remove(string id)
        {
            _sessionDataStore.Remove(id);
        }

        public object Get(string pieId)
        {
            return _sessionDataStore.Get(pieId);
        }

        public T Get<T>(string pieId)
        {
            return (T) Get(pieId);
        }

        public void Dispose()
        {

        }


        public void Commit()
        {
            var transaction = new DatabaseTransaction();
            transaction.Actions.AddRange(_sessionDataStore.GetActions());
            if (transaction.Actions.Any())
            {
                _log.LogAndApplyTransaction(transaction);
            }
            _sessionDataStore = new SessionDataStore(() => _log.Value);
        }
    }
}
