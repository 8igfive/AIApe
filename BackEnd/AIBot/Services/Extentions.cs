using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Services.CodeAnalyze;
using Buaa.AIBot.Repository;

namespace Buaa.AIBot.Services
{
    public static class Extentions
    {
        public static IServiceCollection AddCodeAnalyze(this IServiceCollection services, IConfiguration config)
        {
            string cppcheckWorkDir = config.GetSection("Path").GetValue<string>("CppCheckWorkDir");
            services
                .AddTransient<ICodeAnalyzeService, CodeAnalyzeService>()
                .AddSingleton<CppCheckResultTanslation>()
                .AddSingleton<ICppCheckCallerFactory>(provider =>
                    new CppCheckCallerFactory(cppcheckWorkDir, provider.GetRequiredService<ILogger<CppCheckCallerFactory>>()))
                ;
            return services;
        }

        public static IServiceCollection AddNLPServices(this IServiceCollection services, IConfiguration config)
        {
            var nlpConfig = config.GetSection("NLPService");
            var options = new NLPService.Options()
            {
                Name = nlpConfig.GetValue<string>("Name"),
                Password = nlpConfig.GetValue<string>("Password"),
                BaseUrl = nlpConfig.GetValue<string>("BaseUrl")
            };
            TimedTask.NLPSynchronizer.DEFAULT.NLPOptions = options;
            services.AddTransient<INLPService>(provider =>
                new NLPService(
                    provider.GetRequiredService<ILogger<NLPService>>(),
                    provider.GetRequiredService<Utils.GlobalCancellationTokenSource>(),
                    options));
            return services;
        }

        public static IServiceCollection AddHotList(this IServiceCollection services, IConfiguration config)
        {
            var hotConfig = config.GetSection("HotList");
            TimeSpan valueInterval = TimeSpan.Parse(hotConfig.GetValue<string>("ValueInterval"));
            TimeSpan listInterval = TimeSpan.Parse(hotConfig.GetValue<string>("ListInterval"));
            int length = hotConfig.GetValue<int>("Length");
            services
                .AddTransient<IHotListService>(provider =>
                    new HotListService(
                        questionRepository: provider.GetRequiredService<IQuestionRepository>(),
                        hotListFreshInterval: listInterval,
                        logger: provider.GetRequiredService<ILogger<HotListService>>(),
                        length: length
                        ))
                ;
            return services;
        }
    }
}
