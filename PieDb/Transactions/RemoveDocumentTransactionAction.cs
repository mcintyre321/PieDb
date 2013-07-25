using System;

namespace PieDb
{
    public class RemoveDocumentTransactionAction : DatabaseTransactionAction
    {
        
        public string Id { get; set; }

        public RemoveDocumentTransactionAction(string id)
        {
            Id = id;
        }

        public override void Apply(DataStore target)
        {
            target[Id] = new StoredObject(null, Guid.NewGuid().ToString());
        }
         
    }
}