PieDb
=====

The minimal embedded .net database that is as easy-as-pie. 

Useful when you just want to get started QUICKLY with NO FUSS.

Nuget: Install-Package PieDb

      var db = new PieDb();
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

      //querying (COMING SOON)
      var results = db.Query<T>(t => t.X == "asd").ToArray();
      

Documents are stored in in App_Data/PieDb/pieId.json
      
Documents are serialized using JSON.NET (http://james.newtonking.com/projects/json-net.aspx)

Documents are indexed using Lucene.Net.Linq (https://github.com/themotleyfool/Lucene.Net.Linq)

MIT Licenced

