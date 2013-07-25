using System;
using Newtonsoft.Json;

namespace PieDb
{
    public class SaveDocumentTransactionAction : DatabaseTransactionAction
    {
        public string Id { get; set; }
        public string Doc { get; set; }

        public SaveDocumentTransactionAction(string id, string doc)
        {
            Id = id;
            Doc = doc;
        }

        public override void Apply(DataStore target)
        {
            
            target[Id] = new StoredObject(Doc, Guid.NewGuid().ToString());
        }
    }
}