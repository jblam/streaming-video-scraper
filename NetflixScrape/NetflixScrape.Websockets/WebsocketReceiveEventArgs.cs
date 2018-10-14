using System;

namespace JBlam.NetflixScrape.Core
{
    /// <summary>
    /// Describes a websocket message event
    /// </summary>
    public class WebsocketReceiveEventArgs : EventArgs
    {
        public WebsocketReceiveEventArgs(string message)
        {
            Message = message;
        }

        /// <summary>
        /// Gets the message text
        /// </summary>
        public string Message { get; }
    }
}
