﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Linq;
using Lucene.Net.Store;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Directory = System.IO.Directory;
using Version = Lucene.Net.Util.Version;

namespace PieDb
{
    public class PieDb
    {
        public string Location { get; private set; }
        public SerializerSettings SerializerSettings { get; set; }
        public AdvancedOptions Advanced { get; private set; }

        public PieDb()
        {
            Location = Path.Combine(Helpers.GetAppDataPath(), "PieDb");
            Directory.CreateDirectory(Location);
            SerializerSettings = new SerializerSettings();
            Advanced = new AdvancedOptions(this);
        }

        public void Store<T>(T obj, string id = null) where T : new()
        {
            var doc = obj.PieDocument(id);
            var json = JsonConvert.SerializeObject(doc, Formatting.Indented, SerializerSettings);
            File.WriteAllText(Path.Combine(Location, doc.Id + ".json"), json);
        }

        public T Get<T>(string pieId)
        {
            var json = File.ReadAllText(Path.Combine(Location, pieId + ".json"));
            var obj = JsonConvert.DeserializeObject<PieDocument>(json, SerializerSettings);
            return (T) obj.Data;
        }

        public class AdvancedOptions
        {
            private readonly PieDb _pieDb;

            public AdvancedOptions(PieDb pieDb)
            {
                _pieDb = pieDb;
            }

            public void Clear()
            {
                Directory.Delete(_pieDb.Location, true);
                Directory.CreateDirectory(_pieDb.Location);
            }
        }

        public IQueryable<T> Query<T>(Expression<Func<T, bool>> where)
        {
            throw new NotImplementedException();
        }
    }


    public class PieDocument
    {
        public object Data { get; set; }

        public string Id { get; set; }
    }

    public static class PieIdExtension
    {
        static ConditionalWeakTable<object, PieDocument> KeyTable = new ConditionalWeakTable<object, PieDocument>(); 

        public static PieDocument PieDocument(this object obj, string id = null)
        {
            return KeyTable.GetValue(obj, o => new PieDocument()
            {
                Data = o,
                Id = id ?? Guid.NewGuid().ToString()
            });
        }

        public static string PieId(this object obj)
        {
            return obj.PieDocument().Id;
        }
    }

    internal static class Helpers
    {
        internal static string GetAppDataPath()
        {
            return AppDomain.CurrentDomain.GetData("DataDirectory") as string ?? (AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "/App_Data");
        }
    }
}
