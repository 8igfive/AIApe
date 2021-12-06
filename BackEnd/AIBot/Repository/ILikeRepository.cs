using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository
{
    public interface ILikeRepository : IRepositoryBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>null if user not exist.</returns>
        Task<IEnumerable<int>> SelectLikedQuestionsForUserAsync(int uid);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uid"></param>
        /// <returns>null if user not exist.</returns>
        Task<IEnumerable<int>> SelectLikedAnswersForUserAsync(int uid);

        /// <summary>
        /// Get the count for users liking the question.
        /// </summary>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<int> SelectLikesCountForQuestionAsync(int qid);

        /// <summary>
        /// Get the count for users liking the answer.
        /// </summary>
        /// <exception cref="AnswerNotExistException">given aid matches no Answer.</exception>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<int> SelectLikesCountForAnswerAsync(int aid);

        /// <summary>
        /// Get the count for users liking the question.
        /// </summary>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<int> SelectLikesCountForQuestionAfterTimeAsync(int qid, DateTime start);

        /// <summary>
        /// Get the count for users liking the answer.
        /// </summary>
        /// <exception cref="AnswerNotExistException">given aid matches no Answer.</exception>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<int> SelectLikesCountForAnswerAfterTimeAsync(int aid, DateTime start);

        /// <summary>
        /// Get weather the user liked this question.
        /// </summary>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question. </exception>
        /// <param name="uid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task<bool> UserLikedQuestionAsync(int uid, int qid);

        /// <summary>
        /// Get weather the user liked this answer.
        /// </summary>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="AnswerNotExistException">given aid matches no Answer.</exception>
        /// <param name="uid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task<bool> UserLikedAnswerAsync(int uid, int aid);

        /// <summary>
        /// Mark the question as liked for user.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="UserLikedQuestion(int, int)"/> returns true when using same params, 
        /// and the result of <see cref="SelectLikesCountForQuestion(int)"/> increased.
        /// </remarks>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question.</exception>
        /// <exception cref="UserHasLikedTargetException"><see cref="UserLikedQuestion(int, int)"/> return true before call.</exception>
        /// <param name="uid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task InsertLikeForQuestionAsync(int uid, int qid);

        /// <summary>
        /// Unmark the question as liked for user.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="UserLikedQuestion(int, int)"/> returns false when using same params, 
        /// and the result of <see cref="SelectLikesCountForQuestion(int)"/> decreased.
        /// </remarks>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="QuestionNotExistException">given qid matches no Question.</exception>
        /// <exception cref="UserNotLikedTargetException"><see cref="UserLikedQuestion(int, int)"/> return false before call.</exception>
        /// <param name="uid"></param>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task DeleteLikeForQuestionAsync(int uid, int qid);

        /// <summary>
        /// Mark the answer as liked for user.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="UserLikedAnswer(int, int)"/> returns true when using same params.
        /// and the result of <see cref="SelectLikesCountForAnswer(int)"/> increased.
        /// </remarks>
        /// <exception cref="UserHasLikedTargetException"><see cref="UserLikedAnswer(int, int)"/> return true before call.</exception>
        /// <param name="uid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task InsertLikeForAnswerAsync(int uid, int aid);

        /// <summary>
        /// Unmark the answer as liked for user.
        /// </summary>
        /// <remarks>
        /// After call, <see cref="UserLikedAnswer(int, int)"/> returns false when using same params.
        /// and the result of <see cref="SelectLikesCountForAnswer(int)"/> decreased.
        /// </remarks>
        /// <exception cref="UserNotExistException">given uid matches no User.</exception>
        /// <exception cref="AnswerNotExistException">given qid matches no Question.</exception>
        /// <exception cref="UserNotLikedTargetException"><see cref="UserLikedAnswer(int, int)"/> return false before call.</exception>
        /// <param name="uid"></param>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task DeleteLikeFroAnswerAsync(int uid, int aid);
    }
}
