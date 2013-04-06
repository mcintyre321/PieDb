PieDb
=====

The minimalist embedded .net NOSQL database that is as easy-as-pie. 

It's a bit like RavenDb, but with far fewer features, aside from:

1. It's completely transparent - you don't even need Id properties on your objects*.
2. It's completely licence free!
3. It has a very tiny codebase.

*this is due to cunning use of the awesome [ConditionalWeakTable](http://msdn.microsoft.com/en-us/library/dd287757.aspx) class. Know it, use it!


> Install-Package PieDb

      var db = new PieDb.Db();
      var someObject = ...; //your object
      
      //storing an object with a random id
      db.Store(someObject);
      
      //get the id - note, the object doesn't need a .Id property
      string id = someObject.PieId()
      
      //storing an object with a known id
      db.Store(someObject, "groceries");
      
      //removing an object
      db.Remove<TSomeObject>(someObject)
      
      //removing an object by id
      db.Remove(someId)

      //LINQ querying (see Lucene.Net.Linq (https://github.com/themotleyfool/Lucene.Net.Linq)) for info
      var results = db.Query<T>(t => t.X == "asd").ToArray();
      

* Documents are stored in in App_Data/PieDb/pieId.json
* Documents are serialized using JSON.NET (http://james.newtonking.com/projects/json-net.aspx)
* Documents are indexed using Lucene.Net.Linq (https://github.com/themotleyfool/Lucene.Net.Linq)
* Optimistic Concurrency is supported 
* MIT Licenced

Why "PieDb?" Many of my other projects are named after foods and it's short.

