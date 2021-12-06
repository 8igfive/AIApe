using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class EnvironmentStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.Environment;
        private readonly string Installing = "安装";
        private readonly string Using = "使用";


        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            status.IncreaseCount(Id);
            sender
                .AddMessage($"请问您的问题是安装问题，还是使用问题呢？{Kaomojis.Cute}")
                .AddPrompt(Installing)
                .AddPrompt(Using)
                ;

            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var receiver = context.Receiver;
            var sender = context.Sender;
            string msg = receiver.UserMessage;
            if (msg.ToLowerContainsAny(Installing, "install", "配置"))
            {
                status.ClearCount(Id);
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.EnvInstalling);
                return Task.FromResult(StatusId.GetOSForInstalling);
            }
            if (msg.ToLowerContainsAny(Using, "编译", "报错", "运行"))
            {
                status.ClearCount(Id);
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.EnvUsing);
                return Task.FromResult(StatusId.GetOSForUsing);
            }
            sender.AddMessage($"您又说我听不懂的话了呜呜呜{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }
}
