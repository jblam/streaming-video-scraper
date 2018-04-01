using JBlam.NetflixScrape.Core;
using System;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DemoScrapeClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("What port you running on?");
            var portEntry = Console.ReadLine();
            if (int.TryParse(portEntry, out var port))
            {
                using (var socket = new ClientWebSocket())
                {
                    await socket.ConnectAsync(new Uri($"ws://localhost:{port}/ws"), CancellationToken.None);
                    using (var messenger = new WebsocketMessenger(socket))
                    {
                        messenger.MessageReceived += Messenger_MessageReceived;

                        string nextLine;
                        while (!String.IsNullOrEmpty(nextLine = Console.ReadLine()))
                        {
                            await messenger.SendAsync(nextLine);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine($"{portEntry} is not an integer. Better luck next time");
            }
            ;
        }

        private static void Messenger_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            Console.WriteLine("Received: {0}", e.Message);
        }
    }
}
