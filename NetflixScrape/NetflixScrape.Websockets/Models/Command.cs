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

    public static class Demo
    {
        public static void DoThing()
        {
            var c1 = Commando.Create(CommandAction.Browse);
            var c2 = Commando.Create(CommandAction.Select, "Mr Fluffy the Cat");
            var c3 = Commando.Create(CommandAction.MouseWheel, MouseWheelDirection.WheelDown, 2);
            var c4 = Commando.Create(CommandAction.Navigate, NavigationDirection.Up);
        }
        public static void ReceiveThing(string s)
        {
            var c = Commando.Parse(s);
            DoCommand(c);
        }
        static readonly (int x, int y) killLocation = (25, 17);
        static void DoCommandOnHost(Commando c) => throw new NotImplementedException();
        static void MoveMouse(int x, int y) => throw new NotImplementedException();
        static void SetMouse(int x, int y) => throw new NotImplementedException();
        static void ClickMouse(MouseButton button) => throw new NotImplementedException();
        static void WheelMouse(MouseWheelDirection direction, int amount) => throw new NotImplementedException();
        static void DoCommand(Commando c)
        {
            switch (c.Action)
            {
                case CommandAction.Unidentified:
                    // oops, I can't do that
                    break;
                case CommandAction.State:
                    // return the state
                    break;
                case CommandAction.Browse:
                    // open the browser
                    break;
                case CommandAction.Close:
                    DoCommand(Commando.Create(CommandAction.MouseSet, killLocation.x, killLocation.y));
                    DoCommand(Commando.Create(CommandAction.MouseClick, MouseButton.Left));
                    break;
                case CommandAction.Cancel:
                    // send ESC
                    break;
                case CommandAction.Reload:
                    // send F5
                    break;
                case CommandAction.Navigate:
                case CommandAction.Select:
                    DoCommandOnHost(c);
                    break;
                case CommandAction.Jump:
                    break;
                case CommandAction.Info:
                    break;
                case CommandAction.MouseMove:
                    {
                        var (x, y) = c.GetInts();
                        MoveMouse(x, y);
                        break;
                    }
                case CommandAction.MouseSet:
                    {
                        var (x, y) = c.GetInts();
                        SetMouse(x, y);
                        break;
                    }
                case CommandAction.MouseClick:
                    ClickMouse(c.GetEnum<MouseButton>());
                    break;
                case CommandAction.MouseWheel:
                    {
                        var (direction, amount) = c.GetEnumInt<MouseWheelDirection>(1);
                        WheelMouse(direction, amount);
                        break;
                    }
                case CommandAction.Pause:
                case CommandAction.Play:
                    DoCommandOnHost(c);
                    break;
                case CommandAction.Skip:
                    {
                        var offset = c.GetInt(10);
                        DoCommandOnHost(Commando.Create(CommandAction.Skip, offset));
                        break;
                    }
                default:
                    throw new MalformedCommandException(c);
            }
        }
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

    /// <summary>
    /// Defines a command 
    /// </summary>
    public class Command
    {
        /// <summary>
        /// Gets the identifier of the client which submitted this command
        /// </summary>
        public int Client { get; protected set; }
        /// <summary>
        /// Gets the client-generated sequence identifier for this command
        /// </summary>
        public int Sequence { get; protected set; }
        /// <summary>
        /// Gets the action descriptor for this command
        /// </summary>
        public string Action { get; protected set; }

        public static Command Create(int sequence, string action)
        {
            return new Command
            {
                Client = -1,
                Sequence = sequence,
                Action = action
            };
        }
        public static Command<TData> Create<TData>(int sequence, string action, TData data)
        {
            return new Command<TData>
            {
                Client = -1,
                Sequence = sequence,
                Action = action,
                Data = data
            };
        }
        public Command WithClientIdentifier(int clientIdentifier)
        {
            return new Command
            {
                Client = clientIdentifier,
                Action = this.Action,
                Sequence = this.Sequence
            };
        }
    }
    public class Command<TData> : Command
    {
        /// <summary>
        /// Gets the data description for this command
        /// </summary>
        public TData Data { get; internal set; }
        public Command<TData> WithClientIdentifier(int clientIdentifier)
        {
            return new Command<TData>
            {
                Client = clientIdentifier,
                Action = this.Action,
                Sequence = this.Sequence,
                Data = this.Data
            };
        }
    }
}
