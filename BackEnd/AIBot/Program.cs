// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.12.2

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Buaa.AIBot.Utils.Logging;
using System;
using System.Threading.Tasks;
using Buaa.AIBot.TimedTask;

namespace Buaa.AIBot
{
    public static class Configuration
    {
        public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
        {
            var settings = new LoggingSettings()
            {
                ConsoleLevel = Serilog.Events.LogEventLevel.Information,
                EnableIpLog = false,
                // EnableConsoleLog = false,
            };
            builder.ConfigureLoggingWithSerilog(settings);
            return builder;
        }
    }

    public class Program
    {
        public static readonly string LOG_FILE_PATH = "";

        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateBootstrapLogger();

            try
            {
                PrintWelcomeInfo();
                var host = CreateHostBuilder(args).Build();
                Log.Information("Host build success!");

                var timedTaskManager = new TimedTaskManager(host);
                timedTaskManager.RegisterTask(NLPSynchronizer.DEFAULT.SynchronizeQuestionDataAsync, TimeSpan.FromHours(1));
                timedTaskManager.RegisterTask(HotValueFresher.DEFAULT.FreshHotValueAsync, TimeSpan.FromHours(4));
                Log.Information("TimedTasks build success!");


                Log.Information("Starting...");
                var hostRunTask = host.RunAsync();
                await Task.Delay(TimeSpan.FromSeconds(1));
                timedTaskManager.Start();
                try
                {
                    await hostRunTask;
                }
                finally
                {
                    timedTaskManager.Stop();
                }

                await Task.Delay(TimeSpan.FromSeconds(1));
                Log.Information("Host terminated success");
                return 0;
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                // Configure logging
                .ConfigureLogging()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

        private static void PrintWelcomeInfo()
        {
            var messages = new string[]
            {
                @"                                                                               ",
                @"                                                                               ",
                @"                                                                               ",
                @"                                                                               ",
                @"                                                                               ",
                @"                                                                               ",
                @"                                    _ooOoo_                                    ",
                @"                                   o8888888o                                   ",
                 "                                   88\" . \"88                                 ",
                @"                                   (| -_- |)                                   ",
                @"                                   O\  =  /O                                   ",
                @"                                ____/`---'\____                                ",
                @"                              .'  \\|     |//  `.                              ",
                @"                             /  \\|||  :  |||//  \                             ",
                @"                            /  _||||| -:- |||||-  \                            ",
                @"                            |   | \\\  -  /// |   |                            ",
                @"                            | \_|  ''\---/''  |   |                            ",
                @"                            \  .-\__  `-`  ___/-. /                            ",
                @"                          ___`. .'  /--.--\  `. . __                           ",
                @"                       ."" '<  `.___\_<|>_/___.'  >'"".                        ",
                @"                      | | :  `- \`.;`\ _ /`;.`/ - ` : | |                      ",
                @"                      \  \ `-.   \_ __\ /__ _/   .-` /  /                      ",
                @"                 ======`-.____`-.___\_____/___.-`____.-'======                 ",
                @"                                    `=---='                                    ",
                @"                 ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                 ",
                @"                                                                               ",
                @"                               佛祖保佑   永无BUG                                ",
                @"                                                                               ",
                @"                                                                               ",
                @"                                                                               ",


                @"===============================================================================",
                @"===============================================================================",
                @"       ___          ________              ___         _________     __________ ",
                @"      /   \        |__    __|            /   \       |   ____  \   |   _______|",
                @"     /  .  \          |  |              /  .  \      |  |    \  \  |  |        ",
                @"    /  / \  \         |  |             /  / \  \     |  |     |  | |  |______  ",
                @"   /  /___\  \        |  |            /  /___\  \    |  |____/  /  |   ______| ",
                @"  /  _______  \       |  |           /  _______  \   |   ______/   |  |        ",
                @" /  /       \  \   ___|  |___       /  /       \  \  |  |          |  |_______ ",
                @"/__/         \__\ |__________|     /__/         \__\ |__|          |__________|",
                @"===============================================================================",
                @"===============================================================================",


                @"AIApe 正在启动...                                                               ",
                @"                                                                               ",
                @"                                                                               ",
            };
            foreach (var line in messages)
            {
                Log.Information(line);
            }
        }
    }
}
