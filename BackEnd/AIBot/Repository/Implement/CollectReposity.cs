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
    /// Implement of <see cref="IFavoriteRepository"/>.
    /// </summary>
    /// <remarks><seealso cref="IFavoriteRepository"/></remarks>
    public class CollectRepository : RepositoryBase, ICollectRepository
    {
        private ILogger<CollectRepository> logger;

        public CollectRepository(DatabaseContext context, ICachePool<int> cachePool, 
            Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellationTokenSource)
            : base(context, cachePool, globalCancellationTokenSource.Token)
            { }
        public CollectRepository(DatabaseContext context, ICachePool<int> cachePool, 
            Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellationTokenSource, ILogger<CollectRepository> logger)
            : base(context, cachePool, globalCancellationTokenSource.Token)
            {
                this.logger = logger;
            }
    
        public async Task<IEnumerable<int>> SelectAnswersForFavoriteByIdAsync(int favoriteId)
        {
            var query = await Context
                .FavoriteAnswerRelations
                .Where(far => far.FavoriteId == favoriteId)
                .Select(far => far.AnswerId)
                .ToListAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var favorite = Context.Favorites.FindAsync(favoriteId);
            if (favorite == null)
            {
                return null;
            }
            return query;
        }
    
        public async Task<IEnumerable<int>> SelectQuestionsForFavoriteByIdAsync(int favoriteId)
        {
            var query = await Context
                .FavoriteQuestionRelations
                .Where(fqr => fqr.FavoriteId == favoriteId)
                .Select(fqr => fqr.QuestionId)
                .ToListAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var favorite = Context.Favorites.FindAsync(favoriteId);
            if (favorite == null)
            {
                return null;
            }
            return query;
        }

        public async Task InsertCollectForQuestionAsync(int fid, int qid)
        {
            var old = Context.FavoriteQuestionRelations
                .Where(fqr => fqr.FavoriteId == fid && fqr.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old != null)
            {
                throw new FavoriteHasCollectedTargetException(fid, qid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            await CheckQuestionExistAsync(qid);
            await CheckFavoriteExistAsync(fid);
            var item = new FavoriteQuestionRelation
            {
                FavoriteId = fid,
                QuestionId = qid
            };
            Context.FavoriteQuestionRelations.Add(item);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task DeleteCollectForQuestionAsync(int fid, int qid)
        {
            await CheckQuestionExistAsync(qid);
            await CheckFavoriteExistAsync(fid);
            var old = await Context.FavoriteQuestionRelations
                .Where(fqr => fqr.FavoriteId == fid && fqr.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old == null)
            {
                throw new FavoriteNotCollectedTargetException(fid, qid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            Context.Remove(old);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task InsertCollectForAnswerAsync(int fid, int aid)
        {
            var old = Context.FavoriteAnswerRelations
                .Where(far => far.FavoriteId == fid && far.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old != null)
            {
                throw new FavoriteHasCollectedTargetException(fid, aid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            await CheckAnswerExistAsync(aid);
            await CheckFavoriteExistAsync(fid);
            var item = new FavoriteAnswerRelation
            {
                FavoriteId = fid,
                AnswerId = aid
            };
            Context.FavoriteAnswerRelations.Add(item);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task DeleteCollectForAnswerAsync(int fid, int aid)
        {
            await CheckAnswerExistAsync(aid);
            await CheckFavoriteExistAsync(fid);
            var old = await Context.FavoriteAnswerRelations
                .Where(far => far.FavoriteId == fid && far.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            if (old == null)
            {
                throw new FavoriteNotCollectedTargetException(fid, aid);
            }
            CancellationToken.ThrowIfCancellationRequested();
            Context.Remove(old);
            await SaveChangesAgainAndAgainAsync();
        }

        public async Task<bool> FavoriteCollectedQuestionAsync(int fid, int qid)
        {
            await CheckFavoriteExistAsync(fid);
            await CheckQuestionExistAsync(qid);
            var res = await Context.FavoriteQuestionRelations
                .Where(fqr => fqr.FavoriteId == fid && fqr.QuestionId == qid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res != null;
        }

        public async Task<int> SelectCollectsCountForQuestionAsync(int qid)
        {
            await CheckQuestionExistAsync(qid);    
            var temp = await Context.FavoriteQuestionRelations
                .Where(fqr => fqr.QuestionId == qid)
                .Join(Context.Favorites, fqr => fqr.FavoriteId, f => f.FavoriteId, (fqr, f) => f.UserId)
                .ToDictionaryAsync(uid => uid, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return temp.Count;
        }

        public async Task<bool> FavoriteCollectedAnswerAsync(int fid, int aid)
        {
            await CheckFavoriteExistAsync(fid);
            await CheckAnswerExistAsync(aid);
            var res = await Context.FavoriteAnswerRelations
                .Where(far => far.FavoriteId == fid && far.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res != null;
        }

        public async Task<int> SelectCollectsCountForAnswerAsync(int aid)
        {
            await CheckAnswerExistAsync(aid);
            var temp = await Context.FavoriteAnswerRelations
                .Where(far => far.AnswerId == aid)
                .Join(Context.Favorites, far => far.FavoriteId, f => f.FavoriteId, (far, f) => f.UserId)
                .ToDictionaryAsync(uid => uid, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return temp.Count;
        }

        public async Task<bool> UserCollectedQuestionAsync(int uid, int qid)
        {
            await CheckUserExistAsync(uid);
            await CheckQuestionExistAsync(qid);
            var res = await Context.FavoriteQuestionRelations
                .Where(fqr => fqr.QuestionId == qid)
                .Join(Context.Favorites, fqr => fqr.FavoriteId, f => f.FavoriteId, (fqr, f) => new {f.UserId})
                .Where(t => t.UserId == uid)
                .ToArrayAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res.Length > 0;
        }

        public async Task<bool> UserCollectedAnswerAsync(int uid, int aid)
        {
            await CheckUserExistAsync(uid);
            await CheckAnswerExistAsync(aid);
            var res = await Context.FavoriteAnswerRelations
                .Where(far => far.AnswerId == aid)
                .Join(Context.Favorites, far => far.FavoriteId, f => f.FavoriteId, (fqr, f) => new {f.UserId})
                .Where(t => t.UserId == uid)
                .ToArrayAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return res.Length > 0;
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
            var question = await Context.Answers
                .Where(a => a.AnswerId == aid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (question == null)
            {
                throw new AnswerNotExistException(aid);
            }
        }
    
        private async Task CheckFavoriteExistAsync(int fid)
        {
            var favorite = await Context.Favorites
                .Where(f => f.FavoriteId == fid)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (favorite == null)
            {
                throw new FavoriteNotExistException(fid);
            }
        }
    
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
    }
}