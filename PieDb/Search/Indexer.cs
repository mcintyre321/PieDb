using System;
using System.Collections.Generic;
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
        private readonly PieDb _pieDb;
        private LuceneDataProvider _provider;
        private DirectoryInfo directoryInfo;
        public string Location { get; set; }
        internal ConditionalWeakTable<object, string> KeyTable = new ConditionalWeakTable<object, string>();
        public Indexer(PieDb pieDb)
        {
            _pieDb = pieDb;
            _pieDb.DocumentAdded += AddDocumentToIndex;
            _pieDb.DocumentRemoved += RemoveDocumentFromIndex;
            _pieDb.DocumentUpdated += UpdateDocumentInIndex;
            _pieDb.Clearing += DeleteIndexFilesAndClearIndex;
            _pieDb.Cleared += Reinitialise;

            Location = Path.Combine(_pieDb.Location, "Indexes");
            directoryInfo = new DirectoryInfo(Location);
            directoryInfo.Create();
            //_provider = new LuceneDataProvider(new MMapDirectory(directoryInfo), Version.LUCENE_30);
            _provider = new LuceneDataProvider(new RAMDirectory(), Version.LUCENE_30);
        }

        private void AddDocumentToIndex(PieDocument document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance| BindingFlags.NonPublic)
                .Single(m => m.Name == "AddDocumentToIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.Data.GetType());
            generic.Invoke(this, new[]{document});
        }
        private void AddDocumentToIndex<T>(PieDocument document) where T : new()
        {
            using (var session = _provider.OpenSession<T>(new PieReflectionDocumentMapper<T>(Version.LUCENE_30, this)))
            {
                session.Add((T)document.Data);
            }
        }


        private void UpdateDocumentInIndex(PieDocument document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "UpdateDocumentInIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.Data.GetType());
            generic.Invoke(this, new[] { document });
        }
        private void UpdateDocumentInIndex<T>(PieDocument document)
        {

        }

        private void RemoveDocumentFromIndex(PieDocument document)
        {
            var generic = this.GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(m => m.Name == "RemoveDocumentFromIndex" && m.IsGenericMethodDefinition)
                .MakeGenericMethod(document.Data.GetType());
            generic.Invoke(this, new[] { document });
        }
        private void RemoveDocumentFromIndex<T>(PieDocument document) where T : new()
        {
            using (var session = _provider.OpenSession<T>(new PieReflectionDocumentMapper<T>((Version) Version.LUCENE_30, this)))
            {
                session.Delete((T)document.Data);
            }
        }

        void DeleteIndexFilesAndClearIndex(object sender, EventArgs e)
        {
            _provider.Dispose();
            directoryInfo.Delete(true);
        }

        void Reinitialise(object sender, EventArgs e)
        {
            directoryInfo.Create();
            _provider = new LuceneDataProvider(new MMapDirectory(directoryInfo), Version.LUCENE_30);
        }


        public IEnumerable<T> Query<T>(Expression<Func<T, bool>> @where) where T : new()
        {
            var indexQ = _provider.AsQueryable<T>(new PieReflectionDocumentMapper<T>(Version.LUCENE_30, this));
            if (@where != null) indexQ = indexQ.Where(where);
            return indexQ.
                Select(item => GetValue(item))
                .Select(id => _pieDb.Get<T>(id));
        }

        private string GetValue<T>(T item) where T : new()
        {
            return this.KeyTable.GetValue(item, c=> "asd" );
        }

        public void Dispose()
        {
            _provider.Dispose();
        }
    }
}