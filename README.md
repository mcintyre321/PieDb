PieDb
=====

The minimalist embedded .net NOSQL database that is as easy-as-pie to use.

1. XCOPY deployable 
2. Transparent - you don't need Id properties on your objects*.
2. It's MIT licenced
3. It has a very tiny codebase.
4. It uses a JSON transaction log to record changes (so your objects need to be Json serializable)
5. Transactional sessions (aka Unit Of Work) with optimistic concurrency

*this is due to cunning use of the awesome [ConditionalWeakTable](http://msdn.microsoft.com/en-us/library/dd287757.aspx) class. Know it, use it!

Installation:

> Install-Package PieDb


Creating a database

      //create a single database instance for 
      var db = new PieDatabase(); //defaults to saving changes to /App_Data/piedb.transactions,  but you can pass in your own FileTransactionStore or InMemoryTransactionStore
      
Working with data
      
      var session = db.OpenSession(); //get a 'unit of work'
      
      var someObject = ...; //your object
      
      //storing an object with a random id
      session.Store(someObject);
      
      //get the id - note, the object doesn't need a .Id property
      string id = someObject.PieId()
      
      //storing an object with a known id
      session.Store(someObject, "groceries");
      
      //removing an object
      session.Remove<TSomeObject>(someObject);
      
      //removing an object by id
      session.Remove(someId);

      //flush the changes you have made in your session
      db.Commit();
 

Inside a PieDatabase is a DataStore object, which is a collection of all the saved objects, held in memory, fast but memory-hungry.

When the database starts up, the DataStore is rebuilt from the transaction log. The DataStore implements 
INotifyCollectionChanged, which is called with the changes made by a session.Commit() call.

This means you can add a listener which indexes the DataStore as it rebuilds, allowing different indexing 
libraries and approaches to be used, and allowing realtime changes to be published to consumers. See the SearchTests
for an example using the Lucene.Net.Linq library, which allows querying as below:

      //LINQ querying (see Lucene.Net.Linq (https://github.com/themotleyfool/Lucene.Net.Linq)) for info
      var results = sessopm.Query<User>(t => t.Email == "mcintyre321@gmail.com").ToArray();

Why "PieDb?" Many of my other projects are named after foods and it's short.

