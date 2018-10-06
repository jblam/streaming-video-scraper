using JBlam.NetflixScrape.Core;
using JBlam.NetflixScrape.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    public class CommandProcessor
    {
        public CommandProcessor(ICommandProcessor hostProcessor)
        {
            this.hostProcessor = hostProcessor;
        }

        readonly ICommandProcessor hostProcessor;
        public ICommandProcessor BrowserProcessor { get; set; }

        // TODO: return type
        Responseo Process(Commando command)
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
    public interface ICommandProcessor
    {
        bool CanProcess(CommandAction action);
        // TODO: response typing?
        Responseo BeginProcess(Commando command);
    }
    public class HostCommandProcessor : ICommandProcessor
    {
        // TODO: provide linux and windows implementations
        static void MoveMouse(int x, int y) => throw new NotImplementedException();
        static void SetMouse(int x, int y) => throw new NotImplementedException();
        static void ClickMouse(MouseButton button) => throw new NotImplementedException();
        static void WheelMouse(MouseWheelDirection direction, int amount) => throw new NotImplementedException();

        public bool CanProcess(CommandAction action)
        {
            throw new NotImplementedException();
        }

        public Responseo BeginProcess(Commando command)
        {
            throw new NotImplementedException();
        }
    }
    public class BrowserCommandProcessor : ICommandProcessor
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
