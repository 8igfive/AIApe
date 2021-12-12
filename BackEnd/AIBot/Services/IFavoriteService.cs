using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

namespace Buaa.AIBot.Services
{
    public interface IFavoriteService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="FavoriteNameTooLongException"></exception>
        /// <exception cref="UserNotExistException">creater not exist</exception>
        /// <param name="creater">uid</param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <returns>qid</returns>
        Task<int> AddFavoriteAsync(int creater, string name, string description);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <exception cref="FavoriteNotExistException"></exception>
        /// <param name="fid"></param>
        /// <returns></returns>
        Task<FavoriteInformation> GetFavoriteAsync(int fid);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="FavoriteNotExistException"></exception>
        /// <param name="fid"></param>
        /// <returns></returns>
        Task DeleteFavoriteAsync(int fid);

        /// <summary>
        /// fid is required. For other params, null mean no change.
        /// </summary>
        /// <exception cref="FavoriteNotExistException"></exception>
        /// <exception cref="FavoriteNameTooLongException"></exception>
        /// <param name="fid"></param>
        /// <param name="modifyItems"></param>
        /// <returns></returns>
        Task ModifyFavoriteAsync(int fid, FavoriteModifyItems modifyItems);

        /// <summary>
        /// check whether a question is collected in a favorite
        /// </summary>
        /// <exception cref="FavoriteNotExistException"></exception>
        /// <exception cref="QuestionNotExistException"></exception>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <returns></returns> 
        Task<bool> QuestionInFavoriteAsync(int fid, int qid);

        /// <summary>
        /// check whether a answer is collected in a favorite
        /// </summary>
        /// <exception cref="FavoriteNotExistException"></exception>
        /// <exception cref="AnswerNotExistException"></exception>
        /// <param name="fid"></param>
        /// <param name="aid"></param>
        /// <returns></returns> 
        Task<bool> AnswerInFavoriteAsync(int fid, int aid);

        /// <summary>
        /// for a Favorite, mark a Question as favorite or not.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <param name="target">true if want to mark, or false.</param>
        /// <returns></returns>
        Task<CollectProduceResult> CollectQuestionAsync(int uid, int fid, int qid, bool target);

