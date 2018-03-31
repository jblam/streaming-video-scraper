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
                    var buffer = new byte[4096];
                    for (string input = "hi"; input != "q"; input = Console.ReadLine())
                    {
                        var utf8Length = Encoding.UTF8.GetBytes(input, 0, input.Length, buffer, 0);
                        var segment = new ArraySegment<byte>(buffer, 0, utf8Length);
                        await socket.SendAsync(segment, WebSocketMessageType.Text, false, CancellationToken.None);

                        var response = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        Console.WriteLine($"Received {Encoding.UTF8.GetString(buffer, 0, response.Count)}");
                    }
                }

                using (var client = new HttpClient() { BaseAddress = new Uri($"http://localhost:{port}") })
                {
                    var response = await client.GetAsync("api/values");
                    var data = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(data);
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
