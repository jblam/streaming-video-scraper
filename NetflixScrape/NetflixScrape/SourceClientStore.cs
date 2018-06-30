using JBlam.NetflixScrape.Core;
using JBlam.NetflixScrape.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    public class SourceClientStore
    {
        WebsocketMessenger source;
        public WebsocketMessenger Source
        {
            get => source;
            set
            {
                var oldSource = source;
                source = value;
                if (value != null)
                {
                    value.MessageReceived += Source_MessageReceived;
                }
                if (oldSource != null)
                {
                    oldSource.MessageReceived -= Source_MessageReceived;
                }
            }
        }

        readonly ClientRegister clientRegister = new ClientRegister();

        public ClientTicket AddClient(WebsocketMessenger messenger)
        {
            var ticket = clientRegister.Enregister(messenger);
            ticket.EndTask.ContinueWith(task =>
            {
                clientRegister.Deregister(ticket);
            });
            ticket.MessageReceived += Client_MessageReceived;
            return ticket;
        }

        IReadOnlyCollection<ClientTicket> Clients => throw new NotImplementedException("Somehow enumerate all the clients in the dictionary without throwing if collection mutated");

        private async void Source_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            // check the (client, sequence) -> TCS and fulfills 
            await Task.WhenAll(Clients.Select(c => c.SendIfRelevantAsync(e.Message)));
            Console.WriteLine(e.Message);
        }

        private async void Client_MessageReceived(object sender, EventArgs e)
        {
            Command message;
            throw new NotImplementedException("Get message from eventargs");
            string json = null;
            throw new NotImplementedException("Serialise message");
            await Source.SendAsync(json);
        }
    }
    public class ClientTicket
    {
        public ClientTicket(WebsocketMessenger messenger, int sequence)
        {
            Sequence = sequence;
            this.messenger = messenger;
            messenger.MessageReceived += (sender, e) => RaiseMessageReceived(e.Message);
        }
        readonly WebsocketMessenger messenger;
        public int Sequence { get; }
        void RaiseMessageReceived(string message)
        {
            Command incomingCommand;
            throw new NotImplementedException("Handle incoming message and deserialise");
            var outgoingCommand = incomingCommand.WithClientIdentifier(Sequence);
            throw new NotImplementedException("Define handler type");
            MessageReceived?.Invoke(this, EventArgs.Empty);
        }
        public Task SendIfRelevantAsync(string message)
        {
            throw new NotImplementedException("Is this string relevant? Do I need to JSON-parse first?");
            return messenger.SendAsync(message);
        }
        public Task EndTask => messenger.ReceiveTask;
        public event EventHandler MessageReceived;
    }
    class ClientRegister : TicketRegister<WebsocketMessenger, ClientTicket>
    {
        protected override ClientTicket CreateTicket(WebsocketMessenger source, int sequence) => new ClientTicket(source, sequence);

        protected override int RetrieveSequence(ClientTicket ticket) => ticket.Sequence;
    }
}
