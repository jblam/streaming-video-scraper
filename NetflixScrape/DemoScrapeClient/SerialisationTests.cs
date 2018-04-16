using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JBlam.NetflixScrape.DemoScrapeClient
{
    public class AmazingSerialisationBinder : Newtonsoft.Json.Serialization.ISerializationBinder
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

    public class SerialisationTests
    {
        [Fact]
        public void WatchDurationsSerialiseAsDates()
        {
            var watchModel = new WatchModel
            {
                Duration = TimeSpan.FromSeconds(1.5)
            };
            var jsonString = JsonConvert.SerializeObject(watchModel, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, SerializationBinder = new AmazingSerialisationBinder() });
            var jObject = JObject.Parse(jsonString);
            var serialisedDuration = jObject["Duration"];
            Assert.Equal(JTokenType.Float, serialisedDuration.Type);
            Assert.Equal(1.5, serialisedDuration.Value<double>());
        }

        [Fact]
        public void WatchModelRoundtrips()
        {
            var watchModel = new WatchModel
            {
                Duration = TimeSpan.FromSeconds(1.5)
            };
            var jsonString = JsonConvert.SerializeObject(watchModel);
            var deserialised = JsonConvert.DeserializeObject<WatchModel>(jsonString);
            Assert.Equal(watchModel.Duration, deserialised.Duration);
        }
    }
}
