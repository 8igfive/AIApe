using Buaa.AIBot.Bot.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.BetaBot.Status
{
    public abstract class BetaBotStatusBehaviour : IBotStatusBehaviour<StatusId>
    {

        public abstract StatusId Id { get; }

        protected abstract Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context);

        protected abstract Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context);

        protected void GotoReset(IBotStatusContainer status, IBotExitContext context, string reason)
        {
            context.Sender.AddMessage(reason);
            status.Put(Constants.Key.ResetStatusId, Id);
            throw new GotoResetException();
        }

        private class GotoResetException : Exception { }

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            return ProduceEnterAsync(status, context);
        }

        public async Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            try
            {
                var msg = context.Receiver.UserMessage;
                foreach (var keywords in Constants.ResetKeywords)
                {
                    if (msg.Length > keywords.Length + 2)
                    {
                        continue;
                    }
                    if (msg.Length < keywords.Length)
                    {
                        continue;
                    }
                    if (msg.Contains(keywords))
                    {
                        GotoReset(status, context, $"检测到关键词{keywords}");
                    }
                }
                return await ProduceExitAsync(status, context);
            }
            catch (GotoResetException)
            {
                return StatusId.Reset;
            }
        }
    }

    public class ResetStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.Reset;

        public static readonly string Restart = "是";
        public static readonly string Continue = "否";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            context.Sender
                .AddMessage($"请问您是否是希望结束当前问题，并开始新的问题？")
                .AddPrompt(Restart)
                .AddPrompt(Continue)
                ;
            return Task.CompletedTask;
        }

        public async Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            if (msg == Restart)
            {
                status.Clear();
                return StatusId.Welcome; 
            }
            if (msg == Continue)
            {
                return status.Get<StatusId>(Constants.Key.ResetStatusId);
            }
            var res = await context.Worker.Get<Services.NLPService>().SelectAsync(msg, Restart, Continue);
            if (res == Restart)
            {
                status.Clear();
                return StatusId.Welcome;
            }
            return status.Get<StatusId>(Constants.Key.ResetStatusId);
        }
    }
}
