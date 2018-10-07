using JBlam.NetflixScrape.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
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
        Client(WebsocketMessenger messenger, int clientId)
        {
            this.messenger = messenger;
            this.commandBuilder = new Commando.Builder(clientId);
            messenger.MessageReceived += Messenger_MessageReceived;
        }

        readonly Commando.Builder commandBuilder;
        readonly ConcurrentDictionary<int, TaskCompletionSource<Responseo>> commandCompletionSources = new ConcurrentDictionary<int, TaskCompletionSource<Responseo>>();

        private void Messenger_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            try
            {
                // TODO: type is response, not command
                var response = Responseo.Parse(e.Message);
                if (response.Sequence == 0)
                {
                    // TODO: message type
                    Broadcast?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    if (commandCompletionSources.TryRemove(response.Sequence, out var tcs))
                    {
                        // TODO: outcome
                        tcs.TrySetResult(response);
                    }
                    else
                    {
                        // TODO: desync? throw?
                    }
                }
            }
            catch (FormatException)
            {
                MessageError?.Invoke(this, EventArgs.Empty);
            }
        }

        public Task<Responseo> RequestState()
        {
            return ExecuteCommandAsync(commandBuilder.Create(CommandAction.State));
        }
        public async Task<Responseo> MoveAndClick()
        {
            var moveResponse = await ExecuteCommandAsync(commandBuilder.Create(CommandAction.MouseSet, 1832, 16));
            if (moveResponse.DispatchResult != CommandDispatchResult.Ack) { return moveResponse; }
            return await ExecuteCommandAsync(commandBuilder.Create(CommandAction.MouseClick, MouseButton.Left));
        }
        
        async Task<Responseo> ExecuteCommandAsync(Commando command)
        {
            var tcs = new TaskCompletionSource<Responseo>();
            if (!commandCompletionSources.TryAdd(command.Sequence, tcs))
            {
                throw new ArgumentException("Attempted to add a duplicate command");
            }
            try
            {
                await messenger.SendAsync(command.ToString()).ConfigureAwait(false);
            }
            catch
            {
                commandCompletionSources.TryRemove(command.Sequence, out _);
                throw;
            }
            return await tcs.Task.ConfigureAwait(false);
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
            try
            {
                await socket.ConnectAsync(serverUri, CancellationToken.None);
            }
            catch
            {
                ;
            }
            // TODO: right now, we're prompting the server to give us our client ID.
            // This kinda misses the point of sockets.
            // The server should give us the ID straight up, but we might miss the immediate message
            // because we're connecting the socket first, then attaching a messenger.
            // So: let's make the messenger async-attachable, and then we can ensure that we get immedately-
            // sent messages.

            var clientIdCompletionSource = new TaskCompletionSource<int>();
            void Messenger_FirstMessage(object sender, WebsocketReceiveEventArgs args)
            {
                ((WebsocketMessenger)sender).MessageReceived -= Messenger_FirstMessage;
                try
                {
                    clientIdCompletionSource.SetResult(int.Parse(args.Message));
                }
                catch (Exception ex)
                {
                    clientIdCompletionSource.SetException(ex);
                }
            }
            var m = new WebsocketMessenger(socket);
            m.MessageReceived += Messenger_FirstMessage;

            var output = new Client(m, await clientIdCompletionSource.Task);
            return output;
        }

        Task Dispose() => messenger.IsDisposed ? Task.CompletedTask : messenger.FinishAsync();
        void IDisposable.Dispose()
        {
            Dispose();
        }
        public Task LifetimeTask => messenger.ReceiveTask;

        /// <summary>
        /// Event raised when the server sends a broadcast message
        /// </summary>
        public event EventHandler Broadcast;
        /// <summary>
        /// Event raised when a server message is observed but cannot be interpreted
        /// </summary>
        public event EventHandler MessageError;
        
        readonly WebsocketMessenger messenger;
    }
}
