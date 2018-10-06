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

        public async Task InvokeAsync(HttpContext context, SourceClientStore clientManager, CommandProcessor commandProcessor)
        {

            var isSource = context.Request.Path == "/ws-source";
            var isClient = context.Request.Path == "/ws";

            if (isSource || isClient)
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    if (isSource)
                    {
                        if (clientManager.Source?.IsDisposed == false)
                        {
                            await clientManager.Source.FinishAsync();
                        }
                        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        clientManager.Source = new WebsocketMessenger(webSocket);
                        await clientManager.Source.ReceiveTask;
                    }
                    if (isClient)
                    {
                        var clientSocket = await context.WebSockets.AcceptWebSocketAsync();
                        var messenger = new WebsocketMessenger(clientSocket);
                        var ticket = await clientManager.AddClient(messenger);
                        try
                        {
                            await ticket.EndTask;
                        }
                        finally
                        {
                            clientManager.Deregister(ticket);
                            await messenger.FinishAsync();
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
