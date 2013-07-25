using System;
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericTransactionLog;
using GenericTransactionLog.Currying;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PieDb.Search;

namespace PieDb
{
    public class PieDatabase
    {
        private TransactionLog<DataStore> _transactionLog;
        protected Lazy<DataStore> DataStore { get; set; }

        public PieDatabase(TransactionStore store, SerializerSettings settings = null, Func<DataStore> createDataStore = null)
        {
            settings = settings ?? new SerializerSettings();
            createDataStore = createDataStore ?? (() => new DataStore(settings));

            var writeTransaction = new Func<GenericTransaction<DataStore>, string>(
                tran => JsonConvert.SerializeObject(tran, settings))
                .Then(Funcs.AppendStringToStream);

            var readTransactions = Funcs.StreamToStreamReader.Then(Funcs.ReadStringsFromStreamReader)
                                        .ThenForEach(
                                            str =>
                                            JsonConvert.DeserializeObject<GenericTransaction<DataStore>>(str, settings));



            _transactionLog = new TransactionLog<DataStore>(createDataStore, writeTransaction, readTransactions, store);
            DataStore = new Lazy<DataStore>(() => _transactionLog.Value);
        }


        public DbSession OpenSession()
        {
            return new DbSession(_transactionLog);
        }
    }
}
