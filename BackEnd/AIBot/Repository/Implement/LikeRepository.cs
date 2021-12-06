using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Repository.Exceptions;

namespace Buaa.AIBot.Repository.Implement
{
    public class LikeRepository : RepositoryBase, ILikeRepository
    {
        public LikeRepository(DatabaseContext context, ICachePool<int> cachePool, 
            Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellationTokenSource) 
            : base(context, cachePool, globalCancellationTokenSource.Token)
        { }

        private async Task CheckUserExistAsync(int uid)
        {
            var user = await Context.Users
                .Where(u => u.UserId == uid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new UserNotExistException(uid);
            }
        }

        private async Task CheckQuestionExistAsync(int qid)
        {
            var question = await Context.Questions
                .Where(q => q.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (question == null)
            {
                throw new QuestionNotExistException(qid);
            }
        }

        private async Task CheckAnswerExistAsync(int aid)
        {
            var answer = await Context.Answers
                .Where(q => q.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (answer == null)
            {
                throw new AnswerNotExistException(aid);
            }
        }

        public async Task<IEnumerable<int>> SelectLikedQuestionsForUserAsync(int uid)
        {
            var ret = await Context.LikeQuestions
                .Where(lq => lq.UserId == uid)
                .Select(lq => lq.QuestionId)
                .ToListAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (ret.Count != 0)
            {
                return ret;
            }
            var user = await Context.Users.SingleOrDefaultAsync(u => u.UserId == uid, CancellationToken);
            if (user == null)
            {
                return null;
            }
            return ret;
        }

        public async Task<IEnumerable<int>> SelectLikedAnswersForUserAsync(int uid)
        {
            var ret = await Context.LikeAnswers
                .Where(la => la.UserId == uid)
                .Select(la => la.AnswerId)
                .ToListAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (ret.Count != 0)
            {
                return ret;
            }
            var user = await Context.Users.SingleOrDefaultAsync(u => u.UserId == uid, CancellationToken);
            if (user == null)
            {
                return null;
            }
            return ret;
        }

        public async Task<int> SelectLikesCountForAnswerAsync(int aid)
        {
            await CheckAnswerExistAsync(aid);
            var res = await Context.LikeAnswers
                .Where(la => la.AnswerId == aid)
                .CountAsync();
            CancellationToken.ThrowIfCancellationRequested();
            return res;
        }

        public async Task<int> SelectLikesCountForQuestionAsync(int qid)
        {
            await CheckQuestionExistAsync(qid);
            var res = await Context.LikeQuestions
                .Where(lq => lq.QuestionId == qid)
                .CountAsync();
            CancellationToken.ThrowIfCancellationRequested();
            return res;
        }

        public async Task<int> SelectLikesCountForAnswerAfterTimeAsync(int aid, DateTime start)
        {
            await CheckAnswerExistAsync(aid);
            var res = await Context.LikeAnswers
                .Where(la => la.AnswerId == aid)
                .Where(la => la.CreateTime >= start)
                .CountAsync();
            CancellationToken.ThrowIfCancellationRequested();
            return res;
        }

        public async Task<int> SelectLikesCountForQuestionAfterTimeAsync(int qid, DateTime start)
        {
            await CheckQuestionExistAsync(qid);
            var res = await Context.LikeQuestions
                .Where(lq => lq.QuestionId == qid)
                .Where(lq => lq.CreateTime >= start)
                .CountAsync();
            CancellationToken.ThrowIfCancellationRequested();
            return res;
        }

        public async Task<bool> UserLikedAnswerAsync(int uid, int aid)
        {
            await CheckUserExistAsync(uid);
            await CheckAnswerExistAsync(aid);
            var res = await Context.LikeAnswers
                .Where(lq => lq.AnswerId == aid && lq.UserId == uid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res != null;
        }

        public async Task<bool> UserLikedQuestionAsync(int uid, int qid)
        {
            await CheckUserExistAsync(uid);
            await CheckQuestionExistAsync(qid);
            var res = await Context.LikeQuestions
                .Where(lq => lq.QuestionId == qid && lq.UserId == uid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res != null;
        }

        public async Task InsertLikeForQuestionAsync(int uid, int qid)
        {
            var old = await Context.LikeQuestions
                .Where(lq => lq.UserId == uid && lq.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old != null)
            {
                throw new UserHasLikedTargetException(uid, qid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            await CheckQuestionExistAsync(qid);
            await CheckUserExistAsync(uid);
            var item = new LikeQuestion()
            {
                QuestionId = qid,
                UserId = uid
            };
            Context.Add(item);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task DeleteLikeForQuestionAsync(int uid, int qid)
        {
            await CheckQuestionExistAsync(qid);
            await CheckUserExistAsync(uid);
            var old = await Context.LikeQuestions
                .Where(lq => lq.UserId == uid && lq.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old == null)
            {
                throw new UserNotLikedTargetException(uid, qid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            Context.Remove(old);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task InsertLikeForAnswerAsync(int uid, int aid)
        {
            var old = await Context.LikeAnswers
                .Where(lq => lq.UserId == uid && lq.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old != null)
            {
                throw new UserHasLikedTargetException(uid, aid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            await CheckAnswerExistAsync(aid);
            await CheckUserExistAsync(uid);
            var item = new LikeAnswer()
            {
                AnswerId = aid,
                UserId = uid
            };
            Context.Add(item);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task DeleteLikeFroAnswerAsync(int uid, int aid)
        {
            await CheckAnswerExistAsync(aid);
            await CheckUserExistAsync(uid);
            var old = await Context.LikeAnswers
                .Where(lq => lq.UserId == uid && lq.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old == null)
            {
                throw new UserNotLikedTargetException(uid, aid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            Context.Remove(old);
            await SaveChangesAgainAndAgainAsync();
        }
    }
}
