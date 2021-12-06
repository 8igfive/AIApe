using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.AlphaBot.Status
{

    public class GetOSForUsingStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetOSForUsing;

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
            return Task.FromResult(StatusId.GetIDEForUsing);
        }
    }

    public class GetIDEForUsingStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetIDEForUsing;
        private static readonly string NoIDE = "未使用IDE";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您使用的IDE是什么呢？")
                .AddPrompt(Value.DevCpp)
                .AddPrompt(Value.VisualCpp)
                .AddPrompt(Value.VS)
                .AddPrompt(Value.VSCode)
                .AddPrompt(NoIDE)
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
            else if (msg.ToLowerContainsAny(NoIDE, "不", "无", "未", "NO", "null"))
            {
                status.Remove(Key.IDE);
                status.Remove(Key.IDE_detail);
            }
            else
            {
                context.Sender.AddMessage($"抱歉，我不认识您说的IDE{Kaomojis.Sad}").NewScope();
                return Task.FromResult(Id);
            }
            return Task.FromResult(StatusId.GetCompilerForUsing);
        }
    }

    public class GetCompilerForUsingStatus : IBotStatusBehaviour<StatusId>
    {
        public StatusId Id => StatusId.GetCompilerForUsing;
        private static readonly string UnknownCompiler = "不知道";

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;
            sender
                .AddMessage("请问您使用的编译器是什么呢？")
                .AddPrompt(Value.Gcc)
                .AddPrompt(Value.Clang)
                .AddPrompt(Value.Msvc)
                .AddPrompt(UnknownCompiler);
            return Task.CompletedTask;
        }

        public Task<StatusId> ExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            string msg = context.Receiver.UserMessage;
            if (msg.ToLowerContainsAny(Value.Gcc, "GNU"))
            {
                status.Put(Key.Compiler, Value.Gcc);
                status.Put(Key.Compiler_detail, msg);
            }
            else if (msg.ToLowerContainsAny(Value.Clang, "llvm"))
            {
                status.Put(Key.Compiler, Value.Clang);
                status.Put(Key.Compiler_detail, msg);
            }
            else if (msg.ToLowerContainsAny(Value.Msvc, "VC"))
            {
                status.Put(Key.Compiler, Value.Msvc);
                status.Put(Key.Compiler_detail, msg);
            }
            else if (msg.ToLowerContainsAny(UnknownCompiler, "不", "否", "无", "unknow", "NO"))
            {
                status.Remove(Key.Compiler);
                status.Remove(Key.Compiler_detail);
            }
            else
            {
                context.Sender.AddMessage($"抱歉，我不认识您说的编译器{Kaomojis.Sad}").NewScope();
                return Task.FromResult(Id);
            }
            return Task.FromResult(StatusId.ShowDocumentLinkForUsing);
        }
    }

    public class ShowDocumentLinkForUsingStatus : IBotStatusBehaviour<StatusId>
    {
        private static readonly string Finish = "解决了";
        private static readonly string NeedQuestion = "没解决";
        public StatusId Id => StatusId.ShowDocumentLinkForUsing;

        public Task EnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            if (status.GetCount(Id) == 0)
            {
                context.Sender.AddMessage("我为您找到了这些信息：").NewScope();
                string os = status.Get<string>(Key.OS);
                string ide;
                if (!status.TryGet(Key.IDE, out ide))
                {
                    ide = null;
                }
                string compiler;
                if (!status.TryGet(Key.Compiler, out compiler))
                {
                    compiler = null;
                }
                context.Worker.GetIdeAndCompilerDocumentCollection().SendIDEDocumentMessages(os, ide, compiler, context.Sender);
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
}
