using System;
using System.Collections.Generic;

namespace JBlam.NetflixScrape.Core
{
    /// <summary>
    /// A ticketing system which issues a ticket of type <typeparamref name="TTicket"/>
    /// uniquely identifying a given instance of <see cref="TInput"/> through an integer identifier;
    /// and can recover the ticket when provided with the identifier
    /// </summary>
    /// <typeparam name="TInput">The data which is uniquely identified by the ticket</typeparam>
    /// <typeparam name="TTicket">The ticket wrapping the data</typeparam>
    public abstract class TicketRegister<TInput, TTicket> : IDisposable
    {
        protected TicketRegister()
            : this(0)
        {

        }

        protected TicketRegister(int initialSequence)
        {
            sequence = initialSequence;
        }

        int sequence;
        Dictionary<int, TTicket> inner = new Dictionary<int, TTicket>();
        /// <summary>
        /// Returns a ticket wrapping the given data
        /// </summary>
        /// <param name="source">The data to wrap</param>
        /// <returns>A ticket beaing a unique identifier</returns>
        public TTicket Enregister(TInput source)
        {
            if (isDisposed) { throw new ObjectDisposedException(nameof(TicketRegister<TInput, TTicket>)); }
            lock (inner)
            {
                var ticket = CreateTicket(source, sequence);
                inner.Add(sequence, ticket);
                sequence += 1;
                return ticket;
            }
        }
        /// <summary>
        /// Removes and returns the ticket bearing the given unique identifier
        /// </summary>
        /// <param name="sequence">The integer which identifies the ticket</param>
        /// <returns>The ticket identified by <paramref name="sequence"/></returns>
        public TTicket Deregister(int sequence)
        {
            TTicket result;
            bool foundResult;
            lock (inner)
            {
                foundResult = inner.TryGetValue(sequence, out result);
                if (foundResult)
                {
                    inner.Remove(sequence);
                }
            }
            if (!foundResult) { throw new KeyNotFoundException(); }
            return result;
        }
        /// <summary>
        /// Removes the given ticket
        /// </summary>
        /// <param name="ticket">The ticket to remove</param>
        public void Deregister(TTicket ticket)
        {
            lock (inner)
            {
                inner.Remove(RetrieveSequence(ticket));
            }
        }
        /// <summary>
        /// When overridden in a derived class, creates a ticket object from the given data and sequence integer
        /// </summary>
        /// <param name="source">The data to wrap in the ticket type</param>
        /// <param name="sequence">The unique sequence integer identifying the ticket</param>
        /// <returns>The ticket</returns>
        protected abstract TTicket CreateTicket(TInput source, int sequence);
        /// <summary>
        /// When overridden in a derived class, extracts the sequence identifier from the ticket
        /// </summary>
        /// <param name="ticket">The ticket instance bearing a sequence identifier</param>
        /// <returns>The sequence identifier member of the ticket</returns>
        protected abstract int RetrieveSequence(TTicket ticket);

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls
        private bool isDisposed = false;

        protected virtual void Dispose(bool disposing)
        {
            isDisposed = true;
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var ticket in inner.Values)
                    {
                        DisposeTicket(ticket);
                    }
                }

                disposedValue = true;
            }
        }

        /// <summary>
        /// When overridden in a derived class, disposes of the ticket instance
        /// </summary>
        /// <param name="ticket"></param>
        protected virtual void DisposeTicket(TTicket ticket) { }

        /// <summary>
        /// Disposes the ticket register by preventing additional registrations, and disposing any outstanding tickets
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
