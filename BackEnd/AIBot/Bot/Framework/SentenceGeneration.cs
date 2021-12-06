using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework
{
    public static class SentenceGeneration
    {
        private readonly static Random random = new Random();

        public static string Choose(string str)
        {
            return str;
        }

        public static string Choose(params string[] strs)
        {
            return strs[random.Next(strs.Length)];
        }
    }
}
