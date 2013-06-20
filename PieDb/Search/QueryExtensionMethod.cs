using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;

namespace PieDb.Search
{
    public static class QueryExtensionMethod
    {
        static ConditionalWeakTable<Db, Indexer> lookup = new ConditionalWeakTable<Db, Indexer>();
        public static Indexer Indexer(this PieDb.Db db)
        {
            return lookup.GetValue(db, CreateIndexer);
        }

        public static IEnumerable<T> Query<T>(this PieDb.Db db, Expression<Func<T, bool>> query)
        {
            return db.Indexer().Query(query);
        }

        private static Indexer CreateIndexer(Db db)
        {
            return new Indexer(db);
        }
    }
}
