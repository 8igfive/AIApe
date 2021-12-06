using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Context;

namespace Buaa.AIBot.Utils.Logging
{
    public static class Extensions
    {
        public static IHostBuilder ConfigureLoggingWithSerilog(this IHostBuilder hostBuilder, LoggingSettings settings)
        {
            return hostBuilder
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                })
                // Configure Logging with lib Serilog
                .UseSerilog((context, services, configuration) =>
                    LoggingConfiguration.ConfigurationSerilog(settings, context, services, configuration)
                );
        }

        public static IApplicationBuilder UseIpAddressRecord(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var recorder = context.RequestServices.GetService<IpAddressRecorder>();
                using (LogContext.PushProperty("UserIp", recorder.GetUserAddress(context)))
                {
                    recorder.Logger.LogInformation("User IP = {UserIp}");
                    await next.Invoke();
                }
            });
        }

        public static IApplicationBuilder UseHttpHeaderRecord(this IApplicationBuilder app)
        {
            return app.Use(async (context, next) =>
            {
                var recorder = context.RequestServices.GetService<HttpHeaderRecorder>();
                recorder.LogRequest(context);
                await next.Invoke();
                recorder.LogResponse(context);
            });
        }
    }
}
