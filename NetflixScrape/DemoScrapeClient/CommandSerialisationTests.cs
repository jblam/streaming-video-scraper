using JBlam.NetflixScrape.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JBlam.NetflixScrape.DemoScrapeClient
{
    public class CommandSerialisationTests
    {
        readonly Commando.Builder builder = new Commando.Builder(0);

        [Fact]
        public void CanSerialiseCommand()
        {
            var c = builder.Create(CommandAction.Unidentified);
            var serialised = c.ToString();
            Assert.Equal("0 0 Unidentified", serialised);
        }

        [Fact]
        public void CanDeserialiseCommand()
        {
            var serialised = "0 0 Unidentified";
            var c = Commando.Parse(serialised);
            Assert.Equal(CommandAction.Unidentified, c.Action);
        }

        [Fact]
        public void CanSerialiseParamterCommand()
        {
            var c = builder.Create(CommandAction.MouseMove, 1, 1);
            var serialsed = c.ToString();
            Assert.Equal("0 0 MouseMove 1,1", serialsed);
        }

        [Fact]
        public void CanDeserialiseIntPairCommand()
        {
            var serialised = "0 0 MouseMove 10,11";
            var c = Commando.Parse(serialised);
            Assert.Equal((10, 11), c.GetInts());
        }
    }
}
