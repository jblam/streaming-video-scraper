using JBlam.NetflixScrape.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    public class SourceClientMiddleware
    {
        readonly RequestDelegate next;
        public SourceClientMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task InvokeAsync(HttpContext context, SourceClientStore clientManager)
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
                        if (clientManager.Source?.IsDisposed == false)
                        {
                            await clientManager.Source.FinishAsync();
                        }
                        clientManager.Source = new WebsocketMessenger(webSocket);
                        await clientManager.Source.ReceiveTask;
                    }
                    if (isClient)
                    {
                        using (var client = new WebsocketMessenger(webSocket))
                        {
                            clientManager.AddClient(client);
                            await client.ReceiveTask;
                            clientManager.RemoveClient(client);
                        }

                    }
                }
                else
                {
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await next(context);
            }
        }
    }
}
