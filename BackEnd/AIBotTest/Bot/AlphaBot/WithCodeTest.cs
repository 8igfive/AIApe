using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.WorkingModule;
using Xunit;

using StatusId = Buaa.AIBot.Bot.AlphaBot.StatusId;

namespace AIBotTest.Bot.AlphaBot
{
    public class WithCodeTest : AlphaBotTestBase
    {

        public WithCodeTest()
        {
            InitTags();
            SetupMocks();
        }

        [Fact]
        public async Task WithInputAndOutput_WithoutQuestion()
        {
            var ret = await Run(new string[]
            {
                "代码",
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a;\n\t" +
                    "scanf(\"%d\", &a);\n\t" +
                    "printf(\"%d\n\", a);\n\t" +
                    "return 0;\n" +
                "}",
                "是",
                "1",
                "2",
                "解决了"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task WithInputAndOutput_ChangedWithoutQuestion()
        {
            string code1 =
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a;\n\t" +
                    "scanf(\"%d\", &a);\n\t" +
                    "printf(\"%d\n\", a);\n\t" +
                    "return 0;\n" +
                "}";
            string code2 =
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a;\n\t" +
                    "scanf(\"%d\", &a);\n\t" +
                    "printf(\"%d\n\", a + 1);\n\t" +
                    "return 0;\n" +
                "}";
            var ret = await Run(new string[]
            {
                "代码",
                code1,
                "是",
                "1",
                "2",
                "修改了源代码，但没有解决",
                code2,
                "是",
                "1",
                "2",
                "已解决"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task WithInputAndOutput_WithQuestion()
        {
            string simple = "WithCode";
            string desc = "这是一个测试用WithCode问题";
            var ret = await Run(new string[]
            {
                "代码",
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a;\n\t" +
                    "scanf(\"%d\", &a);\n\t" +
                    "printf(\"%d\n\", a);\n\t" +
                    "return 0;\n" +
                "}",
                "是",
                "1",
                "2",
                "没有修改源代码，也没有解决",
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
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.Code]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OutputOnly_WithQuestion()
        {
            string simple = "WithCode";
            string desc = "这是一个测试用WithCode问题";
            var ret = await Run(new string[]
            {
                "代码",
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a = 0;\n\t" +
                    "printf(\"%d\n\", a++);\n\t" +
                    "return 0;\n" +
                "}",
                "该程序不需要输入",
                "1",
                "没有修改源代码，也没有解决",
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
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.Code]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task WithoutInputAndOutput_WithQuestion()
        {
            string simple = "WithCode";
            string desc = "这是一个测试用WithCode问题";
            var ret = await Run(new string[]
            {
                "代码",
                "#include <stdio.h>\n" +
                "int main()\n" +
                "{\n\t" +
                    "int a;\n\t" +
                    "scanf(\"%d\", &a);\n\t" +
                    "printf(\"%d\n\", a);\n\t" +
                    "return 0;\n" +
                "}",
                "否",
                "没有修改源代码，也没有解决",
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
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.Code]],
                param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }
    }
}
