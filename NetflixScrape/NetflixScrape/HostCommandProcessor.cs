using JBlam.NetflixScrape.Core.Models;
using System;

namespace JBlam.NetflixScrape.Server
{
    abstract class HostCommandProcessor : ICommandProcessor
    {
        public abstract void MoveMouse(int x, int y);
        public abstract void SetMouse(int x, int y);
        public abstract void ClickMouse(MouseButton button);
        public abstract void WheelMouse(MouseWheelDirection direction, int amount);

        public bool CanProcess(CommandAction action)
        {
            switch (action)
            {
                case CommandAction.MouseMove:
                case CommandAction.MouseSet:
                case CommandAction.MouseClick:
                case CommandAction.MouseWheel:
                    return true;
                default:
                    return false;
            }
        }

        public Responseo BeginProcess(Commando command)
        {
            switch (command.Action)
            {
                case CommandAction.MouseMove:
                    var (deltaX, deltaY) = command.GetInts();
                    MoveMouse(deltaX, deltaY);
                    return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
                case CommandAction.MouseSet:
                    var (xPosition, yPosition) = command.GetInts();
                    SetMouse(xPosition, yPosition);
                    return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
                case CommandAction.MouseClick:
                    var clickButton = command.GetEnum<MouseButton>();
                    ClickMouse(clickButton);
                    return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
                case CommandAction.MouseWheel:
                    var (wheelDirection, wheelAmount) = command.GetEnumInt<MouseWheelDirection>();
                    WheelMouse(wheelDirection, wheelAmount);
                    return Responseo.CreateResponse(command, CommandDispatchResult.Ack);
                default:
                    return Responseo.CreateResponse(command, CommandDispatchResult.Nack);
            }
        }
    }
}
