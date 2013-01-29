using System;
using System.Runtime.CompilerServices;

namespace PieDb
{
    public static class PieIdExtension
    {
        static ConditionalWeakTable<object, PieDocument> KeyTable = new ConditionalWeakTable<object, PieDocument>();

        public static PieDocument PieDocument(this object obj, string id = null)
        {
            var hadToCreate = false;
            return PieDocument(obj, id, out hadToCreate);
        } 
        public static PieDocument PieDocument(this object obj, string id, out bool hadToCreate)
        {
            hadToCreate = false;
            bool createdDoc = false;
            var doc = KeyTable.GetValue(obj, o =>
            {
                createdDoc = true;
                return new PieDocument()
                {
                    Data = o,
                    ETag = Guid.NewGuid().ToString(),
                    Id = id ?? Guid.NewGuid().ToString()
                };
            });
            hadToCreate = createdDoc;
            return doc;
        } 


        public static string PieId(this object obj)
        {
            return obj.PieDocument(null as string).Id;
        }

        public static void SetDataPieDocument(this PieDocument doc)
        {
            KeyTable.GetValue(doc.Data, o => doc);
        }
    }
}