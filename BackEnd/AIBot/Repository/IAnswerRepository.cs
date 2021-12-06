using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

namespace Buaa.AIBot.Repository
{
    public interface IAnswerRepository
    {
        /// <summary>
        /// Select an answer by id.
        /// </summary>
        /// <param name="answerId">aid</param>
        /// <returns>an AnswerInfo object if exist, or null</returns>
        Task<AnswerInfo> SelectAnswerByIdAsync(int answerId);

        /// <summary>
        /// Select an answer by qid and uid.
        /// </summary>
        /// <remarks>
        /// if <paramref name="questionId"/> or <paramref name="userId"/> not match any Question or User, returns null, without any exception.
        /// </remarks>
        /// <param name="questionId"></param>
        /// <param name="userId"></param>
        /// <returns>an AnswerInfo object if exist, or null.</returns>
        Task<AnswerInfo> SelectAnswerByQuestionAndUserAsync(int questionId, int userId);

        /// <summary>
        /// Insert a new answer. AnswerId, CreateTime, ModifyTime will be generated automatically.
        /// </summary>
        /// <remarks>
        /// QuestionId and CreaterId is required.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">CreaterId is null.</exception>
        /// <exception cref="UserNotExistException">No User with uid=CreaterId.</exception>
        /// <exception cref="QuestionNotExistException">There is no question with qid=<paramref name="answer"/>.QuestionId</exception>
        /// <exception cref="UserHasAnswerTheQuestionException">There us an answer with given uid and qid</exception>
        /// <param name="answer">the new answer to store</param>
        /// <returns></returns>
        Task<int> InsertAnswerAsync(AnswerInfo answer);

        /// <summary>
        /// Update the answer with aid=<paramref name="answer"/>.AnswerId.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="question"/>.QuestionId and aid=<paramref name="answer"/>.AnswerId to appoint the question to be update.
        /// Every <paramref name="question"/>'s not-null Property will replace the old value.
        /// CreateTime, CreaterId, QuestionId will never change. ModifyTime will fresh automatically.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="AnswerNotExistException">There is no answer with qid=<paramref name="answer"/>.QuestionId and aid=<paramref name="answer"/>.AnswerId.</exception>
        /// <param name="answer">the new info for the answer</param>
        /// <returns></returns>
        Task UpdateAnswerAsync(AnswerInfo answer);

        /// <summary>
        /// Make sure no answer with aid=<paramref name="answerId"/>. (no operation if it has already not exist).
        /// </summary>
        /// <param name="questionId">qid</param>
        /// <param name="answerId">aid</param>
        /// <returns></returns>
        Task DeleteAnswerByIdAsync(int answerId);
    }
}
