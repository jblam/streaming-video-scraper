using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetflixScrape.Websockets
{
    public class WebsocketMessenger : IDisposable
    {
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

        public Task ReceiveTask { get; }
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        readonly WebSocket socket;
        readonly Encoding encoding = Encoding.UTF8;
        readonly byte[] sendBuffer = new byte[1024];
        readonly byte[] receiveBuffer = new byte[1024];

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
                MessageReceived?.Invoke(this, messageEventArgs);
                if (nextMessage.CloseStatus.HasValue)
                {
                    tokenSource.Cancel();
                }
            }
        }

        public event EventHandler<WebsocketReceiveEventArgs> MessageReceived;

        public Task FinishAsync() => Dispose(WebSocketCloseStatus.NormalClosure, string.Empty);
        async void IDisposable.Dispose() => await Dispose(WebSocketCloseStatus.Empty, "Disposing");
        async Task Dispose(WebSocketCloseStatus closeStatus, string reason)
        {
            tokenSource.Cancel();
            if (socket != null && socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(closeStatus, reason, CancellationToken.None);
                socket.Dispose();
            }
        }
        public bool IsDisposed => tokenSource.IsCancellationRequested;
    }
}
