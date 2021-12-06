using Buaa.AIBot.Bot.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.BetaBot.Status
{
    public static class EndingStatuses
    {
    }

    public class CreateQuestionStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.CreateQuestion;

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            context.Sender.AddMessage(SentenceGeneration.Choose(
                $"小猿已经帮您创建好提问模板，请通过导航栏前往提问页面完善您的问题"
                ))
                .AddPrompt("已经完成提问")
                .AddPrompt("我不想提问")
                ;
            return Task.CompletedTask;
        }

        protected override Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            return Task.FromResult(StatusId.GetFeedBack);
        }
    }

    public class GetFeedBackStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.GetFeedBack;
        public static readonly string Skip = "跳过";
        public static readonly string SkipButton = "【跳过】";
        public static readonly string Advice = "有建议（请直接在聊天框输入）";

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"感谢您的使用，请问您有没有什么建议给小猿呢？"
                    ))
                .AddPrompt(Advice)
                .AddPrompt(SkipButton)
                ;
            return Task.CompletedTask;
        }

        protected override Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var msg = context.Receiver.UserMessage;
            if (Advice == msg)
            {
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"人家说了让你直接在聊天框输入了啦，为什么还是要点这个选项啊{Kaomojis.Sad}",
                        $"你这样显得我没法让这个选项卡无法选中就很菜，给点面子好不好{Kaomojis.Cute}"
                        ))
                    .NewScope()
                    ;
                return Task.FromResult(Id);
            }
            if (msg != SkipButton && msg != Skip)
            {
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"小猿已经收到您的建议咯{Kaomojis.Happy}"
                        ))
                    .AddMessage(SentenceGeneration.Choose(
                        $"小猿会尽快将您的建议传达给开发组哒{Kaomojis.Cute}"
                        ))
                    ;
            }
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"再次感谢您的使用{Kaomojis.Happy}"
                    ))
                .NewScope();
            status.Clear();
            return Task.FromResult(StatusId.Welcome);
        }
    }
}
