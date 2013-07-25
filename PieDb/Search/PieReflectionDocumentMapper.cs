using System.Runtime.CompilerServices;
using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Linq.Mapping;
using Lucene.Net.Util;

namespace PieDb.Search
{
    internal class PieReflectionDocumentMapper<T> : ReflectionDocumentMapper<T>
    {
        private readonly Indexer _indexer;
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
            target.Add(new Field("__pieId", source.PieId(), Field.Store.YES, Field.Index.NOT_ANALYZED));

        }
        public override IDocumentKey ToKey(T source)
        {
            return new PieDocumentKey(source.PieId());
        }

        public override void ToObject(global::Lucene.Net.Documents.Document source, global::Lucene.Net.Linq.IQueryExecutionContext context, T target)
        {
            var id = source.GetField("__pieId").StringValue;
            target.PieId(id);
            base.ToObject(source, context, target);
        }
    }
}