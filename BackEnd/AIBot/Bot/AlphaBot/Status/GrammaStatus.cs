using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;
using static Buaa.AIBot.Bot.WorkingModule.DocumentCollection;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class GrammaStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.Gramma;
        private static readonly string StdLib = "标准库";
        private static readonly string Statement = "语句";
        private static readonly string Keyword = "关键字";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您有哪方面的语法问题呢？")
                .AddPrompt(StdLib)
                .AddPrompt(Statement)
                .AddPrompt(Keyword)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(StdLib, "std", ".h"))
            {
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.StdLib);
                return Task.FromResult(StatusId.ShowLinksForStandardLibary);
            }
            if (msg.ToLowerContainsAny(Statement, "循环", "分支", "函数", "形参", "实参", "statement"))
            {
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.Statement);
                return Task.FromResult(StatusId.GetStatementTypeForStatement);
            }
            if (msg.ToLowerContainsAny(Keyword, "关键词", "keyword"))
            {
                status.Put(WorkingModule.QuestionBuilder.Category, WorkingModule.QuestionBuilder.QuestionCategory.Keywords);
                return Task.FromResult(StatusId.GetKeywordForKeywords);
            }
            context.Sender.AddMessage($"对不起，我不明白您在说什么{Kaomojis.Sad}");
            return Task.FromResult(Id);
        }
    }

    public class ShowLinksForStandardLibaryStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.ShowLinksForStandardLibary;
        private static readonly string Yes = "是";
        private static readonly string No = "否";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage("我为您找到了如下资料：")
                    .NewScope()
                    ;
                context.Worker.GetDocumentCollection().SendStandardLibraryMessage(sender);
                status.IncreaseCount(Id);
            }
            sender
                .AddMessage("请问是否帮您解决了问题呢？")
                .AddPrompt(Yes)
                .AddPrompt(No)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(No, "没", "未", "否", "无", "No", "不"))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetSimpleDescribe);
            }
            if (msg.ToLowerContainsAny(Yes, "谢谢", "OK", "finish", "解决", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
            {
                sender
                    .AddMessage($"很荣幸能够帮到您{Kaomojis.Happy}")
                    ;
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Welcome);
            }
            sender.AddMessage($"抱歉我不明白您的意思{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }

    public class GetStatementTypeForStatementStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetStatementTypeForStatement;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您想了解哪种语句呢？");
            var transDict = WorkingModule.DocumentCollection.StatementTypeToChinese;
            foreach (var type in Enum.GetValues<WorkingModule.DocumentCollection.StatementType>())
            {
                sender.AddPrompt(transDict[type]);
            }
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            var transDict = WorkingModule.DocumentCollection.ChineseToStatementType;
            string lowerMsg = msg.ToLowerInvariant();
            foreach (var pair in transDict)
            {
                if (lowerMsg.Contains(pair.Key))
                {
                    status.Put(Key.StatementType, pair.Value);
                    return Task.FromResult(StatusId.ShowLinksForStatement);
                }
            }
            context.Sender.AddMessage($"抱歉，我不能理解您说的话{Kaomojis.Sad}");
            return Task.FromResult(Id);
        }
    }

    public class ShowLinksForStatementStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.ShowLinksForStatement;
        private static readonly string Yes = "是";
        private static readonly string No = "否";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage("我为您找到了如下资料：")
                    .NewScope()
                    ;
                StatementType type = status.Get<StatementType>(Key.StatementType);
                context.Worker.GetDocumentCollection().SendStatementMessage(type, sender);
                status.IncreaseCount(Id);
            }
            sender
                .AddMessage("请问您的问题是否解决了呢？")
                .AddPrompt(Yes)
                .AddPrompt(No)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(No, "没", "未", "否", "无", "No", "不"))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetSimpleDescribe);
            }
            if (msg.ToLowerContainsAny(Yes, "谢谢", "OK", "finish", "解决", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
            {
                sender
                    .AddMessage($"很荣幸能够帮到您{Kaomojis.Happy}")
                    ;
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Welcome);
            }
            sender.AddMessage($"抱歉我不能理解您的意思{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }

    public class GetKeywordForKeywordsStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetKeywordForKeywords;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender.AddMessage("请问您想了解哪个关键字呢？");
            foreach (var keyword in Enum.GetValues<WorkingModule.DocumentCollection.Keyword>())
            {
                sender.AddPrompt(WorkingModule.DocumentCollection.KeywordToString(keyword));
            }
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            string lowerMsg = msg.ToLowerInvariant();
            foreach (var keyword in Enum.GetValues<WorkingModule.DocumentCollection.Keyword>())
            {
                if (lowerMsg.Contains(WorkingModule.DocumentCollection.KeywordToString(keyword)))
                {
                    status.Put(Key.Keyword, keyword);
                    return Task.FromResult(StatusId.ShowLinksForKeywords);
                }
            }
            context.Sender.AddMessage($"抱歉我不能理解您的意思{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }

    public class ShowLinksForKeywordsStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.ShowLinksForKeywords;
        private static readonly string Yes = "是";
        private static readonly string No = "否";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage("我为您找到了如下资料：")
                    .NewScope()
                    ;
                Keyword keyword = status.Get<Keyword>(Key.Keyword);
                context.Worker.GetDocumentCollection().SendKeywordMessage(keyword, sender);
                status.IncreaseCount(Id);
            }
            sender
                .AddMessage("请问您的问题是否解决了呢？")
                .AddPrompt(Yes)
                .AddPrompt(No)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(No, "没", "未", "否", "无", "No", "不"))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetSimpleDescribe);
            }
            if (msg.ToLowerContainsAny(Yes, "谢谢", "OK", "finish", "解决", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
            {
                sender
                    .AddMessage($"很荣幸能够帮到您{Kaomojis.Happy}")
                    ;
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Welcome);
            }
            sender.AddMessage($"抱歉我不能理解您的意思{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }
}
