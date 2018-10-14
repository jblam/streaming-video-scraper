using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    internal class AmazingSerialisationBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name.Replace("Model", "").Replace("Command", "");
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            return Type.GetType(typeName + "Model") ?? Type.GetType(typeName + "Command");
        }
    }
    public static class SerialisationExtensions
    {
        internal static readonly JsonSerializerSettings DefaultSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Objects,
            SerializationBinder = new AmazingSerialisationBinder(),
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        // TODO: this is probably not the best way of doing it.
        public static string Serialise(this object o) => JsonConvert.SerializeObject(o, DefaultSettings);
    }
}
