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

        #region stuff to move out of here
#warning This is all in the wrong spot
        class Response
        {
            public static Response UnrecognisedCommandResponse(Command command) => throw new NotImplementedException();
            public static Response AcknowledgeCommandResponse(Command command) => throw new NotImplementedException();
            public Response(Command command, object data)
            {
                throw new NotImplementedException();
            }
        }
        class CommandProcessor
        {
            public async Task<Response> ProcessAsync(Command c)
            {
                if (!IsRecognisedCommand(c)) { return Response.UnrecognisedCommandResponse(c); }
                throw new NotImplementedException();
            }
            bool IsRecognisedCommand(Command c) => throw new NotImplementedException();
        }
        #endregion

        private async void Client_MessageReceived(object sender, TicketCommandEventArgs e)
        {
            bool shouldPassOnToExtension = default(bool?) ?? throw new NotImplementedException();
            if (shouldPassOnToExtension)
            {
                Task<object> extensionResponse = (Task<object>)null ?? throw new NotImplementedException("Send to extension");
                var response = await extensionResponse;
                await (sender as ClientTicket).SendIfRelevantAsync(response.Serialise());
            }
            else
            {
                throw new NotImplementedException("Decide what to do");
                throw new NotImplementedException("Do that thing");
                throw new NotImplementedException("Respond?");
            }
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
            try
            {
                var incomingCommand = JsonConvert.DeserializeObject<Command>(message);
                MessageReceived?.Invoke(this, new TicketCommandEventArgs(incomingCommand));
            }
            catch (JsonException)
            {
                string s = (string)null ?? throw new NotImplementedException("Define 'error' response type");
                await messenger.SendAsync(s);
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
        public TicketCommandEventArgs(Command command)
        {
            Command = command;
        }
        public Command Command { get; }
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
