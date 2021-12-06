using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Repository.Models;

namespace Buaa.AIBot.Services
{
    public interface IQuestionService
    {
        Repository.ITagRepostory TagRepostory { get; }
        Repository.IQuestionRepository QuestionRepository { get; }
        Repository.IAnswerRepository AnswerRepository { get; }
        Repository.ILikeRepository LikeRepository { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <see cref="QuestionInformation.Like"/> will be set if uid is not null and legal.
        /// </remarks>
        /// <exception cref="QuestionNotExistException"></exception>
        /// <param name="qid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        Task<QuestionInformation> GetQuestionAsync(int qid, int? uid=null);

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// <see cref="QuestionInformation.Like"/> will be set if uid is not null and legal.
        /// </remarks>
        /// <exception cref="AnswerNotExistException"></exception>
        /// <param name="aid"></param>
        /// <param name="uid"></param>
        /// <returns></returns>
        Task<AnswerInformation> GetAnswerAsync(int aid, int? uid=null);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TagNotExistException"></exception>
        /// <param name="tid"></param>
        /// <returns></returns>
        Task<TagInformation> GetTagAsync(int tid);

        Task<IReadOnlyDictionary<int, TagCategory>> GetTagCategoryIndexAsync();

        Task<Dictionary<TagCategory, IEnumerable<int>>> ClassifyTagsAsync(IEnumerable<int> tags, IReadOnlyDictionary<int, TagCategory> tagIndex = null);

        /// <summary>
        /// Return qids 
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="pt"></param>
        /// <param name="number"></param>
        /// <returns></returns>
        Task<IEnumerable<int>> GetQuestionListAsync(IEnumerable<int> tags, int? pt, int number);

        Task<IEnumerable<int>> SearchQuestionAsync(string content, IEnumerable<int> tags);

        Task<IReadOnlyDictionary<string, int>> GetTagListAsync();

        Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>> GetTagCategoryAsync();

        Task<bool> QuestionIsCodeAsync(string title, string remarks);

        Task<Dictionary<string, int>> GenerageTagsForQuestionAsync(string title, string remarks);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="QuestionTitleTooLongException"></exception>
        /// <exception cref="UserNotExistException">creater not exist</exception>
        /// <exception cref="TagNotExistException"></exception>
        /// <param name="creater">uid</param>
        /// <param name="title"></param>
        /// <param name="remarks"></param>
        /// <param name="tags">tids</param>
        /// <returns>qid</returns>
        Task<int> AddQuestionAsync(int creater, string title, string remarks, IEnumerable<int> tags);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="UserHasAnswerTheQuestionException"></exception>
        /// <exception cref="UserNotExistException">creater not exist.</exception>
        /// <exception cref="QuestionNotExistException">qid not exist.</exception>
        /// <param name="creater">uid</param>
        /// <param name="qid"></param>
        /// <param name="content"></param>
        /// <returns>aid</returns>
        Task<int> AddAnswerAsync(int creater, int qid, string content);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TagNameTooLongException"></exception>
        /// <exception cref="TagNameExistException"></exception>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns>tid</returns>
        Task<int> AddTagAsync(string name, string desc, string category);

        /// <summary>
        /// qid is required. For other params, null mean no change.
        /// </summary>
        /// <exception cref="QuestionNotExistException"></exception>
        /// <exception cref="QuestionTitleTooLongException"></exception>
        /// <exception cref="TagNotExistException">If Tags is not null and there is a tid in the list that no tag match it</exception>
        /// <exception cref="AnswerNotExistException">If BestAnswer is not null and no answer match it. </exception>
        /// <param name="qid"></param>
        /// <param name="modifyItems"></param>
        /// <returns></returns>
        Task ModifyQuestionAsync(int qid, QuestionModifyItems modifyItems);

        /// <summary>
        /// aid is required. For other params, null mean no change.
        /// </summary>
        /// <exception cref="AnswerNotExistException"></exception>
        /// <param name="aid"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task ModifyAnswerAsync(int aid, string content);

        /// <summary>
        /// tid is required. For other params, null mean no change.
        /// </summary>
        /// <exception cref="TagNotExistException"></exception>
        /// <exception cref="TagNameTooLongException"></exception>
        /// <exception cref="TagNameExistException"></exception>
        /// <param name="tid"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <returns></returns>
        Task ModifyTagAsync(int tid, string name, string desc, string category);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="QuestionNotExistException"></exception>
        /// <param name="qid"></param>
        /// <returns></returns>
        Task DeleteQuestionAsync(int qid);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="AnswerNotExistException"></exception>
        /// <param name="aid"></param>
        /// <returns></returns>
        Task DeleteAnswerAsync(int aid);

        /// <summary>
        /// 
        /// </summary>
        /// <exception cref="TagNotExistException"></exception>
        /// <param name="tid"></param>
        /// <returns></returns>
        Task DeleteTagAsync(int tid);

        /// <summary>
        /// for a User, mark a Question as Like or unmark.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="qid"></param>
        /// <param name="target">true if want to mark, or false.</param>
        /// <returns></returns>
        Task<LikeProduceResult> LikeQuestionAsync(int uid, int qid, bool target);

        /// <summary>
        /// for a User, mark a Answer as Like or unmark.
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="aid"></param>
        /// <param name="target">true if want to mark, or false.</param>
        /// <returns></returns>
        Task<LikeProduceResult> LikeAnswerAsync(int uid, int aid, bool target);
    }

    public class LikeProduceResult
    {
        public enum ResultStatus
        {
            success,
            alreadyLiked,
            notLiked,
            questionNotExist,
            answerNotExist,
        }

        public ResultStatus Status { get; set; }
        public bool UserLiked { get; set; }
        public int LikeNum { get; set; }
    }
}
