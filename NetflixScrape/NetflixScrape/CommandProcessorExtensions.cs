using JBlam.NetflixScrape.Server.Comms;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    public static class CommandProcessorExtensions
    {
        public static IApplicationBuilder UseSocketCommandProcessor(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SourceClientMiddleware>();
        }
        public static void AddSocketCommandProcessor(this IServiceCollection serviceCollection)
        {
            var hostProcessor = HostCommandProcessor.TryCreate();
            var commandProcessor = new CommandProcessor(hostProcessor);
            var sourceClientStore = new SourceClientStore(commandProcessor);
            serviceCollection.AddSingleton(sourceClientStore);
        }
    }
}
