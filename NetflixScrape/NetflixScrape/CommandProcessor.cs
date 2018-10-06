using JBlam.NetflixScrape.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    class CommandProcessor
    {
        public CommandProcessor(ICommandProcessor hostProcessor)
        {
            this.hostProcessor = hostProcessor;
        }

        readonly ICommandProcessor hostProcessor;
        public ICommandProcessor BrowserProcessor { get; set; }

        public Responseo Process(Commando command)
        {
            if (hostProcessor.CanProcess(command.Action))
            {
                hostProcessor.BeginProcess(command);
                return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
            }
            else if (BrowserProcessor == null)
            {
                return Responseo.CreateResponse(command, CommandDispatchResult.NoHandler);
            }
            else if (BrowserProcessor.CanProcess(command.Action))
            {
                return BrowserProcessor.BeginProcess(command);
            }
            else
            {
                return Responseo.CreateResponse(command, CommandDispatchResult.NoHandler);
            }
        }
    }
}
