using JBlam.NetflixScrape.Core;
using JBlam.NetflixScrape.Core.Models;
using System;

namespace JBlam.NetflixScrape.Server
{
    class BrowserCommandProcessor : ICommandProcessor
    {
        readonly WebsocketMessenger messenger;

        public bool CanProcess(CommandAction action)
        {
            throw new NotImplementedException();
        }

        public Responseo BeginProcess(Commando command)
        {
            if (!CanProcess(command.Action)) { throw new NotSupportedException("Command not supported"); }
            _ = messenger.SendAsync(command.ToString());
            return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
        }
    }
}
