using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{
    public class GetOSForInstallingStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetOSForInstalling;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您的操作系统是什么呢？")
                .AddPrompt(Value.WindowsOS).AddPrompt(Value.LinuxOS).AddPrompt(Value.MacOS);
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(Value.WindowsOS, "win"))
            {
                status.Put(Key.OS, Value.WindowsOS);
            }
            else if (msg.ToLowerContainsAny(Value.MacOS, "mac", "apple"))
            {
                status.Put(Key.OS, Value.MacOS);
            }
            else if (msg.ToLowerContainsAny(
                Value.LinuxOS, "Linux", 
                "ubuntu", "Redhat", "Debain", "Fedora", "openSUSE", "Mandriva", "Mint",
                "PCLinuxOS", "Cent OS", "CentOS", "Slackware", "Gentoo"))
            {
                status.Put(Key.OS, Value.LinuxOS);
            }
            else
            {
                context.Sender.AddMessage($"抱歉，我不认识您说的操作系统{Kaomojis.Sad}").NewScope();
                return Task.FromResult(Id);
            }
            status.Put(Key.OS_detail, msg);
            return Task.FromResult(StatusId.GetIDEForInstalling);
        }
    }

    public class GetIDEForInstallingStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetIDEForInstalling;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您想安装的IDE/编译器是什么呢？")
                .AddPrompt(Value.DevCpp)
                .AddPrompt(Value.VisualCpp)
                .AddPrompt(Value.VS)
                .AddPrompt(Value.VSCode)
                ;
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(Value.DevCpp, "dev"))
            {
                status.Put(Key.IDE, Value.DevCpp);
                status.Put(Key.IDE_detail, msg);
            }
            else if (msg.ToLowerContainsAny(Value.VisualCpp, "visualc", "visual c", "VC++", "VC"))
            {
                status.Put(Key.IDE, Value.VisualCpp);
                status.Put(Key.IDE_detail, msg);
            }
            else if (msg.ToLowerContainsAny(Value.VSCode, "code", "vs code", "vscode", "visualstudio code", "visualstudiocode", "visual studio code"))
            {
                status.Put(Key.IDE, Value.VSCode);
                status.Put(Key.IDE_detail, msg);
            }
            else if (msg.ToLowerContainsAny(Value.VS, "vs", "visualstudio"))
            {
                status.Put(Key.IDE, Value.VS);
                status.Put(Key.IDE_detail, msg);
            }
            // TODO add gcc and clang
            else
            {
                context.Sender.AddMessage($"抱歉，我不认识您说的IDE/编译器{Kaomojis.Sad}").NewScope();
                return Task.FromResult(Id);
            }
            return Task.FromResult(StatusId.ShowGovernmentLinkForInstalling);
        }
    }

    public class ShowGovernmentLinkForInstallingStatus : IBotStatusBehaviour<StatusId>
    {
        private static readonly string Finish = "解决了";
        private static readonly string NeedQuestion = "没解决";
        public StatusId Id => StatusId.ShowGovernmentLinkForInstalling;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            if (status.GetCount(Id) == 0)
            {
                context.Sender.AddMessage("我为您找到了这些信息：").NewScope();
                string os = status.Get<string>(Key.OS);
                string target;
                if (!status.TryGet(Key.IDE, out target))
                {
                    target = status.Get<string>(Key.Compiler);
                }
                context.Worker.GetGovernmentInstallingInfo().SendInstallingMessages(os, target, context.Sender);
                status.IncreaseCount(Id);
            }
            context.Sender
                .NewScope()
                .AddMessage("请问是否解决了您的问题呢？")
                .AddPrompt(Finish).AddPrompt(NeedQuestion);
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var sender = context.Sender;
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(NeedQuestion, "没", "未", "NO", "不") || msg.ToLowerInvariant() == "n")
            {
                status.ClearCount(Id);
                return Task.FromResult(StatusId.GetSimpleDescribe);
            }
            if (msg.ToLowerContainsAny(Finish, "谢谢", "OK",  "finish", "解决", "thanks", "thx", "yes") || msg.ToLowerInvariant() == "y")
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
}
