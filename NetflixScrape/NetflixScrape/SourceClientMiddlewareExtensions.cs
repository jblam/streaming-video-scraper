using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JBlam.NetflixScrape.Server
{
    public static class SourceClientMiddlewareExtensions
    {
        public static IApplicationBuilder UseSourceClientSockets(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SourceClientMiddleware>();
        }
        public static void AddSourceClientStore(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<SourceClientStore>();
        }
    }
}
