using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class GetSimpleDescribeStatus : IBotStatusBehaviour<StatusId>
    {
        private static readonly int minLength = 5;

        public StatusId Id => StatusId.GetSimpleDescribe;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender.AddMessage($"可以简单描述一下您的问题嘛（{Constants.QuestionTitleMaxLength}字以内）{Kaomojis.Cute}");
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var receiver = context.Receiver;
            var sender = context.Sender;
            string msg = receiver.UserMessage;
            if (msg.Length > Constants.QuestionTitleMaxLength)
            {
                sender
                    .AddMessage($"问题太长啦{Kaomojis.Sad}")
                    .AddMessage($"详细描述我过一会儿再问您")
                    .AddMessage($"不能多于{Constants.QuestionTitleMaxLength}个字哟")
                    .NewScope();
                return Task.FromResult(Id);
            }
            if (msg.Length < minLength)
            {
                sender
                    .AddMessage($"问题太短啦{Kaomojis.Sad}")
                    .AddMessage($"至少也得有{minLength}个字吧？")
                    .NewScope();
                return Task.FromResult(Id);
            }
            status.Put(Key.SimpleDescribe, msg);
            // TODO
            return Task.FromResult(StatusId.ShowSearchResult);
        }
    }

    public class ShowSearchResultStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.ShowSearchResult;
        private static readonly string Finish = "解决了";
        private static readonly string NeedQuestion = "没解决";

        public async Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            if (status.GetCount(Id) == 0)
            {
                status.IncreaseCount(Id);
                await context.Worker.GetOuterRepoSearcher().SendSearchResultAsync(status, sender);
            }
            context.Sender
                .NewScope()
                .AddMessage("请问是否解决了您的问题呢？")
                .AddPrompt(Finish).AddPrompt(NeedQuestion);
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(NeedQuestion, "没", "未", "NO", "不") || msg.ToLowerInvariant() == "n")
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetDetails);
            }
            if (msg.ToLowerContainsAny(Finish, "谢谢", "OK", "finish", "已解决", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
            {
                status.ClearCount(Id);
                sender
                    .AddMessage($"很荣幸能够帮到您{Kaomojis.Happy}")
                    ;
                return Task.FromResult(StatusId.Welcome);
            }
            sender.AddMessage($"抱歉我不明白您在说什么{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }

    public class GetDetailsStatus : IBotStatusBehaviour<StatusId>
    {
        private static readonly int minLength = 5;
        public StatusId Id => StatusId.GetDetails;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender.AddMessage($"详细描述一下您的问题吧（字数不限）{Kaomojis.Cute}");
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var receiver = context.Receiver;
            var sender = context.Sender;
            string msg = receiver.UserMessage;
            if (msg.Length < minLength)
            {
                sender
                    .AddMessage($"您的“详细描述”太短啦{Kaomojis.Sad}")
                    .AddMessage($"至少也得有{minLength}个字吧？")
                    .NewScope();
                return Task.FromResult(Id);
            }
            status.Put(Key.DetailDescribe, msg);
            return Task.FromResult(StatusId.RunAddQuestion);
        }
    }

    public class AddQuestionStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.RunAddQuestion;

        public async Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            var builder = context.Worker.GetQuestionBuilder();
            var qid = await builder.BuildAsync(status);
            sender
                .AddMessage($"小猿已经帮您创建了提问咯{Kaomojis.Happy}")
                .AddMessage("请耐心等待人工回答~")
                .AddMessage("您可以在", false).AddQuestion(qid).AddMessage($"查看这个问题哟{Kaomojis.Cute}")
                .AddPrompt("好哒")
                ;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            return Task.FromResult(StatusId.Welcome);
        }
    }
}
