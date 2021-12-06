// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with EmptyBot .NET Template version v4.12.2

using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Services;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Bot.BetaBot;

namespace Buaa.AIBot.Controllers
{
    // This ASP Controller is created to handle a request. Dependency Injection will provide the Adapter and IBot
    // implementation at runtime. Multiple different IBot implementations running at different endpoints can be
    // achieved by specifying a more specific type for the bot constructor argument.
    [Route("api/[controller]")]
    [ApiController]
    public class BotController : ControllerBase
    {
        private readonly IBotRunner botRunner;
        private readonly IUserService userService;
        private readonly QuestionTemplateGenerater questionTemplateGenerater = null;
        private readonly Bot.WorkingModule.NaturalAnswerGenerator naturalAnswerGenerator = null;

        public BotController(IBotRunner botRunner, IUserService userService, QuestionTemplateGenerater questionTemplateGenerater, Bot.WorkingModule.NaturalAnswerGenerator naturalAnswerGenerator)
        {
            this.botRunner = botRunner;
            this.userService = userService;
            this.questionTemplateGenerater = questionTemplateGenerater;
            this.naturalAnswerGenerator = naturalAnswerGenerator;
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("start")]
        public async Task<IActionResult> StartAsync(BotBody body)
        {
            int uid = userService.GetUidFromToken(Request);
            OutputInfo info = await botRunner.Start((uid));
            return Ok(info);
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("message")]
        public async Task<IActionResult> MessageAsync(BotBody body)
        {
            string message = (body.Message == null)? "" : body.Message;
            int uid = userService.GetUidFromToken(Request);
            OutputInfo info = await botRunner.Run(uid, new InputInfo

            {
                Message = message
            });
            return Ok(info);
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpGet("question_template")]
        public async Task<IActionResult> QuestionTemplateAsync()
        {
            if (questionTemplateGenerater == null)
            {
                return NotFound(new
                {
                    Status = "notSupport",
                    Message = "question template is disabled"
                });
            }
            bool force = false;
            if (HttpContext.Request.Query.TryGetValue("f", out var f))
            {
                force = (f == "1");
            }
            int uid = userService.GetUidFromToken(Request);
            var res = await questionTemplateGenerater.GenerateAsync(uid, force);
            return Ok(new
            {
                Status = "success",
                Message = "get question template success",
                Template = res
            });
        }

        public class QuestionAndAnswerBody
        {
            public List<string> Questions {get; set;}
            public List<string> Answers { get; set; }
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add_natrual")]
        public async Task<IActionResult> AddNatrualQuestionAndAnswerAsync(QuestionAndAnswerBody body)
        {
            if (naturalAnswerGenerator == null)
            {
                return NotFound(new
                {
                    Status = "notSupport",
                    Message = "natural answer is disabled"
                });
            }
            await naturalAnswerGenerator.AddQuestionAndAnswersAsync(body.Questions, body.Answers);
            // TODO
            return Ok(new 
            {
                Status = "success",
                Message = "success"
            });
        }
    }
}
