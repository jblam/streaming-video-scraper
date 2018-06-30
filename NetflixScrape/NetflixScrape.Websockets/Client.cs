using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Core
{
    class Client
    {
        public Client(Uri serverUri)
        {
            if (serverUri == null) { throw new ArgumentNullException(nameof(serverUri)); }
            if (serverUri.Scheme != "ws") { throw new ArgumentException("URI must be a websocket URI"); }
            this.serverUri = serverUri;
            socket = new ClientWebSocket();

            // TODO: dispose semantics
        }

        public async Task ConnectAsync()
        {
            // Unsure if this is an undocumented feature, but no other member of enum
            // WebSocketState seems to describe a newly-created websocket.
            if (socket.State != WebSocketState.None || messenger != null)
            {
                throw new InvalidOperationException("Connection already attempted");
            }
            await socket.ConnectAsync(serverUri, CancellationToken.None);
            messenger = new WebsocketMessenger(socket);
            messenger.MessageReceived += Messenger_MessageReceived;
        }

        private void Messenger_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            try
            {
                // TODO: type is response, not command
                var command = JsonConvert.DeserializeObject<Command>(e.Message);
                if (command.Sequence == 0)
                {
                    // TODO: message type
                    Broadcast?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    var ticket = c1.Deregister(command.Sequence);
                    ticket.Resolve(command);
                }
            }
            catch (JsonException)
            {
                // TODO: design event args type
                MessageError?.Invoke(this, EventArgs.Empty);
            }
        }
        public async Task<Command> ExecuteCommandAsync(string action)
        {
            var ticket = c1.Enregister(action);
            await messenger.SendAsync(ticket.Command.Serialise());
            return await ticket.ResponseTask;
        }
        public static async Task<Client> ConnectNewAsync(Uri serverUri)
        {
            var output = new Client(serverUri);
            await output.ConnectAsync();
            return output;
        }
        public event EventHandler Broadcast;
        public event EventHandler MessageError;



        readonly Uri serverUri;
        readonly ClientWebSocket socket;
        readonly CommandTicketRegister c1;
        WebsocketMessenger messenger;
    }


    class CommandResponseTicket
    {
        public CommandResponseTicket(Command command)
        {
            Command = command;
        }
        public int Sequence => Command.Sequence;
        public Command Command { get; }
        readonly TaskCompletionSource<Command> responseCompletionSource = new TaskCompletionSource<Command>();
        public Task<Command> ResponseTask => responseCompletionSource.Task;
        public void Resolve(Command response) => responseCompletionSource.SetResult(response);

    }
    class CommandTicketRegister : TicketRegister<string, CommandResponseTicket>
    {
        protected override CommandResponseTicket CreateTicket(string source, int sequence)
        {
            return new CommandResponseTicket(Command.Create(sequence, source));
        }
        protected override int RetrieveSequence(CommandResponseTicket ticket)
        {
            return ticket.Sequence;
        }
    }
}
