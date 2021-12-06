using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot.WorkingModule;
using Microsoft.Extensions.Configuration;
using Buaa.AIBot.Bot.BetaBot;

namespace Buaa.AIBot.Bot
{
    public static class Extensions
    {
        public static IServiceCollection AddEchoBot(this IServiceCollection services, IConfiguration config)
        {
            var options = new BotRunnerOptions<EchoBot.StatusEnum>()
            {
                StatusPool = new StatusContainerPoolInMemory<EchoBot.StatusEnum>(),
                BehaviourPool = new StatusBehaviourPool<EchoBot.StatusEnum>(EchoBot.GetStatusBehaviours()),
                InitStatus = new BotStatus<EchoBot.StatusEnum>()
                {
                    Status = EchoBot.StatusEnum.Welcome
                }
            };
            services
                .AddWorkingModule(config)
                .AddTransient<IBotRunner>(
                services => new BotRunner<EchoBot.StatusEnum>(options, services.GetService<IWorkingModule>()));

            return services;
        }

        public static IServiceCollection AddAlphaBot(this IServiceCollection services, IConfiguration config)
        {
            var options = new BotRunnerOptions<AlphaBot.StatusId>()
            {
                StatusPool = new StatusContainerPoolInMemory<AlphaBot.StatusId>(),
                BehaviourPool = new StatusBehaviourPool<AlphaBot.StatusId>(AlphaBot.Configuration.GetStatusBehaviours()),
                InitStatus = new BotStatus<AlphaBot.StatusId>()
                {
                    Status = AlphaBot.StatusId.Welcome
                }
            };
            services
                .AddWorkingModule(config)
                .AddTransient<IBotRunner>(
                services => new BotRunner<AlphaBot.StatusId>(options, services.GetService<IWorkingModule>()));

            return services;
        }

        public static IServiceCollection AddBetaBot(this IServiceCollection services, IConfiguration config)
        {
            var statusPool = new StatusContainerPoolInMemory<BetaBot.StatusId>();
            var options = new BotRunnerOptions<BetaBot.StatusId>()
            {
                StatusPool = statusPool,
                BehaviourPool = new StatusBehaviourPool<BetaBot.StatusId>(BetaBot.Configuration.GetStatusBehaviours()),
                InitStatus = new BotStatus<BetaBot.StatusId>()
                {
                    Status = BetaBot.StatusId.Welcome
                }
            };
            services
                .AddWorkingModule(config)
                .AddTransient<IBotRunner>(
                services => new BotRunner<BetaBot.StatusId>(options, services.GetService<IWorkingModule>()))
                .AddTransient(services => new QuestionTemplateGenerater(statusPool))
                ;

            return services;
        }
    }
}
