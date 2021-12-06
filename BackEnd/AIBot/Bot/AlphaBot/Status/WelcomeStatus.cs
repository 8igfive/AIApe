using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class WelcomeStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.Welcome;
        private static readonly string Env = "环境";
        private static readonly string Gramma = "语言";
        private static readonly string Code = "代码";
        private static readonly string Question = "人工";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            int called = status.GetCount(Id);
            if (called == 0)
            {
                status.Clear();
                status.IncreaseCount(Id);
                sender
                    .AddMessage("您好，我叫小猿，是一个C语言智能问答机器人，" +
                        $"虽然现在还不是很智能，但是我会努力变得更加智能哒{Kaomojis.Cute}")
                    .AddMessage($"为了弥补我的不足{Kaomojis.Sad}，解决不了的问题会转人工哟。");
            }
            sender
                .AddMessage("我现在可以提供以下相关帮助：")
                .AddMessage("\t编程环境相关：IDE/编译器安装、报错信息；")
                .AddMessage("\t语言相关：标准库函数、各种语法、关键字")
                .AddMessage("\t代码相关：编写的代码的问题（目前只能人工回答）")
                .AddMessage($"请问您有哪方面的问题呢？{Kaomojis.Happy}")
                .AddPrompt(Env)
                .AddPrompt(Gramma)
                .AddPrompt(Code)
                ;

            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var receiver = context.Receiver;
            var sender = context.Sender;
            string msg = receiver.UserMessage;
            if (msg.Contains(Env))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Environment);
            }
            if (msg.Contains(Gramma))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Gramma);
            }
            if (msg.Contains(Code))
            {
                status.ClearCount(Id);
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.Code);
                return Task.FromResult(StatusId.GetCode);
            }
            if (msg.Contains(Question))
            {
                sender
                    .AddMessage("为了提高人工回答的效率，还是需要小猿我来先收集一些信息哟" +
                    $"，不可以直接进行人工回答{Kaomojis.Cute}")
                    .NewScope();
                return Task.FromResult(Id);
            }
            sender.AddMessage($"我暂时无法回答这方面的问题{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }
}
