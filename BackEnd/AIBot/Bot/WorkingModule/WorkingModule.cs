using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public static class Configuration
    {
        public static IServiceCollection AddWorkingModule(this IServiceCollection services, IConfiguration config)
        {
            string gccWorkDir = config.GetSection("Path").GetValue<string>("GccWorkDir");
            var betaBot = config.GetSection("BetaBot");
            double minScore = betaBot.GetValue<double>("MinScore");
            int continueCount = betaBot.GetValue<int>("ContinueCount");
            double secondLimitScoreTimes = betaBot.GetValue<double>("SecondLimitScoreTimes");
            services
                .AddTransient<IWorkingModule, WorkingModule>()
                .AddTransient<QuestionBuilder>()
                .AddTransient<NaturalAnswerGenerator>()
                .AddSingleton<GovernmentInstallingInfo>()
                .AddSingleton<IdeAndCompilerDocumentCollection>()
                .AddSingleton<IGccHandlerFactory>(provider =>
                    new GccHandlerFactory(gccWorkDir, provider.GetRequiredService<ILogger<GccHandlerFactory>>()))
                .AddSingleton<SourceCodeAnalyzer>()
                .AddSingleton<DocumentCollection>()
                .AddTransient(provider =>
                    new InnerRepoSearcher(
                        provider.GetRequiredService<Services.IQuestionService>(),
                        provider.GetRequiredService<Services.INLPService>(),
                        minScore,
                        continueCount,
                        secondLimitScoreTimes))
                .AddTransient<OuterRepoSearcher>()
                ;
            return services;
        }
    }

    public interface IWorkingModule
    {
        T Get<T>();
        QuestionBuilder GetQuestionBuilder();
        GovernmentInstallingInfo GetGovernmentInstallingInfo();
        IdeAndCompilerDocumentCollection GetIdeAndCompilerDocumentCollection();
        IGccHandlerFactory GetGccHandlerFactory();
        SourceCodeAnalyzer GetSourceCodeAnalyzer();
        DocumentCollection GetDocumentCollection();
        OuterRepoSearcher GetOuterRepoSearcher();
        InnerRepoSearcher GetInnerRepoSearcher();

        NaturalAnswerGenerator GetNaturalAnswerGenerator();
    }

    public class WorkingModule : IWorkingModule
    {
        public IServiceProvider Services { get; }

        public WorkingModule(IServiceProvider services)
        {
            this.Services = services;
        }

        public T Get<T>()
        {
            return Services.GetRequiredService<T>();
        }

        public QuestionBuilder GetQuestionBuilder()
        {
            return Services.GetService<QuestionBuilder>();
        }

        public GovernmentInstallingInfo GetGovernmentInstallingInfo()
        {
            return Services.GetService<GovernmentInstallingInfo>();
        }

        public IdeAndCompilerDocumentCollection GetIdeAndCompilerDocumentCollection()
        {
            return Services.GetService<IdeAndCompilerDocumentCollection>();
        }

        public IGccHandlerFactory GetGccHandlerFactory()
        {
            return Services.GetService<IGccHandlerFactory>();
        }

        public SourceCodeAnalyzer GetSourceCodeAnalyzer()
        {
            return Services.GetService<SourceCodeAnalyzer>();
        }

        public DocumentCollection GetDocumentCollection()
        {
            return Services.GetService<DocumentCollection>();
        }

        public OuterRepoSearcher GetOuterRepoSearcher()
        {
            return Services.GetService<OuterRepoSearcher>();
        }

        public InnerRepoSearcher GetInnerRepoSearcher()
        {
            return Services.GetService<InnerRepoSearcher>();
        }

        public NaturalAnswerGenerator GetNaturalAnswerGenerator()
        {
            return Services.GetRequiredService<NaturalAnswerGenerator>();
        }
    }
}
