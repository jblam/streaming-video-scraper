using JBlam.NetflixScrape.Core;
using Microsoft.Extensions.DependencyInjection;
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

        readonly HashSet<WebsocketMessenger> clients = new HashSet<WebsocketMessenger>();
        public IReadOnlyCollection<WebsocketMessenger> Clients => clients;

        public void AddClient(WebsocketMessenger client)
        {
            client.MessageReceived += Client_MessageReceived;
            clients.Add(client);
        }
        public void RemoveClient(WebsocketMessenger client)
        {
            if (clients.Remove(client))
            {
                client.MessageReceived -= Client_MessageReceived;
            }
        }



        private async void Source_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            await Task.WhenAll(Clients.Select(c => c.SendAsync(e.Message)));
            Console.WriteLine(e.Message);
        }

        private async void Client_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            await Source.SendAsync(e.Message);
        }
    }
}
