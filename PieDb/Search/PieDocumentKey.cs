using Lucene.Net.Index;
using Lucene.Net.Linq.Mapping;
using Lucene.Net.Search;

namespace PieDb.Search
{
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

        public bool Empty { get { return false; } }
    }
}