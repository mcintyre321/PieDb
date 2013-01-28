using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json;

namespace PieDb
{
    public class SerializerSettings : JsonSerializerSettings
    {
        public SerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All;
            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple;
            PreserveReferencesHandling = PreserveReferencesHandling.Objects;
        }
    }
}