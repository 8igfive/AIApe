using System;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Buaa.AIBot.Utils
{
    public static class Extensions
    {
        /// <summary>
        /// Make sure call AddScoped<GlobalCancellationTokenSource>() in Startup. 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseGlobalTimeout(this IApplicationBuilder app, TimeSpan timeout)
        {
            return app.Use(async (context, next) =>
            {
                var source = context.RequestServices.GetService<GlobalCancellationTokenSource>();
                source.Source = new CancellationTokenSource(timeout);
                try
                {
                    await next.Invoke();
                }
                catch (OperationCanceledException)
                {
                    context.Response.StatusCode = Microsoft.AspNetCore.Http.StatusCodes.Status500InternalServerError;
                    context.Response.Headers.Add("Buaa-AIApe-Timeout", "true");
                }
            });
        }
    }
}
