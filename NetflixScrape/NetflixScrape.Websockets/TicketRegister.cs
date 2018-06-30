using System.Collections.Generic;

namespace JBlam.NetflixScrape.Core
{
    public abstract class TicketRegister<TInput, TTicket>
    {
        int sequence;
        Dictionary<int, TTicket> inner = new Dictionary<int, TTicket>();
        public TTicket Enregister(TInput source)
        {
            lock (inner)
            {
                var ticket = CreateTicket(source, sequence);
                inner.Add(sequence, ticket);
                sequence += 1;
                return ticket;
            }
        }
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
        public void Deregister(TTicket ticket)
        {
            lock (inner)
            {
                inner.Remove(RetrieveSequence(ticket));
            }
        }
        protected abstract TTicket CreateTicket(TInput source, int sequence);
        protected abstract int RetrieveSequence(TTicket ticket);
    }
}
