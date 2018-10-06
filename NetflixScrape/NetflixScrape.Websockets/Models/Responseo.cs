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

        public override string ToString() => $"{Source} {Sequence} {DispatchResult}";
        public static bool TryParse(string serialisedResponse, out Responseo response)
        {
            var tokens = serialisedResponse.Split(new[] { ' ' }, 3);
            
            if (tokens.Length == 3
                && int.TryParse(tokens[0], out var source)
                && int.TryParse(tokens[1], out var sequence)
                && Enum.TryParse(tokens[2], out CommandDispatchResult result))
            {
                response = new Responseo(source, sequence, result);
                return true;
            }
            else
            {
                response = default(Responseo);
                return false;
            }
        }
        public static Responseo Parse(string serialisedResponse) => TryParse(serialisedResponse, out var response) ? response : throw new FormatException();
    }
    public enum CommandDispatchResult
    {
        Ack,
        Nack,
        NoHandler,
        ParseError
    }
}
