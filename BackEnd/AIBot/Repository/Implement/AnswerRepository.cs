using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository.Implement
{
    /// <summary>
    /// Implement of <see cref="IAnswerRepository"/>.
    /// </summary>
    /// <remarks><seealso cref="IAnswerRepository"/></remarks>
    public class AnswerRepository : RepositoryBase, IAnswerRepository
    {
        public AnswerRepository(DatabaseContext context, ICachePool<int> cachePool, GlobalCancellationTokenSource globalCancellationTokenSource) 
            : base(context, cachePool, globalCancellationTokenSource.Token) { }

        public async Task<AnswerInfo> SelectAnswerByIdAsync(int answerId)
        {
            var query = await Context
                .Answers
                .Select(a => new AnswerInfo()
                {
                    AnswerId = a.AnswerId,
                    CreaterId = a.UserId,
                    QuestionId = a.QuestionId,
                    Content = a.Content,
                    CreateTime = a.CreateTime,
                    ModifyTime = a.ModifyTime
                })
                .Where(a => a.AnswerId == answerId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return query;
        }

        public async Task<AnswerInfo> SelectAnswerByQuestionAndUserAsync(int questionId, int userId)
        {
            var query = await Context
                .Answers
                .Select(a => new AnswerInfo()
                {
                    AnswerId = a.AnswerId,
                    CreaterId = a.UserId,
                    QuestionId = a.QuestionId,
                    Content = a.Content,
                    CreateTime = a.CreateTime,
                    ModifyTime = a.ModifyTime
                })
                .Where(a => a.QuestionId == questionId && a.CreaterId == userId)
                // .FirstOrDefaultAsync(CancellationToken);
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return query;
        }

        private async Task CheckInsertAsync(AnswerInfo answer)
        {
            if ((await Context.Users.FindAsync(answer.CreaterId)) == null)
            {
                throw new UserNotExistException(answer.CreaterId ?? 0);
            }
            if ((await Context.Questions.FindAsync(answer.QuestionId)) == null)
            {
                throw new QuestionNotExistException(answer.QuestionId);
            }
            var old = await Context
                .Answers
                .Select(a => new
                {
                    a.AnswerId,
                    a.UserId,
                    a.QuestionId
                })
                .Where(a => a.QuestionId == answer.QuestionId && a.UserId == answer.CreaterId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (old != null)
            {
                throw new UserHasAnswerTheQuestionException((int)answer.CreaterId, answer.QuestionId);
            }
        }

        public async Task<int> InsertAnswerAsync(AnswerInfo answer)
        {
            if (answer.CreaterId == null)
            {
                throw new ArgumentNullException(nameof(answer.CreaterId));
            }
            await CheckInsertAsync(answer);
            var target = new AnswerData()
            {
                UserId = answer.CreaterId,
                QuestionId = answer.QuestionId,
                Content = answer.Content
            };
            Context.Add(target);
            bool success = false;
            while (!success)
            {
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                await CheckInsertAsync(answer);
            }
            return target.AnswerId;
        }

        public async Task UpdateAnswerAsync(AnswerInfo answer)
        {
            var target = await Context.Answers.FindAsync(answer.AnswerId);
            if (target == null)
            {
                throw new AnswerNotExistException(answer.AnswerId);
            }
            bool success = true;
            if (answer.Content != null)
            {
                target.Content = answer.Content;
                success = false;
            }
            while (!success)
            {
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                if ((await Context.Answers.FindAsync(answer.AnswerId)) == null)
                {
                    throw new AnswerNotExistException(answer.AnswerId);
                }
            }
        }

        public async Task DeleteAnswerByIdAsync(int answerId)
        {
            var target = await Context.Answers.FindAsync(answerId);
            if (target != null)
            {
                Context.Remove(target);
                await SaveChangesAgainAndAgainAsync();
            }
        }
    }
}
