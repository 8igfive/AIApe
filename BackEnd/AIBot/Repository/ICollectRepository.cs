using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository
{
    public interface ICollectRepository : IRepositoryBase
    {
        /// <summary>
        /// Select all answers for the favorite with given fid.
        /// </summary>
        /// <remarks> 
        /// Return null if the favorite not exist.
        /// Return an empty list if no answers for this favorite.
        /// </remarks>
        /// <param name="favaoriteId"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> SelectAnswersForFavoriteByIdAsync(int favoriteId);

        /// <summary>
        /// Select all questions for the favorite with given fid.
        /// </summary>
        /// <remarks> 
        /// Return null if the favorite not exist.
        /// Return an empty list if no question for this favorite.
        /// </remarks>
        /// <param name="favaoriteId"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> SelectQuestionsForFavoriteByIdAsync(int favoriteId);

        /// <summary>
        /// Mark the question as collected for favorite.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="FavoriteCollectedQuestionAsync(int, int)"/> returns true when using same params, 
        /// and the result of <see cref="SelectCollectsCountForQuestionAsync(int)"/> increased.
        /// </remarks>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question.</exception>
        /// <exception cref="FavoriteHasCollectedTargetException"><see cref="FavoriteCollectedQuestionAsync(int, int)"/> return true before call.</exception>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task InsertCollectForQuestionAsync(int fid, int qid);

        /// <summary>
        /// Unmark the question as collected for favorite.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="FavoriteCollectedQuestionAsync(int, int)"/> returns false when using same params, 
        /// and the result of <see cref="SelectCollectsCountForQuestionAsync(int)"/> decreased.
        /// </remarks>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question.</exception>
        /// <exception cref="FavoriteNotCollectedTargetException"><see cref="FavoriteCollectedQuestionAsync(int, int)"/> return false before call.</exception>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task DeleteCollectForQuestionAsync(int fid, int qid);

        /// <summary>
        /// Mark the answer as collected for favorite.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="FavoriteCollectedAnswerAsync(int, int)"/> returns true when using same params, 
        /// and the result of <see cref="SelectCollectsCountForAnswerAsync(int)"/> increased.
        /// </remarks>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="AnswerNotExistException">given qid matches no Question.</exception>
        /// <exception cref="FavoriteHasCollectedTargetException"><see cref="FavoriteCollectedAnswerAsync(int, int)"/> return true before call.</exception>
        /// <param name="fid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task InsertCollectForAnswerAsync(int fid, int aid);

        /// <summary>
        /// Unmark the answer as collected for favorite.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="FavoriteCollectedAnswerAsync(int, int)"/> returns false when using same params, 
        /// and the result of <see cref="SelectCollectsCountForAnswerAsync(int)"/> decreased.
        /// </remarks>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="AnswerNotExistException">given qid matches no Question.</exception>
        /// <exception cref="FavoriteNotCollectedTargetException"><see cref="FavoriteCollectedAnswerAsync(int, int)"/> return false before call.</exception>
        /// <param name="fid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task DeleteCollectForAnswerAsync(int fid, int aid);

        /// <summary>
        /// Get whether the favorite collected this question.
        /// </summary>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="fid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<bool> FavoriteCollectedQuestionAsync(int fid, int qid);

        /// <summary>
        /// Get the count for users collecting the question.
        /// </summary>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<int> SelectCollectsCountForQuestionAsync(int qid);

        /// <summary>
        /// Get whether the favorite collected this answer.
        /// </summary>
        /// <exception cref="FavoriteNotExistException">given fid matches no Favorite.</exception>
        /// <exception cref="AnswerNotExistException">given qid matches no Question. </exception>
        /// <param name="fid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<bool> FavoriteCollectedAnswerAsync(int fid, int aid);

        /// <summary>
        /// Get the count for users collecting the answer.
        /// </summary>
        /// <exception cref="AnswerNotExistException">given aid matches no Answer. </exception>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<int> SelectCollectsCountForAnswerAsync(int aid);

        /// <summary>
        /// Get whether the user collected this question.
        /// </summary>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="uid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<bool> UserCollectedQuestionAsync(int uid, int qid);

        /// <summary>
        /// Get whether the user collected this answer.
        /// </summary>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="AnswerNotExistException">given qid matches no Question. </exception>
        /// <param name="uid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<bool> UserCollectedAnswerAsync(int uid, int aid);
    }
}