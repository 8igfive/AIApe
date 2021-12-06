using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Implement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Buaa.AIBot.Services;
using Microsoft.EntityFrameworkCore;

namespace Buaa.AIBot.TimedTask
{
    public class NLPSynchronizer
    {
        public static NLPSynchronizer DEFAULT { get; } = new NLPSynchronizer();
        public NLPService.Options NLPOptions { get; set; } = null;
        private readonly IReadOnlyDictionary<NLPService.Languages, IEnumerable<string>> languagesAlias;
        private Dictionary<int, NLPService.Languages> transformDictionary;
        public IReadOnlyDictionary<int, NLPService.Languages> TransformDictionart => transformDictionary;

        public NLPSynchronizer()
        {
            var tmp = new Dictionary<NLPService.Languages, IEnumerable<string>>()
            {
                [NLPService.Languages.C] = new string[] { "C", "C语言" },
                [NLPService.Languages.Java] = new string[] { "Java", "JVM", "Spring"},
                [NLPService.Languages.Python] = new string[] { "Python", "Python3", "Python2" },
                [NLPService.Languages.SQL] = new string[] { "SQL", "MySQL", "My SQL", "SQL Server", "SQLite"}
            };

            languagesAlias = new Dictionary<NLPService.Languages, IEnumerable<string>>(
                tmp.Select(kv => new KeyValuePair<NLPService.Languages, IEnumerable<string>>(kv.Key,
                    kv.Value.Select(alias => alias.ToLowerInvariant()))));
        }

        private ILogger<NLPSynchronizer> logger;


        private async Task<Dictionary<int, NLPService.Languages>> BuildTransformDictionaryAsync(IndependentDatabaseContext context, CancellationToken ct)
        {
            logger.LogInformation("Building TransformDictionary for NLPSynchronizer");
            var tags = await context.Tags.Where(t => t.Category == (int)TagCategory.Lang).Select(t => new { t.Name, t.TagId }).ToListAsync(ct);
            var ret = new Dictionary<int, NLPService.Languages>();
            foreach (var tag in tags)
            {
                var lowerName = tag.Name.ToLowerInvariant();
                foreach (var lang in languagesAlias)
                {
                    bool match = false;
                    foreach (var alias in lang.Value)
                    {
                        if (lowerName == alias)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (match)
                    {
                        ret[tag.TagId] = lang.Key;
                        break;
                    }
                }
            }
            return ret;
        }

        public async Task AddQuestion(int qid, INLPService nlpService, string title, IEnumerable<int> tags)
        {
            if (transformDictionary == null)
            {
                return;
            }
            var langs = new HashSet<NLPService.Languages>();
            foreach (int tid in tags)
            {
                if (transformDictionary.TryGetValue(tid, out var lang))
                {
                    langs.Add(lang);
                }
            }
            if (title == default)
            {
                return;
            }
            if (langs.Count == 0)
            {
                await nlpService.AddAsync(qid, title, NLPService.Languages.Other);
            }
            foreach (var lang in langs)
            {
                await nlpService.AddAsync(qid, title, lang);
            }
        }

        private async Task AddQuestionAsync(int qid, NLPService nlpService, DatabaseContext context, CancellationToken ct)
        {
            var tags = await context.QuestionTagRelations.Where(qt => qt.QuestionId == qid).Select(qt => qt.TagId).ToListAsync();
            var title = await context.Questions.Where(q => q.QuestionId == qid).Select(q => q.Title).FirstOrDefaultAsync();
            await AddQuestion(qid, nlpService, title, tags);
        }

        public async Task SynchronizeQuestionDataAsync(CancellationToken ct, TimedTaskManager.LoggerFactory loggerFactory)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            logger ??= loggerFactory.GetLogger<NLPSynchronizer>();
            if (NLPOptions == null)
            {
                logger.LogWarning("SynchronizeQuestionDataAsync cancelled because the options has not initialize yet.");
                return;
            }
            logger.LogInformation("SynchronizeQuestionDataAsync start");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            using var context = new IndependentDatabaseContext(loggerFactory.GetLogger<IndependentDatabaseContext>());
            transformDictionary ??= await BuildTransformDictionaryAsync(context, ct);
            var nlpService = new NLPService(
                loggerFactory.GetLogger<NLPService>(),
                new Utils.GlobalCancellationTokenSource()
                {
                    Source = cts
                },
                NLPOptions);

            DateTime start = DateTime.Now;
            try
            {
                int lastQid = int.MaxValue;
                int batchSize = 128;
                while (!ct.IsCancellationRequested)
                {
                    var questionList = await context
                        .Questions
                        .Where(q => q.QuestionId < lastQid)
                        .OrderByDescending(q => q.QuestionId)
                        .Select(q => q.QuestionId)
                        .Take(batchSize).ToListAsync(ct);
                    ct.ThrowIfCancellationRequested();
                    if (questionList.Count == 0)
                    {
                        break;
                    }
                    if (lastQid == int.MaxValue)
                    {
                        lastQid = questionList.First();
                    }
                    int nextQid = questionList[questionList.Count - 1];
                    logger.LogInformation("NLPSychronizer is checking qid {startQid}~{endQid}", lastQid, nextQid);
                    var remoteQidsTask = nlpService.CheckqidsAsync(Enumerable.Range(nextQid, lastQid - nextQid + 1));
                    var localQids = new HashSet<int>(questionList);
                    var deletedQids = Enumerable.Range(nextQid, lastQid - nextQid + 1).Where(qid => !localQids.Contains(qid)).ToList();
                    ct.ThrowIfCancellationRequested();
                    var notRemoteQids = new HashSet<int>(await remoteQidsTask);
                    ct.ThrowIfCancellationRequested();
                    // logger.LogInformation("local:{localQids}", localQids);
                    // logger.LogInformation("not in remote:{remoteQids}", notRemoteQids);
                    foreach (var qid in deletedQids.Where(q => !localQids.Contains(q)).Where(q => !notRemoteQids.Contains(q)))
                    {
                        try
                        {
                            await nlpService.DeleteAsync(qid);
                        }
                        catch (System.Exception e)
                        {
                            logger.LogError("NLPSychronizer calling nlpService.DeleteAsync with exception: {e}", e);
                        }
                    }
                    foreach (var qid in notRemoteQids.Where(q => localQids.Contains(q)))
                    {
                        try
                        {
                            await AddQuestionAsync(qid, nlpService, context, ct);
                        }
                        catch (System.Exception e)
                        {
                            logger.LogError("NLPSychronizer calling nlpService.AddQuestionAsync with exception: {e}", e);
                        }
                    }
                    lastQid = nextQid;
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("synchronizing question data cancelled");
            }
            finally
            {
                logger.LogInformation("synchronizing question data cost {sec} s", (DateTime.Now - start).TotalSeconds);
            }
        }
    }
}
