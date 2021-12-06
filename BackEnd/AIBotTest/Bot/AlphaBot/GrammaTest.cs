using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Buaa.AIBot.Bot.WorkingModule;

using StatusId = Buaa.AIBot.Bot.AlphaBot.StatusId;


namespace AIBotTest.Bot.AlphaBot
{
    public class GrammaTest : AlphaBotTestBase
    {
        public GrammaTest()
        {
            InitTags();
            SetupMocks();
        }

        [Fact]
        public async Task OnStandardLibary_WithoutQuestion()
        {
            var ret = await Run(new string[]
            {
                "语言",
                "标准库",
                "thx",
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStandardLibary_WithQuestion()
        {
            string simple = "OnStandardLibary";
            string desc = "这是一个测试用OnStandardLibary问题";
            var ret = await Run(new string[]
            {
                "语言",
                "标准库",
                "NO",
                simple,
                "没解决",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.StdLib]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStandardLibary_WithQuestion_TooLongSimple()
        {
            var titleBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.QuestionTitleMaxLength))
            {
                titleBuilder.Append('t');
            }
            string simple = titleBuilder.ToString();
            string desc = "这是一个测试用OnStandardLibary问题";
            var ret = await Run(new string[]
            {
                "语言",
                "标准库",
                "NO",
                simple + 't',
                simple,
                "没解决",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.StdLib]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStandardLibary_WithQuestion_EmptySimple()
        {
            string simple = "OnStandardLibary";
            string desc = "这是一个测试用OnStandardLibary问题";
            var ret = await Run(new string[]
            {
                "语言",
                "标准库",
                "NO",
                "",
                simple,
                "没解决",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.StdLib]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStandardLibary_WithQuestion_EmptyDetail()
        {
            string simple = "OnStandardLibary";
            string desc = "这是一个测试用OnStandardLibary问题";
            var ret = await Run(new string[]
            {
                "语言",
                "标准库",
                "NO",
                simple,
                "没解决",
                "",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.StdLib]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStatement_WithoutQuestion()
        {
            var ret = await Run(new string[]
            {
                "语言",
                "语句",
                "循环",
                "yes"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStatement_WithQuestionTillSearch()
        {
            string simple = "OnStatement";
            var ret = await Run(new string[]
            {
                "语言",
                "语句",
                "分支",
                "NO",
                simple,
                "ok"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnStatement_WithQuestion()
        {
            string simple = "OnStatement";
            string desc = "这是一个测试用OnStatement问题";
            var ret = await Run(new string[]
            {
                "语言",
                "语句",
                "函数",
                "NO",
                simple,
                "没解决",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.Statement]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnKeyword_WithoutQuestion()
        {
            var ret = await Run(new string[]
            {
                "语言",
                "关键字",
                "for",
                "OK"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnKeyword_WithQuestion()
        {
            string simple = "OnKeyword";
            string desc = "这是一个测试用OnKeyword问题";
            var ret = await Run(new string[]
            {
                "语言",
                "关键字",
                "auto",
                "NO",
                simple,
                "没解决",
                desc,
                "阿巴阿巴"
            });

            Assert.NotNull(questionAddParams.Value);
            var param = questionAddParams.Value;
            Assert.Equal(uid, param.Creater);
            Assert.Equal(simple, param.Title);
            Assert.Contains(desc, param.Remarks);
            Assert.Contains(
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.Keywords]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }
    }
}
