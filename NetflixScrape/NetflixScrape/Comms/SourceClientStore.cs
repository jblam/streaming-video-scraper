using JBlam.NetflixScrape.Core;
using JBlam.NetflixScrape.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server.Comms
{
    public class SourceClientStore
    {
        public SourceClientStore(CommandProcessor commandProcessor)
        {
            this.commandProcessor = commandProcessor;
        }

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
        readonly CommandProcessor commandProcessor;

        public async Task<ClientTicket> AddClient(WebsocketMessenger messenger)
        {
            var ticket = clientRegister.Enregister(messenger);
            ticket.MessageReceived += Client_MessageReceived;
            await messenger.SendAsync(ticket.Sequence.ToString());
            return ticket;
        }
        public void Deregister(ClientTicket ticket) => clientRegister.Deregister(ticket);

        IReadOnlyCollection<ClientTicket> Clients => throw new NotImplementedException("Somehow enumerate all the clients in the dictionary without throwing if collection mutated");

        private async void Source_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            // check the (client, sequence) -> TCS and fulfills 
            await Task.WhenAll(Clients.Select(c => c.SendIfRelevantAsync(e.Message)));
            Console.WriteLine(e.Message);
        }
        
        private async void Client_MessageReceived(object sender, TicketCommandEventArgs e)
        {
            var response = commandProcessor.Process(e.Command);
            await ((ClientTicket)sender).SendIfRelevantAsync(response);
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
        async void RaiseMessageReceived(string message)
        {
            if (Commando.TryParse(message, out var command))
            {
                MessageReceived?.Invoke(this, new TicketCommandEventArgs(command));
            }
            else
            {
                await messenger.SendAsync(Responseo.ParseError.ToString());
            }
        }
        public Task SendIfRelevantAsync(Responseo response)
        {
            if (response.Source == Sequence)
            {
                return messenger.SendAsync(response.ToString());
            }
            else
            {
                return Task.CompletedTask;
            }
        }
        public Task SendIfRelevantAsync(string message)
        {
            throw new NotImplementedException("Is this string relevant? Do I need to JSON-parse first?");
            return messenger.SendAsync(message);
        }
        public Task EndTask => messenger.ReceiveTask;
        public event EventHandler<TicketCommandEventArgs> MessageReceived;
    }
    public class TicketCommandEventArgs : EventArgs
    {
        public TicketCommandEventArgs(Commando command)
        {
            Command = command;
        }
        public Commando Command { get; }
    }
    class ClientRegister : TicketRegister<WebsocketMessenger, ClientTicket>
    {
        public ClientRegister()
            : base(1)
        {
            // ensure that clients start from `1` to reserve `0` for future use
        }
        protected override ClientTicket CreateTicket(WebsocketMessenger source, int sequence) => new ClientTicket(source, sequence);

        protected override int RetrieveSequence(ClientTicket ticket) => ticket.Sequence;
    }
}
