using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot.WorkingModule;

namespace Buaa.AIBot.Bot
{
    public class EchoBot
    {
        private static readonly string MsgKey = "msg";
        private static readonly string RestartPrompt = "重新开始";

        public enum StatusEnum
        {
            Welcome, Echo
        }

        public static Dictionary<EchoBot.StatusEnum, IBotStatusBehaviour<EchoBot.StatusEnum>> GetStatusBehaviours()
        {
            var list = new List<IBotStatusBehaviour<StatusEnum>>() { new WelcomeStatus(), new EchoStatus() };
            var ret = new Dictionary<StatusEnum, IBotStatusBehaviour<StatusEnum>>();
            foreach (var status in list)
            {
                ret[status.Id] = status;
            }
            return ret;
        }

        private static StatusEnum Receive(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            var receiver = context.Receiver;
            string newMsg = receiver.UserMessage;
            if (newMsg == RestartPrompt)
            {
                status.Put(MsgKey, new List<string>());
                sender.AddMessage("重新开始！");
                sender.NewScope();
                return StatusEnum.Welcome;
            }
            else
            {
                List<string> received = status.Get<List<string>>(MsgKey);
                received.Add(receiver.UserMessage);
                status.Put(MsgKey, received);
                return StatusEnum.Echo;
            }
        }

        public class WelcomeStatus : IBotStatusBehaviour<StatusEnum>
        {
            public StatusEnum Id => StatusEnum.Welcome;

            public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
            {
                var sender = context.Sender;
                sender.AddMessage("您好，我叫小猿。");
                sender.AddMessage("我现还没有开发完成，我只会记住您说的话，然后将所有您说的话重复下来。");
                sender.NewScope();
                sender.AddMessage($"您可以随时输入“{RestartPrompt}”，来删除我的记录。");
                status.Put(MsgKey, new List<string>());
                return Task.CompletedTask;
            }

            public Task<StatusEnum> ExitAsync(IBotStatusContainer status, IBotExitContext context)
            {
                return Task.FromResult(Receive(status, context));
            }
        }

        public class EchoStatus : IBotStatusBehaviour<StatusEnum>
        {

            public StatusEnum Id => StatusEnum.Echo;

            public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
            {
                var sender = context.Sender;
                List<string> received = status.Get<List<string>>(MsgKey);
                if (received.Count == 0)
                {
                    sender.AddMessage("您现在还没有发任何消息");
                }
                else
                {
                    string msg = string.Join("]\n[", received);
                    sender.AddMessage($"您现在发过了{received.Count}条消息，它们是：\n[{msg}]");
                }
                sender.AddPrompt(RestartPrompt);
                return Task.FromResult(StatusEnum.Echo);
            }

            public Task<StatusEnum> ExitAsync(IBotStatusContainer status, IBotExitContext context)
            {
                return Task.FromResult(Receive(status, context));
            }
        }
    }
}
