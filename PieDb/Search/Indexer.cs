using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Lucene.Net.Linq;
using Lucene.Net.Store;
using Version = Lucene.Net.Util.Version;

namespace PieDb.Search
{
    public class Indexer
    {
        private readonly DataStore _dataStore;
        private LuceneDataProvider _provider;
        private Func<Type, object> Factory { get; set; }
        private Func<T> TypedFactory<T>()
        {
            return () => (T)Factory(typeof(T));
        }

        public Indexer(DataStore dataStore)
        {
            _dataStore = dataStore;

            _provider = new LuceneDataProvider(new RAMDirectory(), Version.LUCENE_30);

            _dataStore.OnDispose += this.Dispose;
            Factory = Activator.CreateInstance;



            dataStore.CollectionChanged += (sender, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        AddDocumentToIndex(args.NewItems.Cast<object>().Single());
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        RemoveDocumentFromIndex(args.OldItems.Cast<object>().Single());
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        DeleteIndexFilesAndClearIndex(this, args);
                        Reinitialise(this, args);
                        break;
                    default:
                        throw new NotImplementedException(args.ToString() + " not expected");
                }
            };
        }

        private void AddDocumentToIndex(object document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "AddDocumentToIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.GetType());
            generic.Invoke(this, new[] { document });
        }
        private void AddDocumentToIndex<T>(object document)
        {
            using (var session = _provider.OpenSession<T>(TypedFactory<T>(), new PieReflectionDocumentMapper<T>(Version.LUCENE_30, this)))
            {
                session.Add((T)document);
            }
        }


        private void UpdateDocumentInIndex(object document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "UpdateDocumentInIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.GetType());
            generic.Invoke(this, new[] { document });
        }
        private void UpdateDocumentInIndex<T>(object document)
        {
            using (var session = _provider.OpenSession<T>(TypedFactory<T>(), new PieReflectionDocumentMapper<T>((Version)Version.LUCENE_30, this)))
            {
                session.Delete((T)document);
            }
            using (var session = _provider.OpenSession(TypedFactory<T>(), new PieReflectionDocumentMapper<T>((Version)Version.LUCENE_30, this)))
            {
                session.Add((T)document);
            }
        }

        private void RemoveDocumentFromIndex(object document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "RemoveDocumentFromIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.GetType());
            generic.Invoke(this, new[] { document });
        }
        private void RemoveDocumentFromIndex<T>(object document)
        {
            using (var session = _provider.OpenSession<T>(TypedFactory<T>(), new PieReflectionDocumentMapper<T>((Version)Version.LUCENE_30, this)))
            {
                session.Delete((T)document);
            }
        }

        void DeleteIndexFilesAndClearIndex(object sender, EventArgs e)
        {
            _provider.Dispose();
        }

        void Reinitialise(object sender, EventArgs e)
        {
            _provider = new LuceneDataProvider(new RAMDirectory(), Version.LUCENE_30);
        }


        public IEnumerable<T> Query<T>(DbSession session, Expression<Func<T, bool>> @where = null)
        {
            using (var s = _provider.OpenSession<T>(TypedFactory<T>(), new PieReflectionDocumentMapper<T>(Version.LUCENE_30, this)))
            {
                var indexQ = s.Query();
                if (@where != null) indexQ = indexQ.Where(where);
                return indexQ.
                    Select(item => GetValue(item))
                    .Select(id => session.Get<T>(id));
            }
        }

        private string GetValue<T>(T item)
        {
            return item.PieId();
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}