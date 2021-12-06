using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Buaa.AIBot.Bot.WorkingModule;

using StatusId = Buaa.AIBot.Bot.AlphaBot.StatusId;

namespace AIBotTest.Bot.AlphaBot
{
    public class EnvironmentTest : AlphaBotTestBase
    {
        public EnvironmentTest()
        {
            InitTags();
            SetupMocks();
        }

        [Fact]
        public async Task OnInstalling_WithoutQuestion_ubuntu_vc()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "ubuntu 20 LTS",
                "VC++ 6.0",
                "解决了"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnInstalling_WithoutQuestion_ubuntu_vscode()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "ubuntu 20 LTS",
                "visual studio code",
                "解决了"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnInstalling_WithoutQuestion_mac_vs()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "macOS Big Sur",
                "visual studio",
                "解决了"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnInstalling_WithoutQuestion_win_dev()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "Windows 10 家庭版",
                "devcpp",
                "解决了"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnInstalling_WithQuestionTillSearch()
        {
            string simple = "OnInstalling";
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "Windows 10 家庭版",
                "devcpp",
                "no",
                simple,
                "解决了",
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnInstalling_WithQuestion()
        {
            string simple = "OnInstalling";
            string desc = "这是一个测试用OnInstalling问题";
            var ret = await Run(new string[]
            {
                "环境",
                "安装",
                "Windows 10 家庭版",
                "visual studio code",
                "no",
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
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.EnvInstalling]],
                param.Tags);
            Assert.Contains(tags[ConstantStrings.OS.WindowsOS], param.Tags);
            Assert.Contains(tags[ConstantStrings.IDE.VSCode], param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnUsing_WithoutQuestion_win_vc_unknown()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "使用",
                "win7",
                "vc++",
                "不知道",
                "yes"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnUsing_WithoutQuestion_mac_vs_msvc()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "使用",
                "mac",
                "vs",
                "msvc",
                "yes"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnUsing_WithoutQuestion_centos_vscode_gcc()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "使用",
                "CentOS",
                "vscode",
                "gcc",
                "yes"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnUsing_WithoutQuestion_win_dev_gcc()
        {
            var ret = await Run(new string[]
            {
                "环境",
                "使用",
                "Windows 10 家庭版",
                "devcpp",
                "gcc",
                "yes"
            });

            Assert.Null(questionAddParams.Value);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }

        [Fact]
        public async Task OnUsing_WithQuestion()
        {
            string simple = "OnUsing";
            string desc = "这是一个测试用OnUsing问题";
            var ret = await Run(new string[]
            {
                "环境",
                "使用",
                "Windows 10 家庭版",
                "vs code",
                "gcc",
                "no",
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
                tags[QuestionBuilder.CategoryToChinese[QuestionBuilder.QuestionCategory.EnvUsing]],
                param.Tags);
            Assert.Contains(tags[ConstantStrings.OS.WindowsOS], param.Tags);
            Assert.Contains(tags[ConstantStrings.IDE.VSCode], param.Tags);
            Assert.Contains(tags[ConstantStrings.Compiler.Gcc], param.Tags);
            Assert.Equal(StatusId.Welcome, ret.Status);
        }
    }
}
