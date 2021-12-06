using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.WorkingModule;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Repository.Models;
using static Buaa.AIBot.Bot.WorkingModule.InnerRepoSearcher;

namespace Buaa.AIBot.Bot.BetaBot.Status
{
    public static class Utils
    {

        public static Dictionary<TagCategory, HashSet<int>> GetSelectedTags(this IBotStatusContainer status)
        {
            Dictionary<TagCategory, HashSet<int>> ret;
            if (status.TryGet(Constants.Key.SelectedTags, out ret))
            {
                return ret;
            }
            ret = new Dictionary<TagCategory, HashSet<int>>(Enum.GetValues<TagCategory>().Select(c => new KeyValuePair<TagCategory, HashSet<int>>(c, null)));

            // TODO
            ret[TagCategory.Other] = new HashSet<int>();
            status.Put(Constants.Key.SelectedTags, ret);
            return ret;
        }

        public static string GetCheckingQuestion(this IBotStatusContainer status)
        {
            return status.Get<string>(Constants.Key.Checking) == Constants.Value.CheckingLong ?
                status.Get<string>(Constants.Key.LongQuestion) :
                status.Get<string>(Constants.Key.ShortQuestion);
        }

        public static IEnumerable<QuestionScoreInfo> GetQuestionMatches(this IBotStatusContainer status)
        {
            return status.Get<IEnumerable<QuestionScoreInfo>>(Constants.Key.QuestionMatches);
        }

        private class QuestionTagAdapter : AIBot.Utils.QuestionJudgement.IQuestionTagInfo
        {
            public QuestionTagAdapter(QuestionScoreInfo origin)
            {
                Qid = origin.Qid;
                Tags = new Dictionary<TagCategory, IEnumerable<int>>(origin.Tags.Select(kv => new KeyValuePair<TagCategory, IEnumerable<int>>(kv.Key, kv.Value.Keys)));
            }
            public int Qid { get; }
            public IReadOnlyDictionary<TagCategory, IEnumerable<int>> Tags { get; }
        }

        /// <summary>
        /// <see cref="Constants.Key.Checking"/> in <paramref name="status"/> should be set.
        /// if <see cref="Constants.Key.Checking"/> is set to <see cref="Constants.Value.CheckingLong"/>, <see cref="Constants.Key.LongQuestion"/> is required;
        /// the same as short.
        /// </summary>
        /// <param name="status"></param>
        /// <returns></returns>
        public static List<int> CalculateSelectedQuestions(
            this IBotStatusContainer status,
            Dictionary<TagCategory, HashSet<int>> selectedTags = null,
            IEnumerable<QuestionScoreInfo> questions = null)
        {
            selectedTags ??= status.GetSelectedTags();
            questions ??= status.GetQuestionMatches();
            var res = AIBot.Utils.QuestionJudgement.GetFilteredQuestions(
                questions.Select(q => new QuestionTagAdapter(q)).ToList(), 
                new Dictionary<TagCategory, IEnumerable<int>>(selectedTags.Select(kv => 
                    new KeyValuePair<TagCategory, IEnumerable<int>>(kv.Key, kv.Value??new HashSet<int>()))));
            return res;
        }

        public static List<int> CalculateSortedSelectedQuestions(
            this IBotStatusContainer status,
            Dictionary<TagCategory, HashSet<int>> selectedTags = null,
            IEnumerable<QuestionScoreInfo> questions = null)
        {
            selectedTags ??= status.GetSelectedTags();
            questions ??= status.GetQuestionMatches();
            var res = status.CalculateSelectedQuestions(selectedTags, questions);
            var set = new HashSet<int>(res);
            var query = from question in questions
                        where set.Contains(question.Qid)
                        orderby question.Score descending
                        select question.Qid;
            return query.ToList();
        }

        public static List<int> GetSortedSelectedQuestions(this IBotStatusContainer status)
        {
            if (status.TryGet<List<int>>(Constants.Key.Cached_SortedSelectedMatches, out var ret))
            {
                return ret;
            }
            ret = status.CalculateSortedSelectedQuestions();
            status.Put(Constants.Key.Cached_SortedSelectedMatches, ret);
            return ret;
        }

        public static Dictionary<int, double> CalculateSelectedQuestionsWithScore(
            this IBotStatusContainer status,
            Dictionary<TagCategory, HashSet<int>> selectedTags = null,
            IEnumerable<QuestionScoreInfo> questions = null)
        {
            selectedTags ??= status.GetSelectedTags();
            questions ??= status.GetQuestionMatches();
            var res = status.CalculateSelectedQuestions(selectedTags, questions);
            var set = new HashSet<int>(res);
            var query = from question in questions
                        where set.Contains(question.Qid)
                        select new KeyValuePair<int, double>(question.Qid, question.Score);
            return new Dictionary<int, double>(query);
        }

        /// <summary>
        /// key >=  0 ==> real tid;
        /// key == -1 ==> null;
        /// </summary>
        /// <param name="category"></param>
        /// <param name="questions"></param>
        /// <returns></returns>
        private static Dictionary<int, string> GetAllTagsForCategory(TagCategory category, IEnumerable<QuestionScoreInfo> questions)
        {
            var ret = new Dictionary<int, string>();
            foreach (var question in questions)
            {
                var questionTags = question.Tags[category];
                if (questionTags.Count == 0)
                {
                    ret.TryAdd(-1, "null");
                }
                foreach (var tag in questionTags)
                {
                    ret.TryAdd(tag.Key, tag.Value);
                }
            }
            return ret;
        }

        public static Dictionary<int, string> GetReducingTags(
            this IBotStatusContainer status,
            out List<TagCategory> categoriesCanBeEmpty,
            out TagCategory category,
            Dictionary<TagCategory, HashSet<int>> selectedTags = null,
            IEnumerable<QuestionScoreInfo> questions = null)
        {
            selectedTags ??= status.GetSelectedTags();
            categoriesCanBeEmpty = new List<TagCategory>();

            foreach (var kv in selectedTags)
            {
                if (kv.Value == null)
                {
                    questions ??= status.GetQuestionMatches();
                    var res = GetAllTagsForCategory(kv.Key, questions);
                    if (res.Count > 1)
                    {
                        category = kv.Key;
                        if (res.ContainsKey(-1))
                        {
                            res.Remove(-1);
                        }
                        return res;
                    }
                    else
                    {
                        categoriesCanBeEmpty.Add(kv.Key);
                    }
                }
            }
            category = default;
            return null;
        }

        public static StatusId DecideFeedbackOrWelcome(this IBotStatusContainer status)
        {
            // TODO
            return StatusId.GetFeedBack;
        }
    }
}
