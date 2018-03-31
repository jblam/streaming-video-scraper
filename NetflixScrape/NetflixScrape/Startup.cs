using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetflixScrape
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();
            app.UseMvc();

            app.Use(async (context, next) =>
            {
                var isSource = context.Request.Path == "/ws-source";
                var isClient = context.Request.Path == "/ws";

                if (isSource || isClient)
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        if (isSource)
                        {
                            if (source?.CloseStatus != null)
                            {
                                await source.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Another source registered", CancellationToken.None);
                            }
                            source = webSocket;
                            await DoSourceAsync(context, webSocket);
                        }
                        if (isClient)
                        {
                            clients.Add(webSocket);
                        }
                        await Echo(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await next();
                }

            });
        }

        WebSocket source;
        ISet<WebSocket> clients = new HashSet<WebSocket>();

        static ArraySegment<byte> sourceClosingMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes("Source closing"));

        async Task DoSourceAsync(HttpContext context, WebSocket sourceSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await sourceSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var message = new ArraySegment<byte>(buffer, 0, result.Count);
                await Task.WhenAll(clients.Select(s => s.SendAsync(message, result.MessageType, result.EndOfMessage, CancellationToken.None)));

                result = await sourceSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await Task.WhenAll(clients.Select(s => s.SendAsync(sourceClosingMessage, WebSocketMessageType.Text, true, CancellationToken.None)));
            await sourceSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private static async Task Echo(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
