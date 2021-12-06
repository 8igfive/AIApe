using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using System.Text.Json;

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
    public class TestControllerTest
    {
        private Mock<ILogger<TestController>> mockLog;

        public TestControllerTest()
        {
            mockLog = new Mock<ILogger<TestController>>();
        }

        public void CoffeeTest()
        {
            var controller = new TestController(mockLog.Object);
            var ret = controller.Coffee();
            Assert.IsType<StatusCodeResult>(ret);
        }

        // public void EchoJsonTest()
        // {
        //     var controller = new TestController(mockLog.Object);
        //     var ret = controller.EchoJson(new JsonDocument(""));
        //     Assert.IsType<StatusCodeResult>(ret);
        // }

        [Fact]
        public void QuestionListTest()
        {
            var controller = new TestController(mockLog.Object);
            var ret = controller.QuestionList(new TestController.QuestionListRequest
            {
                Tags = new List<int>(),
                Number = 10
            });
            Assert.IsType<OkObjectResult>(ret);
        }
    }
}