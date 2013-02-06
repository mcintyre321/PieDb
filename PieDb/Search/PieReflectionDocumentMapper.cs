using System.Runtime.CompilerServices;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Linq.Mapping;
using Lucene.Net.Search;
using Lucene.Net.Util;

namespace PieDb.Search
{
    internal class PieReflectionDocumentMapper<T> : ReflectionDocumentMapper<T>
    {
        private readonly Indexer _indexer;
        ConditionalWeakTable<object, string> KeyTable = new ConditionalWeakTable<object, string>();

        public PieReflectionDocumentMapper(Version version, Indexer indexer) : base(version)
        {
            _indexer = indexer;
        }

        public PieReflectionDocumentMapper(Version version, Analyzer externalAnalyzer, Indexer indexer) : base(version, externalAnalyzer)
        {
            _indexer = indexer;
        }

        public override void ToDocument(T source, global::Lucene.Net.Documents.Document target)
        {
            base.ToDocument(source, target);
            target.Add(new Field("__pieId", source.PieId(), Field.Store.YES, Field.Index.NO));

        }
        public override IDocumentKey ToKey(T source)
        {
            return new PieDocumentKey(source.PieId());
        }

        public override void ToObject(global::Lucene.Net.Documents.Document source, global::Lucene.Net.Linq.IQueryExecutionContext context, T target)
        {
            var id = source.GetField("__pieId").StringValue;
            this._indexer.KeyTable.GetValue(target, key => id);
            base.ToObject(source, context, target);
        }
    }

    internal class PieDocumentKey : IDocumentKey
    {
        private readonly string _pieId;

        public PieDocumentKey(string pieId)
        {
            _pieId = pieId;
        }

        public bool Equals(IDocumentKey other)
        {
            var otherPie = other as PieDocumentKey;
            if (otherPie != null)
            {
                return otherPie._pieId == this._pieId;
            }
            return false;
        }


        public Query ToQuery()
        {
            return new TermQuery(new Term("__PieId", this._pieId));
        }

        public bool Empty { get; private set; }
    }
}