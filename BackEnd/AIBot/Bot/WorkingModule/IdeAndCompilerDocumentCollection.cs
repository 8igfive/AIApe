using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class IdeAndCompilerDocumentCollection
    {
        public static readonly string WindowsOS = ConstantStrings.OS.WindowsOS;
        public static readonly string LinuxOS = ConstantStrings.OS.LinuxOS;
        public static readonly string MacOS = ConstantStrings.OS.MacOS;

        public static readonly string DevCpp = ConstantStrings.IDE.DevCpp;
        public static readonly string VisualCpp = ConstantStrings.IDE.VisualCpp;
        public static readonly string VSCode = ConstantStrings.IDE.VSCode;
        public static readonly string VS = ConstantStrings.IDE.VS;

        public static readonly string Gcc = ConstantStrings.Compiler.Gcc;
        public static readonly string Clang = ConstantStrings.Compiler.Clang;
        public static readonly string Msvc = ConstantStrings.Compiler.Msvc;

        public void SendIDEDocumentMessages(string os, string ide, string compiler, IBotSender sender)
        {
            bool found = false;

            if (os == WindowsOS && ide == VS)
            {
                sender
                    .AddMessage("Visual Studio 文档：", newLine: false)
                    .AddUrl("https://docs.microsoft.com/zh-cn/visualstudio/windows/?view=vs-2019&preserve-view=true");
            }
            if (os == MacOS && ide == VS)
            {
                sender
                    .AddMessage("Visual Studio for Mac 文档：", newLine: false)
                    .AddUrl("https://docs.microsoft.com/zh-cn/visualstudio/mac/?view=vsmac-2019");
            }
            if (ide == VSCode)
            {
                sender
                    .AddMessage("Visual Studio Code 文档：", newLine: false)
                    .AddUrl("https://code.visualstudio.com/docs");
            }
            if (compiler == Gcc)
            {
                sender
                    .AddMessage("GCC 官方网站")
                    .AddUrl("https://gcc.gnu.org/");
            }
            if (compiler == Clang)
            {
                sender
                    .AddMessage("Clang 官方网站")
                    .AddUrl("https://clang.llvm.org/");
            }
            if (!found)
            {
                sender.AddMessage("抱歉，未能找到相关文档。");
            }
        }
    }
}
