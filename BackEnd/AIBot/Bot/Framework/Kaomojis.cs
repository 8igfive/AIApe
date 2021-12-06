using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework
{
    public static class Kaomojis
    {
        private static readonly Random rnd = new Random();
        private static string RandomChoose(List<string> list)
        {
            int index = rnd.Next(list.Count);
            return list[index];
        }

        private static readonly List<string> sads = new List<string>()
        {
            "ε(┬┬﹏┬┬)3",
            "o(╥﹏╥)o",
            "o(TヘTo)",
            "╥﹏╥...",
            "┭┮﹏┭┮",
            "(￣m￣）",
            "(o_ _)ﾉ",
            "┑(￣Д ￣)┍",
            "(。﹏。)"
        };

        public static string Sad => RandomChoose(sads);

        private static readonly List<string> happys = new List<string>()
        {
            "♪(´∇`*)",
            "(｡･∀･)ﾉﾞ",
            "(*^▽^*)",
            "φ(゜▽゜*)♪",
            "(/≧▽≦)/",
            "ㄟ(≧◇≦)ㄏ",
            "( ‵▽′)ψ"
        };

        public static string Happy => RandomChoose(happys);

        private static readonly List<string> frighteneds = new List<string>()
        {
            "o((⊙﹏⊙))o.",
            "ヽ(*。>Д<)o゜",
            "(⊙ˍ⊙)",
            "(°ー°〃)"
        };

        public static string Frightened => RandomChoose(frighteneds);

        private static readonly List<string> cutes = new List<string>()
        {
            "(>▽<)",
            "(≧∇≦)ﾉ",
            "ヾ(´∀`o)+",
            "=￣ω￣=",
            "o(〃'▽'〃)o",
            "(～o￣3￣)～",
            "ヾ(´∀`o)+",
            "(ˉ▽￣～)",
            "(*/ω＼*)"
        };

        public static string Cute => RandomChoose(cutes);

        private static readonly List<string> fightings = new List<string>()
        {
            "(ง •_•)ง"
        };

        public static string Fighting => RandomChoose(fightings);
    }
}
