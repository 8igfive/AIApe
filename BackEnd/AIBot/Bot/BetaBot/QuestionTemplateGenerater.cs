using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.BetaBot
{
    public class QuestionTemplateGenerater
    {
        public class QuestionTemplate
        {
            public string Title { get; set; }
            public string Remarks { get; set; }
            public IEnumerable<int> Tags { get; set; }
        }

        private IStatusContainerPool<StatusId> statusPool;

        public QuestionTemplateGenerater(IStatusContainerPool<StatusId> statusPool)
        {
            this.statusPool = statusPool;
        }

        public async Task<QuestionTemplate> GenerateAsync (int qid, bool force)
        {
            var status = await statusPool.GetStatusAsync(qid);
            if (status == null)
            {
                if (force)
                {
                    return new QuestionTemplate();
                }
                return null;
            }
            if (!force && status.Status != StatusId.CreateQuestion)
            {
                return null;
            }
            var ret = new QuestionTemplate();

            if (status.TryGet<string>(Constants.Key.ShortQuestion, out var title))
            {
                ret.Title = title;
            }

            if (status.TryGet<string>(Constants.Key.LongQuestion, out var remarks))
            {
                ret.Remarks = remarks;
            }

            if (status.TryGet<Dictionary<TagCategory, HashSet<int>>>(Constants.Key.SelectedTags, out var tags))
            {
                var list = new List<int>();
                foreach (var kv in tags)
                {
                    if (kv.Value != null)
                    {
                        list.AddRange(kv.Value);
                    }
                }
                ret.Tags = list;
            }

            return ret;
        }
    }
}
