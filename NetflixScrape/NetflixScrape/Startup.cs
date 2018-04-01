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
using NetflixScrape.Websockets;

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
                            if (source?.IsDisposed == false)
                            {
                                await source.FinishAsync();
                            }
                            source = new WebsocketMessenger(webSocket);
                            source.MessageReceived += Source_MessageReceived;
                            await source.ReceiveTask;
                        }
                        if (isClient)
                        {
                            using (var client = new WebsocketMessenger(webSocket))
                            {
                                client.MessageReceived += Client_MessageReceived;
                                clients.Add(client);
                                await client.ReceiveTask;
                                clients.Remove(client);
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
                    await next();
                }

            });
        }

        private async void Source_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            await Task.WhenAll(clients.Select(c => c.SendAsync(e.Message)));
        }

        private async void Client_MessageReceived(object sender, WebsocketReceiveEventArgs e)
        {
            await source.SendAsync(e.Message);
        }

        WebsocketMessenger source;
        ISet<WebsocketMessenger> clients = new HashSet<WebsocketMessenger>();
    }
}
