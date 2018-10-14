using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public enum CommandAction
    {
        Unidentified,
        State,
        Browse,
        Close,
        Cancel,
        Reload,
        Navigate,
        Select,
        Jump,
        Info,
        MouseMove,
        MouseSet,
        MouseClick,
        MouseWheel,
        Pause,
        Play,
        Skip
    }
    public enum NavigationDirection
    {
        Up,
        Down,
        Left,
        Right,
        In,
        Out
    }
    public enum MouseButton
    {
        Left,
        Right
    }
    public enum MouseWheelDirection
    {
        WheelUp,
        WheelDown,
    }
    public sealed class MalformedCommandException : Exception
    {
        static string GetMessage(Commando command)
        {
            if (command.Action == CommandAction.Unidentified)
            {
                return "Command action could not be identified";
            }
            else
            {
                return $"{command.Action} command did not have expected parameters";
            }
        }
        public MalformedCommandException(Commando source)
            : base(GetMessage(source))
        {

        }
    }
}