        /// <summary>
        /// for a Favorite, mark a Answer as favorite or not.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="fid"></param>
        /// <param name="aid"></param>
        /// <param name="target">true if want to mark, or false.</param>
        /// <returns></returns>
        Task<CollectProduceResult> CollectAnswerAsync(int uid, int fid, int aid, bool target);
    }

    /// <summary>
    /// Implemention of <see cref="IFavoriteService"/>
    /// </summary>
    /// <remarks><seealso cref="IFavoriteService"/></remarks>
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository favoriteRepository;
        private readonly ICollectRepository collectRepository;

        public IFavoriteRepository FavoriteRepository => favoriteRepository;
        public ICollectRepository CollectRepository => collectRepository;
        public FavoriteService(IFavoriteRepository favoriteRepository, ICollectRepository collectRepository)
        {
            this.favoriteRepository = favoriteRepository;
            this.collectRepository = collectRepository;
        }

        public async Task<int> AddFavoriteAsync(int creator, string name, string description)
        {
            if (name.Length > Constants.FavoriteNameMaxLength)
            {
                throw new Services.Exceptions.FavoriteNameTooLongException(name.Length, Constants.FavoriteNameMaxLength);
            }
            try
            {
                int fid = await favoriteRepository.InsertFavoriteAsync(new FavoriteInfo
                {
                    CreatorId = creator,
                    Name = name,
                    Description = description
                });
                return fid;
            } 
            catch (Repository.Exceptions.UserNotExistException e)
            {
                throw new Services.Exceptions.UserNotExistException(creator, e);
            }
        }
    
        public async Task DeleteFavoriteAsync(int fid)
        {
            var favorite = await favoriteRepository.SelectFavoriteByIdAsync(fid);
            if (favorite == null)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid);
            }
            await favoriteRepository.DeleteFavoriteByIdAsync(fid);
        }
    
        public async Task ModifyFavoriteAsync(int fid, FavoriteModifyItems modifyItems)
        {
            if (modifyItems.Name != null && modifyItems.Name.Length > Constants.FavoriteNameMaxLength)
            {
                throw new Exceptions.FavoriteNameTooLongException(modifyItems.Name.Length, Constants.FavoriteNameMaxLength);
            }
            try
            {
                await favoriteRepository.UpdateFavoriteAsync(new FavoriteInfo
                {
                    FavoriteId = fid,
                    Name = modifyItems.Name,
                    Description = modifyItems.Description
                });
            }
            catch (Repository.Exceptions.FavoriteNotExistException e)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid, e);
            }
        }
    
        public async Task<FavoriteInformation> GetFavoriteAsync(int fid)
        {
            var favorite = await favoriteRepository.SelectFavoriteByIdAsync(fid);
            if (favorite == null)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid);
            }
            var answers = await collectRepository.SelectAnswersForFavoriteByIdAsync(fid);
            if (answers == null)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid);
            }
            var questions = await collectRepository.SelectQuestionsForFavoriteByIdAsync(fid);
            if (questions == null)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid);
            }
            return new FavoriteInformation
            {
                Creator = favorite.CreatorId,
                CreateTime = favorite.CreateTime,
                Name = favorite.Name,
                Description = favorite.Description,
                Questions = questions,
                Answers = answers
            };
        }
    
        public async Task<bool> QuestionInFavoriteAsync(int fid, int qid)
        {
            try
            {
                bool collected = await collectRepository.FavoriteCollectedQuestionAsync(fid, qid);
                return collected;
            }
            catch (Repository.Exceptions.FavoriteNotExistException e)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid, e);
            }
            catch (Repository.Exceptions.QuestionNotExistException e)
            {
                throw new Services.Exceptions.QuestionNotExistException(qid, e);
            }
        }

        public async Task<bool> AnswerInFavoriteAsync(int fid, int aid)
        {
            try
            {
                bool collected = await collectRepository.FavoriteCollectedAnswerAsync(fid, aid);
                return collected;
            }
            catch (Repository.Exceptions.FavoriteNotExistException e)
            {
                throw new Services.Exceptions.FavoriteNotExistException(fid, e);
            }
            catch (Repository.Exceptions.AnswerNotExistException e)
            {
                throw new Services.Exceptions.AnswerNotExistException(aid, e);
            }
        }

        public async Task<CollectProduceResult> CollectQuestionAsync(int uid, int fid, int qid, bool target)
        {
            var ret = new CollectProduceResult();
            if (target)
            {
                try
                {
                    await collectRepository.InsertCollectForQuestionAsync(fid, qid);
                    ret.Status = CollectProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.QuestionNotExistException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.questionNotExist;
                }
                catch (Repository.Exceptions.FavoriteHasCollectedTargetException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.alreadyCollected;
                }
            }
            else
            {
                try
                {
                    await collectRepository.DeleteCollectForQuestionAsync(fid, qid);
                    ret.Status = CollectProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.QuestionNotExistException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.questionNotExist;
                }
                catch (Repository.Exceptions.FavoriteNotCollectedTargetException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.notCollected;
                }
            }
            ret.Collected = await collectRepository.UserCollectedQuestionAsync(uid, qid);
            ret.CollectNum = await collectRepository.SelectCollectsCountForQuestionAsync(qid);
            return ret;
        }

        public async Task<CollectProduceResult> CollectAnswerAsync(int uid, int fid, int aid, bool target)
        {
            var ret = new CollectProduceResult();
            if (target)
            {
                try
                {
                    await collectRepository.InsertCollectForAnswerAsync(fid, aid);
                    ret.Status = CollectProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.AnswerNotExistException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.answerNotExist;
                }
                catch (Repository.Exceptions.FavoriteHasCollectedTargetException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.alreadyCollected;
                }
            }
            else
            {
                try
                {
                    await collectRepository.DeleteCollectForAnswerAsync(fid, aid);
                    ret.Status = CollectProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.AnswerNotExistException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.answerNotExist;
                }
                catch (Repository.Exceptions.FavoriteNotCollectedTargetException)
                {
                    ret.Status = CollectProduceResult.ResultStatus.notCollected;
                }
            }
            ret.Collected = await collectRepository.UserCollectedAnswerAsync(uid, aid);
            ret.CollectNum = await collectRepository.SelectCollectsCountForAnswerAsync(aid);
            return ret;
        }
    }

    public class CollectProduceResult
    {
        public enum ResultStatus
        {
            success,
            alreadyCollected,
            notCollected,
            questionNotExist,
            answerNotExist,
        }
        public ResultStatus Status { get; set; }
        public bool Collected { get; set; }
        public int CollectNum { get; set; }
    }
}