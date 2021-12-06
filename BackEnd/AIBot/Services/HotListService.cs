using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Microsoft.Extensions.Logging;
using Buaa.AIBot.Repository.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Buaa.AIBot.Repository.Implement;

namespace Buaa.AIBot.Services
{
    public interface IHotListService
    {
        Task<IEnumerable<int>> GetHotListAsync();
        Task FreshHotListAsync();
    }

    public class HotListService : IHotListService
    {
        private class FreshingInfo
        {
            public Task task = null;
            public CancellationTokenSource cts = null;
        }

        //private static FreshingInfo freshingValue = null;
        private static IEnumerable<int> hotList;
        private static DateTime listTime = DateTime.Now;
        //private static DateTime valueTime = DateTime.MinValue;
        //private static readonly SemaphoreSlim freshValueLock = new SemaphoreSlim(1);
        private static readonly SemaphoreSlim freshListLock = new SemaphoreSlim(1);

        private readonly IQuestionRepository questionRepository;
        private readonly ILogger<HotListService> logger;
        //private readonly TimeSpan hotValueFreshInterval;
        private readonly TimeSpan hotListFreshInterval;
        // private readonly IServiceProvider serviceProvider;
        private readonly int length;

        public HotListService(IQuestionRepository questionRepository, ILogger<HotListService> logger, TimeSpan hotListFreshInterval, int length)
        {
            this.questionRepository = questionRepository;
            this.logger = logger;
            this.hotListFreshInterval = hotListFreshInterval;
            this.length = length;
        }

        private bool NeedFreshHotList()
        {
            return hotList == null || DateTime.Now - listTime >= hotListFreshInterval;
        }

        //private bool NeedFreshHotValue()
        //{
        //    return DateTime.Now - valueTime >= hotValueFreshInterval;
        //}

        //private void startFreshValue()
        //{
        //    var cts = new CancellationTokenSource(hotValueFreshInterval);
        //    logger.LogInformation("start hot-value freshing with interval={interval}", hotValueFreshInterval);
        //    var info = new FreshingInfo()
        //    {
        //        cts = cts,
        //        task = FreshHotValueAsync(cts.Token)
        //    };
        //    var old = Interlocked.Exchange(ref freshingValue, info);
        //    if (old != null && !old.task.IsCompleted)
        //    {
        //        old.cts.Cancel();
        //    }
        //}

        public async Task FreshHotListAsync()
        {
            hotList = await questionRepository.SelectQuestionsHotValueAsync(length);
        }

        public async Task<IEnumerable<int>> GetHotListAsync()
        {
            if (NeedFreshHotList())
            {
                await freshListLock.WaitAsync();
                try
                {
                    if (NeedFreshHotList())
                    {
                        logger.LogInformation("freshing hotlist");
                        listTime = DateTime.Now;
                        await FreshHotListAsync();
                    }
                }
                finally
                {
                    freshListLock.Release();
                }
            }
            //if (NeedFreshHotValue())
            //{
            //    await freshValueLock.WaitAsync();
            //    try
            //    {
            //        if (NeedFreshHotValue())
            //        {
            //            logger.LogInformation("freshing hotvalue");
            //            valueTime = DateTime.Now;
            //            startFreshValue();
            //        }
            //    }
            //    finally
            //    {
            //        freshValueLock.Release();
            //    }
            //}
            return hotList;
        }

        //private async Task FreshHotValueAsync(CancellationToken ct)
        //{
        //    var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        //    using var context = new IndependentDatabaseContext();
        //    var gcts = new Buaa.AIBot.Utils.GlobalCancellationTokenSource()
        //    {
        //        Source = cts
        //    };
        //    ITagRepostory tagRepostory = new TagRepository(context, serviceProvider.GetRequiredService<ICachePool<int>>(), gcts);
        //    IQuestionRepository questionRepository = new QuestionRepository(context, serviceProvider.GetRequiredService<ICachePool<int>>(), tagRepostory, gcts);
        //    ILikeRepository likeRepository = new LikeRepository(context, serviceProvider.GetRequiredService<ICachePool<int>>(), gcts);
        //    ILogger<HotListService> logger = this.logger;
        //    DateTime start = DateTime.Now;
        //    try
        //    {
        //        var questionList = await questionRepository.SelectQuestionsByTagsAsync(new int[0], int.MaxValue, int.MaxValue);
        //        foreach (int qid in questionList)
        //        {
        //            ct.ThrowIfCancellationRequested();
        //            await Task.Yield();
        //            ct.ThrowIfCancellationRequested();
        //            try
        //            {
        //                int hot = await CalculateHotValueAsync(qid, questionRepository, likeRepository, logger);
        //                if (hot < 0)
        //                {
        //                    continue;
        //                }
        //                await questionRepository.UpdateHotInfoAsync(qid, hot);
        //            }
        //            catch (QuestionNotExistException) { }
        //            catch (Exception e)
        //            {
        //                logger.LogWarning("unknown exception:{exception}", e);
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        logger.LogWarning("freshing hot-value cancelled");
        //    }
        //    finally
        //    {
        //        logger.LogInformation("freshing hot-value cost {sec} s", (DateTime.Now - start).TotalSeconds);
        //    }
        //}

        //private static async Task<int> CalculateHotValueAsync(
        //    int qid,
        //    IQuestionRepository questionRepository,
        //    ILikeRepository likeRepository,
        //    ILogger<HotListService> logger)
        //{
        //    try
        //    {
        //        var oldHot = await questionRepository.SelectHotInfoByIdAsync(qid);
        //        if (oldHot == null)
        //        {
        //            return -1;
        //        }
        //        var answers = await questionRepository.SelectAnswersForQuestionByIdAsync(qid);
        //        if (answers == null)
        //        {
        //            return -1;
        //        }
        //        int questionLikeCount;
        //        try
        //        {
        //            questionLikeCount = await likeRepository.SelectLikesCountForQuestionAfterTimeAsync(qid, oldHot.ModifyTime);
        //        }
        //        catch (QuestionNotExistException)
        //        {
        //            return -1;
        //        }
        //        int answerCount = 0;
        //        int answerLikeCount = 0;
        //        foreach (var aid in answers)
        //        {
        //            try
        //            {
        //                var res = await likeRepository.SelectLikesCountForAnswerAfterTimeAsync(aid, oldHot.ModifyTime);
        //                answerCount++;
        //                answerLikeCount += res;
        //            }
        //            catch (AnswerNotExistException) { }
        //        }
        //        questionLikeCount++;
        //        int decreaseTimes = (DateTime.Now - oldHot.ModifyTime).Minutes;
        //        double newBase = oldHot.HotValue * Math.Pow(0.9995, decreaseTimes); // 0.9995^(24 * 60) = 0.48
        //        double newHots = (answerCount * questionLikeCount * 0.5 / Math.Log10(questionLikeCount) + answerLikeCount) * 100000;
        //        double ret = newBase + newHots;
        //        return ret > int.MaxValue ? int.MaxValue : (int)ret;
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        throw;
        //    }
        //    catch (Exception e)
        //    {
        //        logger.LogWarning("unknown exception: {exception}", e);
        //        return -1;
        //    }
        //}
    }
}
