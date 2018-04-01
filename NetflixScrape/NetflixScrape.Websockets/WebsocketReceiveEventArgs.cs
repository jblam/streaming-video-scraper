using System;

namespace JBlam.NetflixScrape.Core
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
