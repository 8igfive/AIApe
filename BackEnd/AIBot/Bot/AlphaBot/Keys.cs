using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.AlphaBot
{
    public static class Key
    {
        public static readonly string SimpleDescribe = WorkingModule.QuestionBuilder.SimpleDescribe;
        public static readonly string DetailDescribe = WorkingModule.QuestionBuilder.DetailDescribe;

        public static readonly string OS = WorkingModule.QuestionBuilder.OS;
        public static readonly string OS_detail = WorkingModule.QuestionBuilder.OS_detail;

        public static readonly string IDE = WorkingModule.QuestionBuilder.IDE;
        public static readonly string IDE_detail = WorkingModule.QuestionBuilder.IDE_detail;

        public static readonly string Compiler = WorkingModule.QuestionBuilder.Compiler;
        public static readonly string Compiler_detail = WorkingModule.QuestionBuilder.Compiler_detail;

        public static readonly string SourceCode = "SourceCode";
        public static readonly string WrongCaseInput = "WrongCaseInput";
        public static readonly string WrongCaseOutput = "WrongCaseOutput";

        public static readonly string StatementType = WorkingModule.DocumentCollection.Key_StatementType;
        public static readonly string Keyword = WorkingModule.DocumentCollection.Key_Keyword;
    }

    public static class Value
    {
        public static readonly string WindowsOS = WorkingModule.ConstantStrings.OS.WindowsOS;
        public static readonly string LinuxOS = WorkingModule.ConstantStrings.OS.LinuxOS;
        public static readonly string MacOS = WorkingModule.ConstantStrings.OS.MacOS;

        public static readonly string DevCpp = WorkingModule.ConstantStrings.IDE.DevCpp;
        public static readonly string VisualCpp = WorkingModule.ConstantStrings.IDE.VisualCpp;
        public static readonly string VSCode = WorkingModule.ConstantStrings.IDE.VSCode;
        public static readonly string VS = WorkingModule.ConstantStrings.IDE.VS;

        public static readonly string Gcc = WorkingModule.ConstantStrings.Compiler.Gcc;
        public static readonly string Clang = WorkingModule.ConstantStrings.Compiler.Clang;
        public static readonly string Msvc = WorkingModule.ConstantStrings.Compiler.Msvc;

        public static bool ContainsAny(this string msg, params string[] alias)
        {
            foreach (var alia in alias)
            {
                if (msg.Contains(alia))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool ToLowerContainsAny(this string msg, params string[] alias)
        {
            string target = msg.ToLowerInvariant();
            foreach (var alia in alias)
            {
                if (target.Contains(alia.ToLowerInvariant()))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
