using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Core
{
    /// <summary>
    /// Wraps a websocket connection to provide an async-send and event-receive API
    /// </summary>
    public class WebsocketMessenger : IDisposable
    {
        /// <summary>
        /// Creates an instance of <see cref="WebsocketMessenger"/> around a socket instance
        /// </summary>
        /// <param name="socket">The socket to wrap</param>
        public WebsocketMessenger(WebSocket socket)
        {
            if (socket == null) { throw new ArgumentNullException(nameof(socket)); }
            if (socket.State == WebSocketState.Connecting || socket.State == WebSocketState.Open)
            {
                this.socket = socket;
                ReceiveTask = ReceiveLoop();
            }
            else
            {
                throw new ArgumentException("Socket not open", nameof(socket));
            }
        }

        /// <summary>
        /// Gets a task which resolves when the socket closes
        /// </summary>
        public Task ReceiveTask { get; }
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        readonly WebSocket socket;
        readonly Encoding encoding = Encoding.UTF8;
        readonly byte[] sendBuffer = new byte[1024];
        readonly byte[] receiveBuffer = new byte[1024];

        /// <summary>
        /// Asynchronously sends a message through the socket
        /// </summary>
        /// <param name="message">The message to send</param>
        /// <returns>A task which resolves when the message has been sent</returns>
        public async Task SendAsync(string message)
        {
            if (IsDisposed) { throw new ObjectDisposedException(nameof(WebsocketMessenger)); }
            if (message.Length > 1024) { throw new ArgumentException("Message was too long"); }
            var encodedLength = encoding.GetBytes(message, 0, message.Length, sendBuffer, 0);
            var output = new ArraySegment<byte>(sendBuffer, 0, encodedLength);
            await socket.SendAsync(output, WebSocketMessageType.Text, true, tokenSource.Token);
        }
        async Task ReceiveLoop()
        {
            while (!tokenSource.IsCancellationRequested)
            {
                var nextMessage = await socket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), tokenSource.Token);
                var messageEventArgs = new WebsocketReceiveEventArgs(encoding.GetString(receiveBuffer, 0, nextMessage.Count));
                messageEventSource.SendOrBuffer(this, messageEventArgs);
                if (nextMessage.CloseStatus.HasValue)
                {
                    tokenSource.Cancel();
                }
            }
        }

        readonly BufferedEventSource<WebsocketReceiveEventArgs> messageEventSource = new BufferedEventSource<WebsocketReceiveEventArgs>();
        /// <summary>
        /// Event raised when the messager receives a message
        /// </summary>
        public event EventHandler<WebsocketReceiveEventArgs> MessageReceived
        {
            add => messageEventSource.Event += value;
            remove => messageEventSource.Event -= value;
        }

        /// <summary>
        /// Asynchronously wait for any outgoing messages, then disposes the messanger and the underlying socket
        /// </summary>
        /// <returns></returns>
        public Task FinishAsync() => Dispose(WebSocketCloseStatus.NormalClosure, string.Empty);
        async void IDisposable.Dispose() => await Dispose(WebSocketCloseStatus.Empty, null);
        async Task Dispose(WebSocketCloseStatus closeStatus, string reason)
        {
            tokenSource.Cancel();
            if (socket != null && socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(closeStatus, reason, CancellationToken.None);
                socket.Dispose();
            }
        }
        /// <summary>
        /// Gets a value indicating if the messanger is disposed
        /// </summary>
        public bool IsDisposed => tokenSource.IsCancellationRequested;
    }
}
