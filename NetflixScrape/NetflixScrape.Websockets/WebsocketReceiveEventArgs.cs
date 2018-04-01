using System;

namespace NetflixScrape.Websockets
{

    public class WebsocketReceiveEventArgs : EventArgs
    {
        public WebsocketReceiveEventArgs(string message)
        {
            Message = message;
        }

        public string Message { get; }
    }
}
