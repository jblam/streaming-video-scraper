using System;
using System.Collections.Generic;
using System.Text;

namespace JBlam.NetflixScrape.Core.Models
{
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
