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
                using (var c = await Client.ConnectNewAsync(new Uri($"ws://localhost:{port}/ws")))
                {
                    c.Broadcast += (sender, e) => Console.WriteLine("New broadcast: {0}", e);
                    c.MessageError += (sender, e) => Console.WriteLine("Say what now?! {0}", e);
                    string nextLine;
                    while (!String.IsNullOrEmpty(nextLine = Console.ReadLine()))
                    {
                        await c.ExecuteCommandAsync(nextLine);
                    }
                }
            }
            else
            {
                Console.WriteLine($"{portEntry} is not an integer. Better luck next time");
            }
            ;
        }
    }
}
