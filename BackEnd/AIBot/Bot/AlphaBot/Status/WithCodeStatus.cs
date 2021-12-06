using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class GetCodeStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetCode;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender.AddMessage("请告诉我您的完整程序，要求可以通过编译且无错误、警告");
            return Task.CompletedTask;
        }

        public async Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var content = context.Receiver.UserMessage;
            var sender = context.Sender;
            string gccMessage = null;
            using (var gcc = await context.Worker.GetGccHandlerFactory().CreateHandlerAsync())
            {
                await gcc.CreatSourceFileAsync(content);
                gccMessage = await gcc.CompileAsync();
                gcc.CleanUp();
            }
            if (gccMessage.Length == 0)
            {
                sender.AddMessage($"已通过编译{Kaomojis.Happy}");
                status.Put(Key.SourceCode, content);
                return StatusId.AskIfHaveWrongCase;
            }
            sender
                .AddMessage($"编译仍旧有错误/警告信息哟{Kaomojis.Cute}:")
                .AddMessage(gccMessage)
                .AddMessage("请先按照提示修改完再来问我哟")
                .NewScope();
            return Id;
        }
    }

    public class AskIfHaveWrongCaseStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.AskIfHaveWrongCase;
        private static readonly string Yes = "是";
        private static readonly string No = "否";
        private static readonly string NoInput = "该程序不需要输入";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您是否知道程序出错时的输入是什么呢？")
                .AddPrompt(Yes)
                .AddPrompt(No)
                .AddPrompt(NoInput)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage; 
            if (msg.Contains(NoInput))
            {
                status.Remove(Key.WrongCaseInput);
                return Task.FromResult(StatusId.GetWrongCaseExpectOutput);
            }
            if (msg.ToLowerContainsAny(No, "NO", "没", "不", "否") || msg.ToLowerInvariant() == "n")
            {
                status.Remove(Key.WrongCaseInput);
                status.Remove(Key.WrongCaseOutput);
                return Task.FromResult(StatusId.TrySolveWithCode);
            }
            else if (msg.ToLowerContainsAny(Yes, "YES", "有", "知道") || msg.ToLowerInvariant() == "y")
            {
                return Task.FromResult(StatusId.GetWrongCaseInput);
            }
            context.Sender
                .AddMessage($"对不起，我不知道您在说什么{Kaomojis.Sad}")
                .AddMessage("如果您知道出错时的输入，我待会儿再问您。")
                .NewScope()
                ;
            return Task.FromResult(Id);
        }
    }

    public class GetWrongCaseInputStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetWrongCaseInput;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请输入出错时的输入：")
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string input = context.Receiver.UserMessage;
            status.Put(Key.WrongCaseInput, input);
            return Task.FromResult(StatusId.GetWrongCaseExpectOutput);
        }
    }

    public class GetWrongCaseExpectOutputStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetWrongCaseExpectOutput;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请输入出错时的期望输出（正确的输出）：")
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string output = context.Receiver.UserMessage;
            status.Put(Key.WrongCaseOutput, output);
            return Task.FromResult(StatusId.TrySolveWithCode);
        }
    }

    public class TrySolveWithCodeStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.TrySolveWithCode;
        public static readonly string NotSolveWithModify = "修改了源代码，但没有解决";
        public static readonly string NotSolveWithoutModify = "没有修改源代码，也没有解决";
        public static readonly string Solved = "已解决";

        public async Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage("小猿正在尝试帮您找出问题...")
                    .NewScope()
                    ;
                await context.Worker.GetSourceCodeAnalyzer().AnalyzeAsync(status, sender);
                status.IncreaseCount(Id);
            }

            sender
                .AddMessage("请问您解决问题了吗？")
                .AddPrompt(NotSolveWithModify)
                .AddPrompt(NotSolveWithoutModify)
                .AddPrompt(Solved)
                ;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            var sender = context.Sender;
            if (msg.ToLowerContainsAny(Solved, "谢谢", "OK", "finish", "已解决", "解决了", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
            {
                sender
                    .AddMessage($"很荣幸能够帮到您{Kaomojis.Happy}")
                    ;
                status.ClearCount(Id);
                return Task.FromResult(StatusId.Welcome);
            }
            if (msg.Contains(NotSolveWithModify))
            {
                sender
                    .AddMessage("请告诉我您修改后的源代码")
                    .NewScope()
                    ;
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetCode);
            }
            if (msg.Contains(NotSolveWithoutModify))
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetSimpleDescribe);
            }
            sender.AddMessage($"抱歉我不明白您在说什么{Kaomojis.Sad}").NewScope();
            return Task.FromResult(Id);
        }
    }
}
