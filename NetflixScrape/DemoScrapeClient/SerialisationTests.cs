using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JBlam.NetflixScrape.DemoScrapeClient
{
    public class SerialisationTests
    {
        [Fact]
        public void WatchDurationsSerialiseAsDates()
        {
            var watchModel = new WatchModel
            {
                Duration = TimeSpan.FromSeconds(1.5)
            };
            var jsonString = JsonConvert.SerializeObject(watchModel);
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
