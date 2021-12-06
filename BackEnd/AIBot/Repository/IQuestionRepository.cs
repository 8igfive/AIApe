using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository
{
    public interface IQuestionRepository : IRepositoryBase
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionId"></param>
        /// <returns>null if the question not exist.</returns>
        Task<QuestionHotInfo> SelectHotInfoByIdAsync(int questionId);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="QuestionNotExistException">given qid matches no Question.</exception>
        /// <param name="questionId"></param>
        /// <param name="hotValue"></param>
        /// <returns></returns>
        Task UpdateHotInfoAsync(int questionId, int hotValue);

        /// <summary>
        /// Return questions whose tags contains all the given <paramref name="tags"/>.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> SelectQuestionsByTagsAsync(IEnumerable<int> tags, int maxQid = int.MaxValue, int num = int.MaxValue);

        /// <summary>
        /// Returns the most hot questions.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> SelectQuestionsHotValueAsync(int count);

        /// <summary>
        /// Select a question by id.
        /// </summary>
        /// <param name="questionId">qid</param>
        /// <returns>a QuestionInfo object if exist, or null</returns>
        Task<QuestionInfo> SelectQuestionByIdAsync(int questionId);

        /// <summary>
        /// Select all answers for the question with given qid.
        /// </summary>
        /// <remarks> 
        /// Return null if the question not exist.
        /// Return an empty list if no tags for this question.
        /// </remarks>
        /// <param name="questionId"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> SelectAnswersForQuestionByIdAsync(int questionId);

        /// <summary>
        /// Select all tags for the question with given qid.
        /// </summary>
        /// <remarks> 
        /// Return null if the question not exist.
        /// Return an empty dictionary if no tags for this question.
        /// </remarks>
        /// <param name="questionId">qid</param>
        /// <returns>name-tid dictionary for tags.</returns>
        Task<IEnumerable<KeyValuePair<string, int>>> SelectTagsForQuestionByIdAsync(int questionId);

        /// <summary>
        /// Insert a new question. QuestionId, CreateTime, and ModifyTime will be generated automatically.
        /// </summary>
        /// <remarks>
        /// CreaterId is required.
        /// HotValue and HotFreshTime will be ignored.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">CreaterId is null</exception>
        /// <exception cref="QuestionTitleTooLongException">Title Length greater than limits.</exception>
        /// <exception cref="UserNotExistException">There is no user with uid=<paramref name="question"/>.CreaterId. </exception>
        /// <exception cref="TagNotExistException">There is a tid in <paramref name="question"/>.Tags, but no Tag has the same. </exception>
        /// <param name="question">the new question to store</param>
        /// <returns>qid</returns>
        Task<int> InsertQuestionAsync(QuestionWithListTag question);

        /// <summary>
        /// Update the question with qid=<paramref name="question"/>.QuestionId.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="question"/>.QuestionId to appoint the question to be update
        /// Every <paramref name="question"/>'s not-null Property will replace the old value.
        /// HotValue and HotFreshTime will be ignored.
        /// CreateTime, CreaterId will never change. ModifyTime will fresh automatically.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="QuestionNotExistException">There is no question with given qid.</exception>
        /// <exception cref="QuestionTitleTooLongException">Title Length greater than limits.</exception>
        /// <exception cref="TagNotExistException">There is a tid in <paramref name="question"/>.Tags, but no Tag has the same. </exception>
        /// <param name="question">the new info for the question</param>
        /// <returns></returns>
        Task UpdateQuestionAsync(QuestionWithListTag question);

        /// <summary>
        /// Make sure no question whose Id is <paramref name="questionId"/>. (no operation if it has already not exist).
        /// </summary>
        /// <remarks>
        /// All the answers will be removed if a question is removed.
        /// </remarks>
        /// <param name="questionId">aid</param>
        /// <returns></returns>
        Task DeleteQuestionByIdAsync(int questionId);
    }
}
