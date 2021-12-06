using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public static class ConstantStrings
    {
        public static readonly string SimpleDescribe = "SimpleDescribe";
        public static readonly string DetailDescribe = "DetailDescribe";

        public static class OS
        {
            public static readonly string WindowsOS = "Windows";
            public static readonly string LinuxOS = "Linux";
            public static readonly string MacOS = "macOS";
        }

        public static class IDE
        {
            public static readonly string DevCpp = "Dev C++";
            public static readonly string VisualCpp = "Visual C++";
            public static readonly string VSCode = "VS Code";
            public static readonly string VS = "Visual Studio";
        }

        public static class Compiler
        {
            public static readonly string Gcc = "gcc";
            public static readonly string Clang = "clang";
            public static readonly string Msvc = "msvc";
        }
    }
}
