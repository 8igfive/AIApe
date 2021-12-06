using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class GovernmentInstallingInfo
    {
        public static readonly string WindowsOS = ConstantStrings.OS.WindowsOS ;
        public static readonly string LinuxOS   = ConstantStrings.OS.LinuxOS;
        public static readonly string MacOS = ConstantStrings.OS.MacOS;

        public static readonly string DevCpp = ConstantStrings.IDE.DevCpp;
        public static readonly string VisualCpp = ConstantStrings.IDE.VisualCpp;
        public static readonly string VSCode = ConstantStrings.IDE.VSCode;
        public static readonly string VS = ConstantStrings.IDE.VS;

        public void SendInstallingMessages(string os, string target, IBotSender sender)
        {
            if (target == DevCpp)
            {
                if (os == WindowsOS)
                {
                    WinDev(sender);
                    return;
                }
                else if (os == LinuxOS)
                {
                    LinuxDev(sender);
                    return;
                }
                else if (os == MacOS)
                {
                    MacDev(sender);
                    return;
                }
                throw new ArgumentException($"unknown os={os}");
            }
            else if (target == VisualCpp)
            {
                if (os == WindowsOS)
                {
                    WinVC(sender);
                    return;
                }
                else if (os == LinuxOS)
                {
                    LinuxVC(sender);
                    return;
                }
                else if (os == MacOS)
                {
                    MacVC(sender);
                    return;
                }
                throw new ArgumentException($"unknown os={os}");
            }
            else if (target == VSCode)
            {
                sender
                    .AddMessage("请先确认您是在官网下载的安装包：")
                    .AddUrl(@"https://code.visualstudio.com/")
                    .AddMessage("并参考官方文档进行安装：")
                    .AddUrl(@"https://code.visualstudio.com/docs/setup/setup-overview")
                    .AddMessage("最后参考官方文档配置环境：")
                    .AddUrl(@"https://code.visualstudio.com/docs/cpp/introvideos-cpp")
                    .NewScope()
                    ;
                return;
            }
            else if (target == VS)
            {
                if (os == LinuxOS)
                {
                    sender.AddMessage("抱歉，目前 Visual Studio 暂时没有Linux版本").NewScope();
                    return;
                }
                sender
                    .AddMessage("请先确认您是在官网下载的安装包：")
                    .AddUrl(@"https://visualstudio.microsoft.com/zh-hans/downloads/")
                    .AddMessage("并参考官方文档进行安装：")
                    .AddUrl(@"https://docs.microsoft.com/zh-cn/visualstudio/?view=vs-2019")
                    .NewScope()
                    ;
                return;
            }
            throw new ArgumentException($"unknown target={target}");
        }

        private void WinDev(IBotSender sender)
        {
            sender
                .AddMessage("现在 Dev C++ 官方版本已经停止维护，但是仍旧有一些社区维护的版本，参考以下知乎问题：")
                .AddUrl(@"https://www.zhihu.com/question/348305462")
                .AddMessage("其中提到的“官方”维护的开源版本：")
                .AddUrl(@"https://github.com/Embarcadero/Dev-Cpp#embarcadero-dev-c")
                .AddMessage("其中提到的斑竹软件维护的版本：")
                .AddUrl(@"https://devcpp.gitee.io/")
                .AddMessage("其中提到的小熊猫维护的版本：")
                .AddUrl(@"https://royqh.net/devcpp/")
                .AddMessage($"小猿这里推荐后面两个由中国教师维护的中文版本哟{Kaomojis.Cute}")
                .NewScope()
                ;
        }

        private void LinuxDev(IBotSender sender)
        {
            sender
                .AddMessage("似乎并没有原生支持Linux的 Dev C++，但是可以通过wine进行安装，参考这篇博客：")
                .AddUrl(@"https://blog.csdn.net/sherpahu/article/details/103936422")
                .NewScope()
                ;
        }

        private void MacDev(IBotSender sender)
        {
            sender
                .AddMessage("目前似乎 Dev C++ 并不支持Mac")
                .NewScope()
                ;
        }

        private void WinVC(IBotSender sender)
        {
            sender
                .AddMessage("Visual C++ 早在2005年就停止维护了，并且微软官方也停止了官方下载渠道以及安装支持，建议改为其它IDE。")
                .AddMessage("如果仍旧要使用 VC++ 6.0，可以参考C语言中文网的下载链接：")
                .AddUrl(@"http://c.biancheng.net/view/463.html")
                .NewScope()
                ;
        }

        private void LinuxVC(IBotSender sender)
        {
            sender
                .AddMessage("目前似乎 Visual C++ 并不支持Linux")
                .NewScope()
                ;
        }

        private void MacVC(IBotSender sender)
        {
            sender
                .AddMessage("目前似乎 Visual C++ 并不支持Mac")
                .NewScope()
                ;
        }
    }
}
