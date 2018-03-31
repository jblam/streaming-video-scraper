using System;
using System.Net.Http;
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
