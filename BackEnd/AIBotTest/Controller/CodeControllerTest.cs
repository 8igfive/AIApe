using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;

using Buaa.AIBot.Services;
using Buaa.AIBot.Controllers;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Utils;

using Buaa.AIBot.Bot.BetaBot;
using Buaa.AIBot.Bot.WorkingModule;
using Buaa.AIBot.Services.CodeAnalyze;


namespace AIBotTest.Controller
{
    public class CodeControllerTest
    {
        private Mock<ICodeAnalyzeService> mockCAS;

        private Mock<ILogger<CodeController>> mockLog;

        public CodeControllerTest()
        {
            mockCAS = new Mock<ICodeAnalyzeService>();
            mockLog = new Mock<ILogger<CodeController>>();
        }

        [Fact]
        public async Task StartAsyncTest()
        {
            var controller = new CodeController(mockCAS.Object, mockLog.Object);
            mockCAS.Setup(cas => cas.AnalyzeAsync(It.IsAny<string>())).ReturnsAsync(new CodeAnalyzeService.CodeAnalyzeResult
            {
                SourceCode = "",
                Messages = new List<CodeAnalyzeService.CodeAnalyzeResult.CodeAnalyzeMessage>()
            }
            );
            var ret = await controller.StartAsync(new CodeController.CodeBody
            {
                Code = ""
            });
            var okRet = Assert.IsType<OkObjectResult>(ret);
        }
    }
}