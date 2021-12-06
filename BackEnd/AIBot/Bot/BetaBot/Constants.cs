using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.BetaBot
{
    public static class Constants
    {
        public static IReadOnlyList<string> ResetKeywords = new List<string>()
        {
            "下个问题", "重新开始"
        };

        public static class Key
        {
            // store the question that user input for title.
            public static readonly string ShortQuestion = "ShortQuestion";

            // store the question that user input for remarks.
            public static readonly string LongQuestion = "LongQuestion";

            // store bot is checking long or short
            public static readonly string Checking = "Checking";

            // store the Questions in db that matches the user's input.
            public static readonly string QuestionMatches = "QuestionMatches";

            // store the sorted-selected-question-matches, to make it running faster.
            public static readonly string Cached_SortedSelectedMatches = "Cached_SSM";

            // store the tags that can be choose to reduce the matching questions.
            public static readonly string Cached_ReducingTags = "Cached_ReducingTags";

            // marks the tag category that can be reduce.
            public static readonly string Cached_CheckingTagCategory = "Cached_CTC";

            // store the tags that user has selected.
            public static readonly string SelectedTags = "SelectedTags";

            // store the status to make bot into Reset-status, in order to recover.
            public static readonly string ResetStatusId = "ResetStatusId";
        }

        public static class Value
        {
            public static readonly string CheckingShort = "CheckingShort";
            public static readonly string CheckingLong = "CheckingLong";
        }
    }
}
