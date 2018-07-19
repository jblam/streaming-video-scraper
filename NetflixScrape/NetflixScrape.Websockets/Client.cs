using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Core
{
    /// <summary>
    /// A client which issues commands to, and observes notifications from,
    /// a websocket server
    /// </summary>
    public class Client : IDisposable
    {
        /// <summary>
        /// Creates a new instace of <see cref="Client"/>
        /// </summary>
        /// <param name="serverUri">The URI identifying the server</param>
        public Client(WebsocketMessenger messenger)
        {
            this.messenger = messenger;
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

        /// <summary>
        /// Asynchronously requests execution of an action, resolving with the server response
        /// </summary>
        /// <param name="action">The action to execute remotely</param>
        /// <returns>A task which resovles to the server response</returns>
        public async Task<Command> ExecuteCommandAsync(string action)
        {
            var ticket = c1.Enregister(action);
            await messenger.SendAsync(ticket.Command.Serialise());
            return await ticket.ResponseTask;
        }

        /// <summary>
        /// Asynchronously connects to a server and returns an instance of <see cref="Client"/>
        /// </summary>
        /// <param name="serverUri">The server to connect</param>
        /// <returns>A task resolving to a client instance</returns>
        public static async Task<Client> ConnectNewAsync(Uri serverUri)
        {
            if (serverUri == null) { throw new ArgumentNullException(nameof(serverUri)); }
            if (serverUri.Scheme != "ws") { throw new ArgumentException("URI must be a websocket URI"); }
            var socket = new ClientWebSocket();
            await socket.ConnectAsync(serverUri, CancellationToken.None);

            // TODO: right now, we're prompting the server to give us our client ID.
            // This kinda misses the point of sockets.
            // The server should give us the ID straight up, but we might miss the immediate message
            // because we're connecting the socket first, then attaching a messenger.
            // So: let's make the messenger async-attachable, and then we can ensure that we get immedately-
            // sent messages.

            var m = new WebsocketMessenger(socket);
            var output = new Client(m);
            var id = await output.ExecuteCommandAsync("id");
            return output;
        }

        Task Dispose() => messenger.IsDisposed ? Task.CompletedTask : messenger.FinishAsync();
        void IDisposable.Dispose()
        {
            Dispose();
        }

        /// <summary>
        /// Event raised when the server sends a broadcast message
        /// </summary>
        public event EventHandler Broadcast;
        /// <summary>
        /// Event raised when a server message is observed but cannot be interpreted
        /// </summary>
        public event EventHandler MessageError;

        
        readonly CommandTicketRegister c1 = new CommandTicketRegister();
        readonly WebsocketMessenger messenger;
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
        // TODO: set the command identifier here

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
