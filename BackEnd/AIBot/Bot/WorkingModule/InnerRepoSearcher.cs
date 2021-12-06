using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Services;
using Buaa.AIBot.Repository.Models;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class InnerRepoSearcher
    {
        private readonly IQuestionService questionService;
        private readonly INLPService nlpService;
        private readonly double minScore;
        private readonly int continueCount;
        private readonly double secondLimitScoreTimes;

        public InnerRepoSearcher(IQuestionService questionService, INLPService nlpService, double minScore, int continueCount, double secondLimitScoreTimes)
        {
            this.questionService = questionService;
            this.nlpService = nlpService;
            this.minScore = minScore;
            this.continueCount = continueCount;
            this.secondLimitScoreTimes = secondLimitScoreTimes;
        }

        public class QuestionScoreInfo
        {
            public int Qid { get; set; }
            public double Score { get; set; }
            public IReadOnlyDictionary<TagCategory, IReadOnlyDictionary<int, string>> Tags { get; set; }
        }

        private async Task<IReadOnlyDictionary<TagCategory, IReadOnlyDictionary<int, string>>> BuildTagsAsync(IReadOnlyDictionary<int, TagCategory> tagIndex, int qid)
        {
            var questionTags = await questionService.QuestionRepository.SelectTagsForQuestionByIdAsync(qid);
            if (questionTags == null)
            {
                return null;
            }
            var ret = new Dictionary<TagCategory, Dictionary<int, string>>(
                Enum.GetValues<TagCategory>().Select(c => new KeyValuePair<TagCategory, Dictionary<int, string>>(c, new Dictionary<int, string>())));
            foreach (var kv in questionTags)
            {
                var category = tagIndex[kv.Value];
                ret[category][kv.Value] = kv.Key;
            }
            return new Dictionary<TagCategory, IReadOnlyDictionary<int, string>>(
                ret.Select(kv => new KeyValuePair<TagCategory, IReadOnlyDictionary<int, string>>(kv.Key, kv.Value)));
        }

        public async Task<IEnumerable<QuestionScoreInfo>> SearchAsync(string question, bool needNatrual)
        {
            var res = await nlpService.RetrievalAsync(question, 30, Enum.GetValues<NLPService.Languages>().ToList());
            var tagIndex = await questionService.GetTagCategoryIndexAsync();
            var ret = new List<QuestionScoreInfo>();
            if (needNatrual)
            {
                var first = res.FirstOrDefault();
                if (first == default)
                {
                    return ret;
                }
                if (first.Item1 < 0)
                {
                    return new QuestionScoreInfo[]{new QuestionScoreInfo() {Qid = first.Item1, Score = first.Item2}};
                }
            }
            res = res.Where(q => q.Item1 >= 0).ToList();
            foreach (var t in res)
            {
                if (t.Item2 < minScore)
                {
                    break;
                }
                var tags = await BuildTagsAsync(tagIndex, t.Item1);
                if (tags != null)
                {
                ret.Add(new QuestionScoreInfo()
                    {
                        Qid = t.Item1,
                        Score = t.Item2,
                        Tags = tags
                    });
                }
            }
            if (ret.Count <= continueCount)
            {
                var average = res.Select(q => q.Item2).Average();
                double secondLimitScore = average * secondLimitScoreTimes;
                if (secondLimitScore <= minScore)
                {
                    foreach (var t in res)
                    {
                        if (t.Item2 >= minScore)
                        {
                            continue;
                        }
                        if (t.Item2 < secondLimitScore)
                        {
                            break;
                        }
                        var tags = await BuildTagsAsync(tagIndex, t.Item1);
                        if (tags != null)
                        {
                        ret.Add(new QuestionScoreInfo()
                            {
                                Qid = t.Item1,
                                Score = t.Item2,
                                Tags = tags
                            });
                        }
                    }
                }
            }
            return ret;
        }
    }
}
