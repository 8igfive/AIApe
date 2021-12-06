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

namespace Buaa.AIBot.TimedTask
{
    public class HotValueFresher
    {
        public static HotValueFresher DEFAULT { get; } = new HotValueFresher();

        private ILogger<HotValueFresher> logger;

        public async Task FreshHotValueAsync(CancellationToken ct, TimedTaskManager.LoggerFactory loggerFactory)
        {
            await Task.Delay(TimeSpan.FromSeconds(10));
            logger ??=loggerFactory.GetLogger<HotValueFresher>();
            logger.LogInformation("FreshHotValueAsync start");
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            using var context = new IndependentDatabaseContext(loggerFactory.GetLogger<IndependentDatabaseContext>());
            var gcts = new Utils.GlobalCancellationTokenSource()
            {
                Source = cts
            };
            ITagRepostory tagRepostory = new TagRepository(context, CachePool<int>.DEFAULT, gcts);
            IQuestionRepository questionRepository = 
                new QuestionRepository(
                    context,
                    CachePool<int>.DEFAULT, 
                    tagRepostory, 
                    gcts, 
                    loggerFactory.GetLogger<QuestionRepository>());
            ILikeRepository likeRepository = new LikeRepository(context, CachePool<int>.DEFAULT, gcts);
            DateTime start = DateTime.Now;
            try
            {
                int lastQid = int.MaxValue;
                int batchSize = 32;
                int count = batchSize;
                while (count == batchSize)
                {
                    var questionList = await questionRepository.SelectQuestionsByTagsAsync(new int[0], lastQid, batchSize);
                    count = 0;
                    foreach (int qid in questionList)
                    {
                        count++;
                        lastQid = qid;
                        ct.ThrowIfCancellationRequested();
                        await Task.Yield();
                        ct.ThrowIfCancellationRequested();
                        try
                        {
                            int hot = await CalculateHotValueAsync(qid, questionRepository, likeRepository);
                            if (hot < 0)
                            {
                                continue;
                            }
                            await questionRepository.UpdateHotInfoAsync(qid, hot);
                        }
                        catch (QuestionNotExistException) { }
                        catch (Exception e)
                        {
                            logger.LogWarning("unknown exception:{exception}", e);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                logger.LogWarning("freshing hot-value cancelled");
            }
            finally
            {
                logger.LogInformation("freshing hot-value cost {sec} s", (DateTime.Now - start).TotalSeconds);
            }
        }

        private Dictionary<int, double> cachedDamping = new Dictionary<int, double>();

        private double CalculateDamping(int times)
        {
            if (cachedDamping.TryGetValue(times, out var ret))
            {
                return ret;
            }
            ret = Math.Pow(0.9995, times);
            cachedDamping[times] = ret;
            return ret;
        }

        private async Task<int> CalculateHotValueAsync(
            int qid,
            IQuestionRepository questionRepository,
            ILikeRepository likeRepository)
        {
            try
            {
                var oldHot = await questionRepository.SelectHotInfoByIdAsync(qid);
                if (oldHot == null)
                {
                    return -1;
                }
                var answers = await questionRepository.SelectAnswersForQuestionByIdAsync(qid);
                if (answers == null)
                {
                    return -1;
                }
                int questionLikeCount;
                try
                {
                    questionLikeCount = await likeRepository.SelectLikesCountForQuestionAfterTimeAsync(qid, oldHot.ModifyTime);
                }
                catch (QuestionNotExistException)
                {
                    return -1;
                }
                int answerCount = 0;
                int answerLikeCount = 0;
                foreach (var aid in answers)
                {
                    try
                    {
                        var res = await likeRepository.SelectLikesCountForAnswerAfterTimeAsync(aid, oldHot.ModifyTime);
                        answerCount++;
                        answerLikeCount += res;
                    }
                    catch (AnswerNotExistException) { }
                }
                questionLikeCount++;
                int decreaseTimes = (DateTime.Now - oldHot.ModifyTime).Minutes;
                double newBase = oldHot.HotValue * CalculateDamping(decreaseTimes); // 0.9995^(24 * 60) = 0.48
                double newHots = (answerCount * questionLikeCount * 0.5 / Math.Log10(questionLikeCount + 10) + answerLikeCount) * 100000;
                double ret = newBase + newHots;
                // logger.LogInformation("new hot-value for question[{qid}] is {hot}", qid, ret);
                return ret > int.MaxValue ? int.MaxValue : (int)ret;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception e)
            {
                logger.LogWarning("unknown exception: {exception}", e);
                return -1;
            }
        }
    }
}
