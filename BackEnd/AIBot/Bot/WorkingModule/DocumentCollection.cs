using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;


namespace Buaa.AIBot.Bot.WorkingModule
{
    public class DocumentCollection
    {
        public static readonly string Key_StatementType = "StatementType";
        public static readonly string Key_Keyword = "Keyword";

        public void SendStandardLibraryMessage(IBotSender sender)
        {
            sender
                // .AddMessage("我为你找到了如下参考：")
                .AddMessage("菜鸟教程：", newLine: false).AddUrl(@"https://www.runoob.com/cprogramming/c-standard-library.html")
                .AddMessage("cppreference：", newLine: false).AddUrl(@"https://zh.cppreference.com/w/c/header")
                .NewScope()
                ;
        }

        public enum StatementType
        {
            Loop,
            Branch,
            Function,
        }

        public static Dictionary<string, StatementType> ChineseToStatementType = new Dictionary<string, StatementType>()
        {
            ["循环"] = StatementType.Loop,
            ["分支"] = StatementType.Branch,
            ["函数"] = StatementType.Function,
        };

        public static Dictionary<StatementType, string> StatementTypeToChinese = new Dictionary<StatementType, string>()
        {
            [StatementType.Loop] = "循环",
            [StatementType.Branch] = "分支",
            [StatementType.Function] = "函数",
        };

        public static Dictionary<StatementType, string> RunNoobForStatement = new Dictionary<StatementType, string>()
        {
            [StatementType.Loop] = @"https://www.runoob.com/cprogramming/c-loops.html",
            [StatementType.Branch] = @"https://www.runoob.com/cprogramming/c-decision.html",
            [StatementType.Function] = @"https://www.runoob.com/cprogramming/c-functions.html",
        };

        public void SendStatementMessage(StatementType type, IBotSender sender)
        {
            // sender.AddMessage("我为您找到了如下参考：");
            InnerSendStatementMessage(type, sender);
            sender.NewScope();
        }

        private void InnerSendStatementMessage(StatementType type, IBotSender sender)
        {
            sender
                .AddMessage("菜鸟教程：", newLine: false).AddUrl(RunNoobForStatement[type])
                ;
        }

        public enum Keyword
        {
            _auto,
            _break,
            _case,
            _char,
            _const,
            _continue,
            _default,
            _do,
            _double,
            _else,
            _enum,
            _extern,
            _float,
            _for,
            _goto,
            _if,
            _inline,
            _int,
            _long,
            _register,
            _restrict,
            _return,
            _short,
            _signed,
            _sizeof,
            _static,
            _struct,
            _switch,
            _typedef,
            _union,
            _unsigned,
            _void,
            _volatile,
            _while,
        }

        public static string KeywordToString(Keyword keyword)
        {
            return keyword.ToString().Substring(1);
        }

        public static bool TryStringToKeyword(string str, out Keyword keyword)
        {
            return Enum.TryParse<Keyword>('_' + str.ToLowerInvariant(), out keyword);
        }

        public void SendKeywordMessage(Keyword keyword, IBotSender sender)
        {
            // sender.AddMessage("我为您找到了如下参考：");
            InnerSendKeywordMessage(keyword, sender);
            sender.NewScope();
        }

        private void InnerSendKeywordMessage(Keyword keyword, IBotSender sender)
        {
            sender
                .AddMessage("cppreference：", newLine: false).AddUrl(@"https://zh.cppreference.com/w/c/keyword/" + KeywordToString(keyword))
                ;
        }
    }
}
