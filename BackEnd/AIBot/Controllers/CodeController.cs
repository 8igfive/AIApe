using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Services;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Services.CodeAnalyze;

namespace Buaa.AIBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CodeController : ControllerBase
    {
        private readonly ICodeAnalyzeService codeAnalyzeService;
        private readonly ILogger<CodeController> logger;

        public CodeController(ICodeAnalyzeService codeAnalyzeService, ILogger<CodeController> logger)
        {
            this.codeAnalyzeService = codeAnalyzeService;
            this.logger = logger;
        }

        public class CodeBody
        {
            public string Code { get; set; }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("static_analyze")]
        public async Task<IActionResult> StartAsync(CodeBody body)
        {
            var res = await codeAnalyzeService.AnalyzeAsync(body.Code);
            return Ok(new
            {
                Status = "success",
                Message = "analyze success",
                FmtCode = res.SourceCode,
                Messages = res.Messages
            });
        }
    }
}