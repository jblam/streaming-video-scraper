using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
    public struct Responseo
    {
        Responseo(int source, int sequence, CommandDispatchResult dispatchResult)
        {
            Source = source;
            Sequence = sequence;
            DispatchResult = dispatchResult;
        }
        public int Source { get; }
        public int Sequence { get; }
        public CommandDispatchResult DispatchResult { get; }

        public static Responseo CreateResponse(Commando command, CommandDispatchResult result) => new Responseo(command.Source, command.Sequence, result);
        public static readonly Responseo ParseError = new Responseo(0, 0, CommandDispatchResult.ParseError);
    }
    public enum CommandDispatchResult
    {
        Ack,
        Nack,
        NoHandler,
        ParseError
    }
}
