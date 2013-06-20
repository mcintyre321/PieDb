using System;

namespace PieDb
{
    public static class DbExtensions
    {
        public static T GetOrCreate<T>(this Db db, Func<T> create, string id = null) where T : class
        {
            id = id ?? typeof (T).Name;
            var t = db.TryGet<T>(id);
            if (t == null)
            {
                t = create();
                db.Store(t, id);
            }
            return t;
        }
        public static T GetOrCreate<T>(this Db db, string id = null) where T : class, new()
        {
            return GetOrCreate(db, () => new T(), id);
        }
    }
}