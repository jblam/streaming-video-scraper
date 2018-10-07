using JBlam.NetflixScrape.Core;
using JBlam.NetflixScrape.Core.Models;
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
                    try
                    {
                        while (true)
                        {
                            Console.WriteLine("[S]tate; [M]ove; [ESC] to quit");
                            var key = Console.ReadKey(true);
                            switch (key.Key)
                            {
                                case ConsoleKey.Escape:
                                    Console.WriteLine("Quitting.");
                                    return;
                                case ConsoleKey.M:
                                    Console.WriteLine("Moving: ");
                                    Console.WriteLine(await c.MoveAndClick());
                                    break;
                                case ConsoleKey.S:
                                    Console.Write("Requesting state: ");
                                    Console.WriteLine(await c.RequestState());
                                    break;
                                default:
                                    Console.WriteLine("Could not parse command. Try again or enter empty line to quit.");
                                    break;
                            }
                        }
                    }
                    catch (TaskCanceledException)
                    {
                        // the task will be cancelled if the server gets closed while waiting on the task
                        // in this case, we can just re-enter the loop.
                        Console.WriteLine("Remote server closed connection. Goodbye.");
                        return;
                    }
                }
            }
            else
            {
                Console.WriteLine($"{portEntry} is not an integer. Better luck next time");
            }
        }
    }
}
