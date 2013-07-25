using System.Collections.Generic;
using GenericTransactionLog;

namespace PieDb
{
    public class DatabaseTransaction : GenericTransaction<DataStore>
    {
        public List<DatabaseTransactionAction> Actions { get; set; }

        public DatabaseTransaction()
        {
            Actions = new List<DatabaseTransactionAction>();
        }

        public override void Apply(DataStore target)
        {
            foreach (var databaseTransactionAction in Actions)
            {
                databaseTransactionAction.Apply(target);
            }
        }
    }
}