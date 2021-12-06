using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Buaa.AIBot.Services;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot.WorkingModule;
using Xunit;
using Buaa.AIBot.Repository;

using StatusId = Buaa.AIBot.Bot.AlphaBot.StatusId;

namespace AIBotTest.Bot.AlphaBot
{
    public class AlphaBotTestBase
    {
        protected class Result<T>
        {
            public T Value { get; set; }
        }
        protected class QuestionAddParams
        {
            public int Creater { get; set; }
            public string Title { get; set; }
            public string Remarks { get; set; }
            public IEnumerable<int> Tags { get; set; }
        }

        protected Mock<IQuestionService> questionServiceMock = new Mock<IQuestionService>();
        protected Mock<IWorkingModule> workingModuleMock = new Mock<IWorkingModule>();
        protected Mock<ICrawlerOuterRepository> crawlerOuterRepositoryMock = new Mock<ICrawlerOuterRepository>();
        protected Mock<IGccHandlerFactory> gccHandlerFactoryMock = new Mock<IGccHandlerFactory>();
        protected Mock<GccHandlerFactory.IGccHandler> gccHandlerMock = new Mock<GccHandlerFactory.IGccHandler>();


        protected Result<QuestionAddParams> questionAddParams = new Result<QuestionAddParams>();
        protected static int uidSource = 1;
        protected int uid = uidSource++;
        protected int qid = 1;
        protected Dictionary<string, int> tags = new Dictionary<string, int>();

        protected void InitTags()
        {
            int tid = 1;
            foreach (var category in Enum.GetValues<QuestionBuilder.QuestionCategory>())
            {
                tags[QuestionBuilder.CategoryToChinese[category]] = tid++;
            }
            var names = new List<string>()
            {
                ConstantStrings.OS.LinuxOS,
                ConstantStrings.OS.MacOS,
                ConstantStrings.OS.WindowsOS,
                ConstantStrings.IDE.DevCpp,
                ConstantStrings.IDE.VisualCpp,
                ConstantStrings.IDE.VS,
                ConstantStrings.IDE.VSCode,
                ConstantStrings.Compiler.Gcc,
                ConstantStrings.Compiler.Msvc,
                ConstantStrings.Compiler.Clang
            };
            foreach (var name in names)
            {
                tags[name] = tid++;
            }
        }

        protected void SetupMocks()
        {
            // QuestionBuilder
            questionServiceMock
                .Setup(qs => qs.GetTagListAsync())
                .ReturnsAsync(tags);
            questionServiceMock
                .Setup(qs => qs.AddQuestionAsync(
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IEnumerable<int>>()))
                .Callback((int creater, string title, string remarks, IEnumerable<int> tags) =>
                {
                    questionAddParams.Value = new QuestionAddParams()
                    {
                        Creater = creater,
                        Title = title,
                        Remarks = remarks,
                        Tags = tags
                    };
                })
                .ReturnsAsync(qid);
            workingModuleMock
                .Setup(wm => wm.GetQuestionBuilder())
                .Returns(() => new QuestionBuilder(questionServiceMock.Object));

            // GovernmentInstallingInfo
            workingModuleMock
                .Setup(wm => wm.GetGovernmentInstallingInfo())
                .Returns(new GovernmentInstallingInfo());

            // IdeAndCompilerDocumentCollection
            workingModuleMock
                .Setup(wm => wm.GetIdeAndCompilerDocumentCollection())
                .Returns(new IdeAndCompilerDocumentCollection());

            // GccHandlerFactory
            gccHandlerMock
                .Setup(gh => gh.CompileAsync())
                .ReturnsAsync("");
            gccHandlerMock
                .Setup(gh => gh.CleanUp());
            gccHandlerMock
                .Setup(gh => gh.CreatSourceFileAsync(It.IsAny<string>()));
            gccHandlerMock
                .Setup(gh => gh.Dispose());
            gccHandlerFactoryMock
                .Setup(ghf => ghf.CreateHandlerAsync())
                .ReturnsAsync(gccHandlerMock.Object);
            workingModuleMock
                .Setup(wm => wm.GetGccHandlerFactory())
                .Returns(gccHandlerFactoryMock.Object);

            // SourceCodeAnalyzer
            workingModuleMock
                .Setup(wm => wm.GetSourceCodeAnalyzer())
                .Returns(new SourceCodeAnalyzer());

            // DocumentCollection
            workingModuleMock
                .Setup(wm => wm.GetDocumentCollection())
                .Returns(new DocumentCollection());


            // OuterRepoSearcher
            crawlerOuterRepositoryMock
                .Setup(cor => cor.SearchAsync(It.IsAny<string>(), null))
                .ReturnsAsync(new List<Buaa.AIBot.Repository.Models.SearchResult>());
            workingModuleMock
                .Setup(wm => wm.GetOuterRepoSearcher())
                .Returns(new OuterRepoSearcher(crawlerOuterRepositoryMock.Object));
        }

        protected async Task<BotStatus<StatusId>> Run(IEnumerable<string> messages)
        {
            var options = new BotRunnerOptions<StatusId>()
            {
                StatusPool = new StatusContainerPoolInMemory<StatusId>(),
                BehaviourPool = new StatusBehaviourPool<StatusId>(
                    Buaa.AIBot.Bot.AlphaBot.Configuration.GetStatusBehaviours()),
                InitStatus = new BotStatus<StatusId>()
                {
                    Status = StatusId.Welcome
                }
            };
            var bot = new BotRunner<StatusId>(options, workingModuleMock.Object);

            await bot.Start(uid);

            foreach (var msg in messages)
            {
                var input = new InputInfo()
                {
                    Message = msg
                };
                await bot.Run(uid, input);
            }

            var status = await options.StatusPool.GetStatusAsync(uid);
            return status;
        }
    }
}
