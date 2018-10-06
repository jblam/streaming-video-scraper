using JBlam.NetflixScrape.Core.Models;

namespace JBlam.NetflixScrape.Server
{
    interface ICommandProcessor
    {
        bool CanProcess(CommandAction action);
        // TODO: response typing?
        Responseo BeginProcess(Commando command);
    }
}
