using System;
using System.Collections.Generic;
using System.Linq;
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
    public class LoggingSettings
    {
        /// <summary>
        /// Enable Ip loggin on file
        /// </summary>
        public bool EnableIpLog { get; set; } = true;

        /// <summary>
        /// Enable Log on Console.
        /// </summary>
        public bool EnableConsoleLog { get; set; } = true;

        /// <summary>
        /// The MinimumLevel for log messages on console.
        /// </summary>
        public LogEventLevel ConsoleLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        /// Enable Log to logfile.
        /// </summary>
        public bool EnableFileLog { get; set; } = true;

        /// <summary>
        /// The MinimumLevel for log messages on logfile.
        /// </summary>
        public LogEventLevel FileLevel { get; set; } = LogEventLevel.Information;

        /// <summary>
        /// Enable Log to jsonfile.
        /// </summary>
        public bool EnableJsonLog { get; set; } = true;

        /// <summary>
        /// The MinimumLevel for log messages on jsonfile.
        /// </summary>
        public LogEventLevel JsonLevel { get; set; } = LogEventLevel.Debug;

        /// <summary>
        /// Enable log on Debug.
        /// </summary>
        public bool EnableDebug { get; set; } = true;

        /// <summary>
        /// The MinimumLevel for log messages on Debug.
        /// </summary>
        public LogEventLevel DebugLevel { get; set; } = LogEventLevel.Debug;

        /// <summary>
        /// The full class-name of the classes whose log message will not be print to console.
        /// </summary>
        public HashSet<string> ConsoleExcludeClasses { get; } = new HashSet<string>()
        {
            "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
            "Microsoft.AspNetCore.Routing.EndpointMiddleware",
        };

        /// <summary>
        /// The full class-name of the classes whose log message will not be write to logfile.
        /// </summary>
        public HashSet<string> FileExcludeClasses { get; } = new HashSet<string>()
        {
            "Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor",
            "Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker",
            "Microsoft.AspNetCore.Routing.EndpointMiddleware",
        };
    }

    public class LoggingConfiguration
    {
        private LoggingSettings settings;
        private HostBuilderContext context;
        private IServiceProvider services;
        private LoggerConfiguration configuration;

        private LoggingConfiguration(
            LoggingSettings settings,
            HostBuilderContext context,
            IServiceProvider services,
            LoggerConfiguration configuration
        )
        {
            this.settings = settings;
            this.context = context;
            this.services = services;
            this.configuration = configuration;
        }

        private void Run()
        {
            configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                // Enable LogContext.PushProperty().
                .Enrich.FromLogContext()
                // Set default logLevel (cover appsetting.json).
                .MinimumLevel.Verbose()
                ;
            // Print log message to Console.
            if (settings.EnableConsoleLog)
            {
                Console();
            }

            // Write log message to logfile.
            if (settings.EnableFileLog)
            {
                File();
            }

            // Write full log message to jsonfile.
            if (settings.EnableJsonLog)
            {
                Json();
            }

            // Write IP to IP-logfile.
            if (settings.EnableIpLog)
            {
                Ip();
            }

            // Write full log message to debugger.
            if (settings.EnableDebug)
            {
                Debug();
            }
        }

        private void Console()
        {
            Log.Information("Enable Console logging");
            configuration.WriteTo.Logger(subLogger =>
            {
                // Logging exclude List.
                subLogger
                    .MinimumLevel.Information()
                    // Exclude log-info from certain Classes.
                    .Filter.ByExcluding(
                        Matching.WithProperty<string>("SourceContext", t => settings.ConsoleExcludeClasses.Contains(t)))
                    .WriteTo.Console(
                        // The min level to print.
                        restrictedToMinimumLevel: settings.ConsoleLevel
                    );
            });
        }

        private void File()
        {
            var path = context.Configuration.GetSection("Path");
            var logFileDir = Path.GetFullPath(path.GetValue<string>("LogFileDir"));
            Log.Information("Saving Log file in {LogFileDir}", logFileDir);
            configuration.WriteTo.Logger(subLogger =>
            {
                subLogger
                    // Exclude log-info from certain Classes.
                    .Filter.ByExcluding(
                        Matching.WithProperty<string>("SourceContext", t => settings.FileExcludeClasses.Contains(t)))
                    .WriteTo.File(
                        // Reading path of logfile from configuration.
                        path: Path.Combine(logFileDir, ".log"),
                        // The min level write to logfile.
                        restrictedToMinimumLevel: settings.FileLevel,
                        // allow buffering to reduce disk writting.
                        buffered: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{RequestId}] {Message:lj}{NewLine}{Exception}",
                        // flush buffer each second.
                        flushToDiskInterval: TimeSpan.FromSeconds(1),
                        // create a new logfile each day.
                        rollingInterval: RollingInterval.Day,
                        // when logfile reach 4 MiB, create a new logfile.
                        fileSizeLimitBytes: 4 * 1024 * 1024,
                        rollOnFileSizeLimit: true
                        );
            });
        }

        private void Json()
        {
            var path = context.Configuration.GetSection("Path");
            var logJsonDir = Path.GetFullPath(path.GetValue<string>("LogJsonDir"));
            Log.Information("Saving Log json in {LogJsonDir}", logJsonDir);
            configuration.WriteTo.Logger(subLogger =>
            {
                subLogger.WriteTo.File(
                    formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
                    // Reading path of logfile from configuration
                    path: Path.Combine(logJsonDir, ".json"),
                    // The min level write to logfile.
                    restrictedToMinimumLevel: settings.JsonLevel,
                    // allow buffering to reduce disk writting
                    buffered: true,
                    // flush buffer every 10 second
                    flushToDiskInterval: TimeSpan.FromSeconds(10),
                    // create a new logfile each day
                    rollingInterval: RollingInterval.Day,
                    // when logfile reach 4 MiB, create a new logfile
                    fileSizeLimitBytes: 4 * 1024 * 1024,
                    rollOnFileSizeLimit: true
                );
            });
        }

        private void Ip()
        {
            configuration.WriteTo.Logger(subLogger =>
            {
                var path = context.Configuration.GetSection("Path");
                var logIpDir = Path.GetFullPath(path.GetValue<string>("LogIpDir"));
                Log.Information("Saving IP Logging in {LogIpDir}", logIpDir);
                subLogger
                    // The min level write to logfile
                    .MinimumLevel.Debug()
                    // Exclude log-info from certain Classes
                    .Filter.ByIncludingOnly(
                        Matching.WithProperty<string>("SourceContext", str => str == typeof(IpAddressRecorder).FullName)
                        )
                    .WriteTo.File(
                        // Reading path of logfile from configuration.
                        path: Path.Combine(logIpDir, ".log"),
                        // allow buffering to reduce disk writting.
                        buffered: true,
                        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{RequestId}] [{RemoteIp}]{NewLine}",
                        // flush buffer each second.
                        flushToDiskInterval: TimeSpan.FromSeconds(10),
                        // create a new logfile each day.
                        rollingInterval: RollingInterval.Day,
                        // when logfile reach 4 MiB, create a new logfile
                        fileSizeLimitBytes: 4 * 1024 * 1024,
                        rollOnFileSizeLimit: true
                        );
            });
        }

        private void Debug()
        {
            Log.Information("Enable Debug logging");
            configuration.WriteTo.Debug(
                restrictedToMinimumLevel: settings.DebugLevel
                );
        }

        public static void ConfigurationSerilog(
            LoggingSettings settings,
            HostBuilderContext context, 
            IServiceProvider services, 
            LoggerConfiguration configuration
        )
        {
            var runner = new LoggingConfiguration(settings, context, services, configuration);
            runner.Run();
        }
    }
}
