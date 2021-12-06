using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class SourceCodeAnalyzer
    {
        public static readonly string SourceCode = "SourceCode";
        public static readonly string WrongCaseInput = "WrongCaseInput";
        public static readonly string WrongCaseOutput = "WrongCaseOutput";

        public Task AnalyzeAsync(IBotStatusContainer status, IBotSender sender)
        {
            bool found = false;
            found |= CheckoutChineseComma(status, sender);
            if (!found)
            {
                sender.AddMessage("抱歉未能帮您找到可能的错误");
            }
            return Task.CompletedTask;
        }

        private bool CheckoutChineseComma(IBotStatusContainer status, IBotSender sender)
        {
            bool sourceComma = false;
            bool inputComma = false;
            bool outputComma = false;
            {
                string source = status.Get<string>(SourceCode);
                if (source.Contains("，"))
                {
                    sourceComma = true;
                    sender.AddMessage("您的源代码中含有中文逗号，请检查一下。");
                }
            }
            {
                string input;
                bool flag = status.TryGet<string>(WrongCaseInput, out input);
                if (flag && input.Contains("，"))
                {
                    inputComma = true;
                    sender.AddMessage("您的错误样例的输入包括中文逗号，请检查一下。");
                }
            }
            {
                string output;
                bool flag = status.TryGet<string>(WrongCaseOutput, out output);
                if (flag && output.Contains("，"))
                {
                    outputComma = true;
                    sender.AddMessage("您的错误样例的输出包括中文逗号，请检查一下。");
                }
            }
            if (inputComma && !sourceComma)
            {
                sender.AddMessage("您的错误样例输入包括中文逗号，但是您的源代码不包括中文逗号，请检查输入的逗号和scanf中的逗号是否一致。");
            }
            if (outputComma && !sourceComma)
            {
                sender.AddMessage("您的错误样例的期望输出包括中文逗号，但是您的源代码不包括中文逗号，请检查输出的逗号和printf中的逗号是否一致。");
            }
            bool ret = sourceComma || inputComma || outputComma;
            if (ret)
            {
                sender.NewScope();
            }
            return ret;
        }
    }
}
