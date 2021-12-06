using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Net.Mime;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private ILogger<TestController> logger;
        private static List<int> questionList;

        static TestController()
        {
            questionList = new List<int>(Tools.Range(1, 256));
        }

        public TestController(ILogger<TestController> logger)
        {
            this.logger = logger;
        }

        [HttpGet("coffee")]
        public StatusCodeResult Coffee()
        {
            return StatusCode(StatusCodes.Status418ImATeapot);
        }

        [HttpPost("echojson")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult EchoJson([FromBody] JsonDocument body)
        {
            return Ok(body);
        }

        public class QuestionListRequest
        {
            public List<int> Tags { get; set; }
            public int? pt { get; set; }
            public int Number { get; set; }
        }

        [HttpPost("questions/questionlist")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult QuestionList([FromBody] QuestionListRequest req)
        {
            int pt = questionList.Count;
            if (req.pt != null)
            {
                int tmp = (int)req.pt;
                pt = Math.Min(pt, tmp);
            }
            pt--;
            List<int> ret = new List<int>();
            foreach (var i in Tools.Range(Math.Min(64, req.Number)))
            {
                if (pt < 0)
                {
                    break;
                }
                ret.Add(questionList[pt]);
                pt--;
            }
            questionList.Add(questionList.Count + 1);
            logger.LogInformation("QuestionList.Count={Count}", questionList.Count);
            return Ok(ret);
        }

        [HttpGet("questions/question")]
        [Consumes(MediaTypeNames.Application.Json)]
        public IActionResult QuestionInfo()
        {
            int qid = int.Parse(Request.Query["qid"]);
            if (qid <= 0 || qid > questionList.Last())
            {
                return NotFound();
            }
            string title = $"请问这是问题的标题吗？#{qid}";
            return Ok(new
            {
                Status = "success",
                Message = "query question information success",
                Question = new
                {
                    Title = title,
                    Remarks = "这里是题目的具体描述",
                    Creater = 1234,
                    CreateTime = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss"),
                    ModifyTime = DateTime.Now.ToString("yyyy/MM/dd-HH:mm:ss"),
                    Best = 1,
                    Tags = new int[] {1, 2, 3},
                    Answers = new int[] {1, 2, 3}
                }
            });
        }
    }
}
