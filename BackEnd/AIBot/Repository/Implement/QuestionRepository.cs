using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Repository.Implement
{
    /// <summary>
    /// Implement of <see cref="IQuestionRepository"/>.
    /// </summary>
    /// <remarks><seealso cref="IQuestionRepository"/></remarks>
    public class QuestionRepository : RepositoryBase , IQuestionRepository
    {
        private ILogger<QuestionRepository> logger;
        private ITagRepostory tagRepostory;

        public QuestionRepository(DatabaseContext context, ICachePool<int> cachePool, ITagRepostory tagRepostory, GlobalCancellationTokenSource globalCancellationTokenSource)
            : base(context, cachePool, globalCancellationTokenSource.Token)
        {
            this.tagRepostory = tagRepostory;
        }

        public QuestionRepository(DatabaseContext context, ICachePool<int> cachePool, ITagRepostory tagRepostory, GlobalCancellationTokenSource globalCancellationTokenSource, ILogger<QuestionRepository> logger)
            : base(context, cachePool, globalCancellationTokenSource.Token) 
        {
            this.logger = logger;
            this.tagRepostory = tagRepostory;
        }

        public async Task<IEnumerable<int>> SelectAnswersForQuestionByIdAsync(int questionId)
        {
            var query = await Context
                .Answers
                .Where(a => a.QuestionId == questionId)
                .Select(a => a.AnswerId)
                .ToListAsync(CancellationToken)
                ;
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var question = await Context.Questions.FindAsync(questionId);
            if (question == null)
            {
                return null;
            }
            return query;
        }

        public async Task<QuestionInfo> SelectQuestionByIdAsync(int questionId)
        {
            var question = await Context
                .Questions
                .Select(q => new QuestionInfo()
                {
                    QuestionId = q.QuestionId,
                    CreaterId = q.UserId,
                    Title = q.Title,
                    Remarks = q.Remarks,
                    CreateTime = q.CreateTime,
                    ModifyTime = q.ModifyTime
                })
                .Where(q => q.QuestionId == questionId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (question != null)
            {
                var hot = await Context
                    .QuestionHotDatas
                    .Where(qh => qh.QuestionId == questionId)
                    .SingleOrDefaultAsync();
                if (hot != null)
                {
                    question.HotValue = hot.HotValue;
                    question.HotFreshTime = hot.ModifyTime;
                }
                else
                {
                    if (logger != null)
                    {
                        logger.LogWarning("question is not null, but hot info is null");
                    }
                }
            }
            return question;
        }

        // TODO can make it faster?
        private class TagMatcher
        {
            public IReadOnlySet<int> Requires { get; }

            public TagMatcher(IEnumerable<int> tags)
            {
                if (tags != null)
                {
                    Requires = new HashSet<int>(tags);
                }
                else
                {
                    Requires = new HashSet<int>();
                }
            }

            public bool Match(IEnumerable<int> actualTags)
            {
                switch (Requires.Count)
                {
                    case 0:
                        return true;
                    case 1:
                        int target = Requires.First();
                        foreach (var tid in actualTags)
                        {
                            if (tid == target)
                            {
                                return true;
                            }
                        }
                        return false;
                    default:
                        break;
                }
                int remain = Requires.Count;
                foreach (var tid in actualTags)
                {
                    if (Requires.Contains(tid))
                    {
                        remain--;
                        if (remain == 0)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        //public async Task<IEnumerable<int>> SelectQuestionsByTagsAsync(IEnumerable<int> tags)
        //{
        //    var matcher = new TagMatcher(tags);
        //    var list = tags.ToList();
        //    if (list.Count == 0)
        //    {
        //        return await Context.Questions.Select(q => q.QuestionId).ToListAsync(CancellationToken);
        //    }
        //    // TODO can make it faster?
        //    string create_set = "drop table if exists tids;\ncreate temporary table tids (tid int not null, primary key(tid));\n";
        //    string values = string.Join("),(", list);
        //    string insert_set = $"insert into tids values ({values});\n";
        //    string select =
        //        "select *\n" +
        //        "from Questions\n" +
        //        "where not exists\n" +
        //        "(\n  " +
        //            "select tid from tids\n  " +
        //            "where not exists\n  " +
        //            "(\n    " +
        //                "select *\n    " +
        //                "from QuestionTagRelations as qt\n    " +
        //                "where qt.QuestionId=Questions.QuestionId and qt.TagId=tids.tid\n  " +
        //            ")\n" +
        //        ");";
        //    string sql = create_set + insert_set + select;
        //    var query = Context
        //        .Questions
        //        .FromSqlRaw(sql)
        //        .AsEnumerable()
        //        .Select(q => q.QuestionId);

        //    //var query = Context
        //    //    .QuestionTagRelations
        //    //    .GroupBy(qt => qt.QuestionId)
        //    //    .Where(q => matcher.Match(q.Select(qt => qt.TagId)))
        //    //    .Select(q => q.First().QuestionId);
        //    return query.ToList();
        //}

        private string SqlCommandForExistTid(int tid)
        {
            return $"exists (select `qt`.`TagId` from `QuestionTagRelations` as `qt` " +
                $"where `qt`.QuestionId=Questions.QuestionId and `qt`.TagId={tid})";
        }

        public async Task<IEnumerable<int>> SelectQuestionsByTagsAsync(IEnumerable<int> tags, int maxQid=int.MaxValue, int num = int.MaxValue)
        {
            var matcher = new TagMatcher(tags);
            var list = tags.ToList();
            if (list.Count == 0)
            {
                return await Context
                    .Questions
                    .Where(q => q.QuestionId < maxQid)
                    .OrderByDescending(q => q.QuestionId)
                    .Select(q => q.QuestionId)
                    .Take(num)
                    .ToListAsync(CancellationToken);
            }
            // TODO can make it faster?


            var tagIndex = await tagRepostory.SelectTagIndexAsync();
            var classifiedTags = new Dictionary<TagCategory, List<int>>(
                Enum.GetValues<TagCategory>().Select(c => new KeyValuePair<TagCategory, List<int>>(c, new List<int>())));
            foreach (var tid in tags)
            {
                if (tagIndex.TryGetValue(tid, out var category))
                {
                    classifiedTags[category].Add(tid);
                }
            }

            string filter = string.Join("\nand\n", classifiedTags.Values
                .Where(categoryList => categoryList.Count > 0)
                .Select(categoryList => string.Join("\nor\n", categoryList
                    .Select(tid => SqlCommandForExistTid(tid)))));

            string sql =
                "select * from `Questions`\n" +
                "where\n" +
                filter
                ;

            var query = await Context
                .Questions
                .FromSqlRaw(sql)
                .Where(q => q.QuestionId < maxQid)
                .OrderByDescending(q => q.QuestionId)
                .Select(q => q.QuestionId)
                .Take(num)
                .ToListAsync();
            return query;
        }

        public async Task<IEnumerable<KeyValuePair<string, int>>> SelectTagsForQuestionByIdAsync(int questionId)
        {
            var query = Context
                .QuestionTagRelations
                .Where(qt => qt.QuestionId == questionId)
                .Select(qt => new KeyValuePair<string, int>(qt.Tag.Name, qt.TagId));
            var list = await query.ToListAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (list.Count != 0)
            {
                return list;
            }
            var question = await Context.Questions.FindAsync(questionId);
            if (question == null)
            {
                return null;
            }
            return list;
        }

        private async Task CheckInsertAsync(QuestionWithListTag question, TagMatcher matcher)
        {
            var user = await Context
                .Users
                .Where(u => u.UserId == question.CreaterId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new UserNotExistException((int)question.CreaterId);
            }
            await CheckTags(matcher);
        }

        private async Task CheckTags(TagMatcher matcher)
        {
            if (!matcher.Match(Context.Tags.Select(t => t.TagId)))
            {
                var set = new HashSet<int>(matcher.Requires);
                var tagsInDatabase = await Context
                    .Tags
                    .Select(t => t.TagId)
                    .ToListAsync(CancellationToken);
                CancellationToken.ThrowIfCancellationRequested();
                foreach (int t in tagsInDatabase)
                {
                    set.Remove(t);
                }
                int tid = set.FirstOrDefault();
                throw new TagNotExistException(tid);
            }
        }

        public async Task<int> InsertQuestionAsync(QuestionWithListTag question)
        {
            if (question.CreaterId == null)
            {
                throw new ArgumentNullException(nameof(question.CreaterId));
            }
            if (question.Title.Length > Constants.QuestionTitleMaxLength)
            {
                throw new QuestionTitleTooLongException(question.Title.Length, Constants.QuestionTitleMaxLength);
            }
            var matcher = new TagMatcher(question.Tags);
            await CheckInsertAsync(question, matcher);
            var target = new QuestionData()
            {
                UserId = question.CreaterId,
                Title = question.Title,
                Remarks = question.Remarks
            };
            var hotData = new QuestionHotData()
            {
                Question = target,
                HotValue = 0
            };
            Context.Questions.Add(target);
            Context.QuestionHotDatas.Add(hotData);
            bool success = false;
            while (!success)
            {
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                await CheckInsertAsync(question, matcher);
            }
            int qid = target.QuestionId;
            Context.QuestionTagRelations.AddRange(
                matcher.Requires.Select(t => new QuestionTagRelation()
                {
                    QuestionId = qid,
                    TagId = t
                }));
            success = false;
            try
            {
                while (!success)
                {
                    success = await TrySaveChangesAgainAndAgainAsync();
                    if (success)
                    {
                        break;
                    }
                    await CheckTags(matcher);
                }
            }
            catch (Exception)
            {
                Context.Remove(target);
                await SaveChangesAgainAndAgainAsync();
            }
            return qid;
        }

        private async Task CheckUpdateAsync(QuestionData target, QuestionWithListTag question, TagMatcher matcher)
        {
            if ((await Context.Questions.FindAsync(question.QuestionId)) == null)
            {
                throw new QuestionNotExistException(question.QuestionId);
            }
            if (question.Tags != null)
            {
                await CheckTags(matcher);
            }
        }

        private class QuestionTagRelationPool
        {
            public Dictionary<int, QuestionTagRelation> Dict { get; } = new Dictionary<int, QuestionTagRelation>();
            private int qid;

            public QuestionTagRelationPool(int qid)
            {
                this.qid = qid;
            }

            public QuestionTagRelation Get(int tid)
            {
                if (Dict.TryGetValue(tid, out QuestionTagRelation ret))
                {
                    return ret;
                }
                ret = new QuestionTagRelation() { QuestionId = qid, TagId = tid };
                Dict[tid] = ret;
                return ret;
            }

            public void AddRange(IEnumerable<QuestionTagRelation> qts)
            {
                foreach (var qt in qts)
                {
                    Dict.TryAdd(qt.TagId, qt);
                }
            }
        }

        public async Task UpdateQuestionAsync(QuestionWithListTag question)
        {
            var target = await Context.Questions.FindAsync(question.QuestionId);
            if (target == null)
            {
                throw new QuestionNotExistException(question.QuestionId);
            }
            bool success = true;
            var matcher = new TagMatcher(question.Tags);
            if (question.Title != null)
            {
                success = false;
                if (question.Title.Length > Constants.QuestionTitleMaxLength)
                {
                    throw new QuestionTitleTooLongException(question.Title.Length, Constants.QuestionTitleMaxLength);
                }
                target.Title = question.Title;
            }
            if (question.Remarks != null)
            {
                success = false;
                target.Remarks = question.Remarks;
            }
            HashSet<int> tidsAdded = null;
            HashSet<int> tidsRemoved = null;
            QuestionTagRelationPool pool = null;
            if (question.Tags != null)
            {
                success = false;
                tidsAdded = new HashSet<int>();
                tidsRemoved = new HashSet<int>();
                pool = new QuestionTagRelationPool(question.QuestionId);
            }
            await CheckUpdateAsync(target, question, matcher);
            while (!success)
            {
                if (question.Tags != null)
                {
                    var tagsInDb = await Context.QuestionTagRelations
                        .Where(qt => qt.QuestionId == question.QuestionId)
                        .ToListAsync(CancellationToken);
                    CancellationToken.ThrowIfCancellationRequested();
                    pool.AddRange(tagsInDb);
                    var inside = new HashSet<int>(tagsInDb.Select(qt => qt.TagId));
                    foreach (var tid in tidsAdded)
                    {
                        inside.Add(tid);
                    }
                    foreach (var tid in tidsRemoved)
                    {
                        inside.Remove(tid);
                    }
                    var toAdd = matcher.Requires
                        .Where(t => !inside.Contains(t))
                        .ToList();
                    var toRemove = inside
                        .Where(t => !matcher.Requires.Contains(t))
                        .ToList();
                    Context.QuestionTagRelations.AddRange(toAdd
                        .Select(t => pool.Get(t)));
                    Context.QuestionTagRelations.RemoveRange(toRemove
                        .Select(t => pool.Get(t)));
                    foreach (var tid in toAdd)
                    {
                        tidsAdded.Add(tid);
                        tidsRemoved.Remove(tid);
                    }
                    foreach (var tid in toRemove)
                    {
                        tidsRemoved.Add(tid);
                        tidsAdded.Remove(tid);
                    }
                }
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                await CheckUpdateAsync(target, question, matcher);
            }
        }

        public async Task DeleteQuestionByIdAsync(int questionId)
        {
            var target = await Context.Questions.FindAsync(questionId);
            if (target != null)
            {
                Context.Remove(target);
                await SaveChangesAgainAndAgainAsync();
            }
        }

        public async Task<QuestionHotInfo> SelectHotInfoByIdAsync(int questionId)
        {
            return await Context.QuestionHotDatas
                .Where(qh => qh.QuestionId == questionId)
                .Select(qh => new QuestionHotInfo()
                {
                    HotValue = qh.HotValue,
                    ModifyTime = qh.ModifyTime
                })
                .SingleOrDefaultAsync(CancellationToken);
        }

        public async Task UpdateHotInfoAsync(int questionId, int hotValue)
        {
            bool success = false;
            while (!success)
            {
                var target = await Context.QuestionHotDatas.Where(q => q.QuestionId == questionId).SingleOrDefaultAsync(CancellationToken);
                if (target == null)
                {
                    throw new QuestionNotExistException(questionId);
                }
                target.HotValue = hotValue;
                success = await TrySaveChangesAgainAndAgainAsync();
            }
        }

        public async Task<IEnumerable<int>> SelectQuestionsHotValueAsync(int count)
        {
            return await Context.QuestionHotDatas
                .OrderByDescending(qh => qh.HotValue)
                .Select(qh => qh.QuestionId)
                .Take(count)
                .ToListAsync();
        }
    }
}
