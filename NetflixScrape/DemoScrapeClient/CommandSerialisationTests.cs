using JBlam.NetflixScrape.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace JBlam.NetflixScrape.DemoScrapeClient
{
    public class CommandSerialisationTests
    {
        [Fact]
        public void CanSerialiseCommand()
        {
            var c = Commando.Create(CommandAction.Unidentified);
            var serialised = c.ToString();
            Assert.Equal("Unidentified", serialised);
        }

        [Fact]
        public void CanDeserialiseCommand()
        {
            var serialised = "Unidentified";
            var c = Commando.Parse(serialised);
            Assert.Equal(CommandAction.Unidentified, c.Action);
        }

        [Fact]
        public void CanSerialiseParamterCommand()
        {
            var c = Commando.Create(CommandAction.MouseMove, 1, 1);
            var serialsed = c.ToString();
            Assert.Equal("MouseMove 1,1", serialsed);
        }

        [Fact]
        public void CanDeserialiseIntPairCommand()
        {
            var serialised = "MouseMove 10,11";
            var c = Commando.Parse(serialised);
            Assert.Equal((10, 11), c.GetInts());
        }
    }
}
