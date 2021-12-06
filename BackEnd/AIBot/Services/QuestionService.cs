using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Services
{
    /// <summary>
    /// Implemention of <see cref="IQuestionService"/>
    /// </summary>
    /// <remarks><seealso cref="IQuestionService"/></remarks>
    public class QuestionService : IQuestionService
    {
        private readonly IQuestionRepository questionRepository;
        private readonly IAnswerRepository answerRepository;
        private readonly ITagRepostory tagRepostory;
        private readonly ILikeRepository likeRepository;
        private readonly INLPService nlpService;

        public ITagRepostory TagRepostory => tagRepostory;

        public IQuestionRepository QuestionRepository => questionRepository;

        public IAnswerRepository AnswerRepository => AnswerRepository;

        public ILikeRepository LikeRepository => likeRepository;

        public QuestionService(IQuestionRepository questionRepository, IAnswerRepository answerRepository, ITagRepostory tagRepostory, ILikeRepository likeRepository, INLPService nlpService)
        {
            this.questionRepository = questionRepository;
            this.answerRepository = answerRepository;
            this.tagRepostory = tagRepostory;
            this.likeRepository = likeRepository;
            this.nlpService = nlpService;
        }

        public async Task<QuestionInformation> GetQuestionAsync(int qid, int? uid=null)
        {
            var question = await questionRepository.SelectQuestionByIdAsync(qid);
            if (question == null)
            {
                throw new Exceptions.QuestionNotExistException(qid);
            }
            var answers = await questionRepository.SelectAnswersForQuestionByIdAsync(qid);
            if (answers == null)
            {
                throw new Exceptions.QuestionNotExistException(qid);
            }
            var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
            if (tags == null)
            {
                throw new Exceptions.QuestionNotExistException(qid);
            }
            bool? like = null;
            int likeNum;
            try
            {
                if (uid != null)
                {
                    try
                    {
                        bool res = await likeRepository.UserLikedQuestionAsync((int)uid, qid);
                        like = res;
                    }
                    catch (Repository.Exceptions.UserNotExistException)
                    {
                        like = null;
                    }
                }
                likeNum = await likeRepository.SelectLikesCountForQuestionAsync(qid);
            }
            catch (Repository.Exceptions.QuestionNotExistException e)
            {
                throw new Exceptions.QuestionNotExistException(qid, e);
            }
            return new QuestionInformation()
            {
                Title = question.Title,
                Remarks = question.Remarks,
                Creator = question.CreaterId,
                Like = like,
                LikeNum = likeNum,
                HotValue = question.HotValue,
                CreateTime = question.CreateTime,
                HotFreshTime = question.HotFreshTime,
                ModifyTime = question.ModifyTime,
                Tags = new Dictionary<string, int>(tags),
                Answers = answers
            };
        }

        public async Task<AnswerInformation> GetAnswerAsync(int aid, int? uid=null)
        {
            var answer = await answerRepository.SelectAnswerByIdAsync(aid);
            if (answer == null)
            {
                throw new Exceptions.AnswerNotExistException(aid);
            }
            bool? like = null;
            int likeNum;
            try
            {
                if (uid != null)
                {
                    try
                    {
                        bool res = await likeRepository.UserLikedAnswerAsync((int)uid, aid);
                        like = res;
                    }
                    catch (Repository.Exceptions.UserNotExistException)
                    {
                        like = null;
                    }
                }
                likeNum = await likeRepository.SelectLikesCountForAnswerAsync(aid);
            }
            catch (Repository.Exceptions.AnswerNotExistException e)
            {
                throw new Exceptions.AnswerNotExistException(aid, e);
            }
            return new AnswerInformation()
            {
                Content = answer.Content,
                Creator = answer.CreaterId,
                Like = like,
                LikeNum = likeNum,
                CreateTime = answer.CreateTime,
                ModifyTime = answer.ModifyTime
            };
        }

        public async Task<TagInformation> GetTagAsync(int tid)
        {
            var tag = await tagRepostory.SelectTagByIdAsync(tid);
            if (tag == null)
            {
                throw new Exceptions.TagNotExistException(tid);
            }
            return new TagInformation()
            {
                Category = (TagCategory)tag.Category,
                Name = tag.Name,
                Desc = tag.Desc
            };
        }

        public Task<IReadOnlyDictionary<int, TagCategory>> GetTagCategoryIndexAsync()
        {
            return tagRepostory.SelectTagIndexAsync();
        }

        public async Task<Dictionary<TagCategory, IEnumerable<int>>> ClassifyTagsAsync(IEnumerable<int> tags, IReadOnlyDictionary<int, TagCategory> tagIndex = null)
        {
            tagIndex ??= await tagRepostory.SelectTagIndexAsync();
            var ret = new Dictionary<TagCategory, List<int>>(
                Enum.GetValues<TagCategory>().Select(c => new KeyValuePair<TagCategory, List<int>>(c, new List<int>())));
            foreach (var tid in tags)
            {
                if (tagIndex.TryGetValue(tid, out var category))
                {
                    ret[category].Add(tid);
                }
            }
            return new Dictionary<TagCategory, IEnumerable<int>>(ret.Select(c => new KeyValuePair<TagCategory, IEnumerable<int>>(c.Key, c.Value)));
        }

        public async Task<IEnumerable<int>> SearchQuestionAsync(string content, IEnumerable<int> tags)
        {
            var retruevalTask = nlpService.RetrievalAsync(content, 50, Enum.GetValues<NLPService.Languages>().Where(l => l != NLPService.Languages.Natrual).ToList());
            var tagIndex = await tagRepostory.SelectTagIndexAsync();
            var res = await retruevalTask;

            var questionInfos = new List<Utils.QuestionJudgement.IQuestionTagInfo>();
            foreach (var q in res)
            {
                if (q.Item1 < 0)
                {
                    continue;
                }
                var qtags = await questionRepository.SelectTagsForQuestionByIdAsync(q.Item1);
                if (qtags == null)
                {
                    continue;
                }
                var qtids = qtags.Select(t => t.Value);
                questionInfos.Add(new Utils.QuestionJudgement.QuestionTagInfo()
                {
                    Qid = q.Item1,
                    Tags = await ClassifyTagsAsync(qtids)
                });
            }
            return Utils.QuestionJudgement.GetFilteredQuestions(questionInfos, await ClassifyTagsAsync(tags));

                        
        }

        public async Task<IEnumerable<int>> GetQuestionListAsync(IEnumerable<int> tags, int? pt, int number)
        {
            if (number <= 0)
            {
                return new int[0];
            }
            int upper = pt ?? int.MaxValue;
            if (number > Constants.QuestionListMaxNumber)
            {
                number = Constants.QuestionListMaxNumber;
            }
            var questions = await questionRepository.SelectQuestionsByTagsAsync(tags, upper, number);
            return questions;
            //var res = from qid in questions
            //          where qid < upper
            //          orderby qid descending
            //          select qid;
            //var ret = new List<int>();
            //foreach (var qid in res)
            //{
            //    if (number <= 0)
            //    {
            //        break;
            //    }
            //    ret.Add(qid);
            //    number--;
            //}
            //return ret;
        }

        public Task<IReadOnlyDictionary<string, int>> GetTagListAsync()
        {
            return tagRepostory.SelectAllTagsAsync();
        }

        public async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>>> GetTagCategoryAsync()
        {
            return new Dictionary<string, IReadOnlyDictionary<string, int>>((await tagRepostory.SelectAllTagsCategorysAsync())
                .Select(kv => new KeyValuePair<string, IReadOnlyDictionary<string, int>>(
                    kv.Key.ToString(), 
                    new Dictionary<string, int>(kv.Value.Select(vkv => new KeyValuePair<string, int>(vkv.Value, vkv.Key))))));
        }

        public Task<bool> QuestionIsCodeAsync(string title, string remarks)
        {
            return Task.FromResult(Utils.QuestionJudgement.IsCode(title, remarks));
        }

        public Task<Dictionary<string, int>> GenerageTagsForQuestionAsync(string title, string remarks)
        {
            return Utils.QuestionJudgement.GenerageTagsForQuestionAsync(tagRepostory, title, remarks);
        }

        public async Task<int> AddQuestionAsync(int creater, string title, string remarks, IEnumerable<int> tags)
        {
            if (title.Length > Constants.QuestionTitleMaxLength)
            {
                throw new Exceptions.QuestionTitleTooLongException(title.Length, Constants.QuestionTitleMaxLength);
            }
            try
            {
                int qid = await questionRepository.InsertQuestionAsync(new QuestionWithListTag()
                {
                    CreaterId = creater,
                    Title = title,
                    Remarks = remarks,
                    Tags = tags
                });
                await TimedTask.NLPSynchronizer.DEFAULT.AddQuestion(qid, nlpService, title, tags);
                return qid;
            }
            catch (Repository.Exceptions.UserNotExistException e)
            {
                throw new Exceptions.UserNotExistException(creater, e);
            }
            catch (Repository.Exceptions.TagNotExistException e)
            {
                throw new Exceptions.TagNotExistException(e.TagId, e);
            }
        }

        public async Task<int> AddAnswerAsync(int creater, int qid, string content)
        {
            try
            {
                int aid = await answerRepository.InsertAnswerAsync(new AnswerInfo()
                {
                    CreaterId = creater,
                    QuestionId = qid,
                    Content = content
                });
                return aid;
            }
            catch (Repository.Exceptions.QuestionNotExistException e)
            {
                throw new Exceptions.QuestionNotExistException(qid, e);
            }
            catch (Repository.Exceptions.UserHasAnswerTheQuestionException e)
            {
                throw new Exceptions.UserHasAnswerTheQuestionException(uid: creater, qid: qid, e);
            }
            catch (Repository.Exceptions.UserNotExistException e)
            {
                throw new Exceptions.UserNotExistException(creater, e);
            }
        }

        public async Task<int> AddTagAsync(string name, string desc, string category)
        {
            if (name.Length > Constants.TagNameMaxLength)
            {
                throw new Exceptions.TagNameTooLongException(name.Length, Constants.TagNameMaxLength);
            }
            if (!Enum.TryParse<TagCategory>(category, out var tagCategory))
            {
                category = category.Substring(0, 1).ToUpper() + category.Substring(1);
                if (!Enum.TryParse(category, out tagCategory))
                {
                    throw new UnknownTagCategoryException(category);
                }
            }
            //if (tagCategory == TagCategory.None)
            //{
            //    tagCategory = TagCategory.Other;
            //}
            try
            {
                int tid = await tagRepostory.InsertTagAsync(new TagInfo()
                { 
                    Category = tagCategory,
                    Name = name,
                    Desc = desc
                });
                return tid;
            }
            catch (TagNameHasExistException e)
            {
                throw new TagNameExistException(name, e);
            }
        }

        public async Task ModifyQuestionAsync(int qid, QuestionModifyItems modifyItems)
        {
            if (modifyItems.Title != null && modifyItems.Title.Length > Constants.QuestionTitleMaxLength)
            {
                throw new Exceptions.
                    QuestionTitleTooLongException(modifyItems.Title.Length, Constants.QuestionTitleMaxLength);
            }
            try
            {
                await questionRepository.UpdateQuestionAsync(new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Title = modifyItems.Title,
                    Remarks = modifyItems.Remarks,
                    Tags = modifyItems.Tags
                });
                if (modifyItems.Title != null || modifyItems.Tags != null)
                {
                    var deleteTask = nlpService.DeleteAsync(qid);
                    var title = modifyItems.Title ?? (await questionRepository.SelectQuestionByIdAsync(qid)).Title;
                    var tags = modifyItems.Tags ?? (await questionRepository.SelectTagsForQuestionByIdAsync(qid)).Select(t => t.Value);
                    await deleteTask;
                    await TimedTask.NLPSynchronizer.DEFAULT.AddQuestion(qid, nlpService, title, tags);
                }
            }
            catch (Repository.Exceptions.TagNotExistException e)
            {
                throw new Exceptions.TagNotExistException(e.TagId, e);
            }
            catch (Repository.Exceptions.QuestionNotExistException e)
            {
                throw new Exceptions.QuestionNotExistException(qid, e);
            }
            //catch (Repository.Exceptions.AnswerNotExistException e)
            //{
            //    if (modifyItems.BestAnswer != null)
            //    {
            //        throw new Exceptions.AnswerNotExistException((int)modifyItems.BestAnswer, e);
            //    }
            //    throw;
            //}
        }

        public async Task ModifyAnswerAsync(int aid, string content)
        {
            try
            {
                await answerRepository.UpdateAnswerAsync(new AnswerInfo()
                {
                    AnswerId = aid,
                    Content = content
                });
            }
            catch (Repository.Exceptions.AnswerNotExistException e)
            {
                throw new Exceptions.AnswerNotExistException(aid, e);
            }
        }

        public async Task ModifyTagAsync(int tid, string name, string desc, string category)
        {
            if (name != null)
            {
                int max = Constants.TagNameMaxLength;
                int actual = name.Length;
                if (actual > max)
                {
                    throw new Exceptions.TagNameTooLongException(actual, max);
                }
            }
            TagCategory? tagCategory = null;// = TagCategory.None;
            if (category != null)
            {
                if (!Enum.TryParse<TagCategory>(category, out var tmp))
                {
                    tagCategory = tmp;
                    category = category.Substring(0, 1).ToUpper() + category.Substring(1);
                    if (!Enum.TryParse(category, out tmp))
                    {
                        tagCategory = tmp;
                        throw new UnknownTagCategoryException(category);
                    }
                }
                //if (tagCategory == TagCategory.None)
                //{
                //    tagCategory = TagCategory.Other;
                //}
            }
            try
            {
                await tagRepostory.UpdateTagAsync(new TagInfo()
                {
                    TagId = tid,
                    Category = tagCategory,
                    Name = name,
                    Desc = desc
                });
            }
            catch (Repository.Exceptions.TagNotExistException e)
            {
                throw new Exceptions.TagNotExistException(tid, e);
            }
            catch (Repository.Exceptions.TagNameHasExistException e)
            {
                throw new Exceptions.TagNameExistException(name, e);
            }
        }

        public async Task DeleteAnswerAsync(int aid)
        {
            var answer = await answerRepository.SelectAnswerByIdAsync(aid);
            if (answer == null)
            {
                throw new Exceptions.AnswerNotExistException(aid);
            }
            await answerRepository.DeleteAnswerByIdAsync(aid);
        }

        public async Task DeleteQuestionAsync(int qid)
        {
            var question = await questionRepository.SelectQuestionByIdAsync(qid);
            if (question == null)
            {
                throw new Exceptions.QuestionNotExistException(qid);
            }
            await questionRepository.DeleteQuestionByIdAsync(qid);
            await nlpService.DeleteAsync(qid);
        }

        public async Task DeleteTagAsync(int tid)
        {
            var tag = await tagRepostory.SelectTagByIdAsync(tid);
            if (tag == null)
            {
                throw new Exceptions.TagNotExistException(tid);
            }
            await tagRepostory.DeleteTagAsync(tid);
        }

        public async Task<LikeProduceResult> LikeQuestionAsync(int uid, int qid, bool target)
        {
            var ret = new LikeProduceResult();
            if (target)
            {
                try
                {
                    await likeRepository.InsertLikeForQuestionAsync(uid, qid);
                    ret.Status = LikeProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.QuestionNotExistException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.questionNotExist;
                }
                catch (Repository.Exceptions.UserHasLikedTargetException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.alreadyLiked;
                }
            }
            else
            {
                try
                {
                    await likeRepository.DeleteLikeForQuestionAsync(uid, qid);
                    ret.Status = LikeProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.QuestionNotExistException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.questionNotExist;
                }
                catch (Repository.Exceptions.UserNotLikedTargetException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.notLiked;
                }
            }
            ret.UserLiked = await likeRepository.UserLikedQuestionAsync(uid, qid);
            ret.LikeNum = await likeRepository.SelectLikesCountForQuestionAsync(qid);
            return ret;
        }

        public async Task<LikeProduceResult> LikeAnswerAsync(int uid, int aid, bool target)
        {
            var ret = new LikeProduceResult();
            if (target)
            {
                try
                {
                    await likeRepository.InsertLikeForAnswerAsync(uid, aid);
                    ret.Status = LikeProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.QuestionNotExistException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.answerNotExist;
                }
                catch (Repository.Exceptions.UserHasLikedTargetException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.alreadyLiked;
                }
            }
            else
            {
                try
                {
                    await likeRepository.DeleteLikeFroAnswerAsync(uid, aid);
                    ret.Status = LikeProduceResult.ResultStatus.success;
                }
                catch (Repository.Exceptions.AnswerNotExistException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.answerNotExist;
                }
                catch (Repository.Exceptions.UserNotLikedTargetException)
                {
                    ret.Status = LikeProduceResult.ResultStatus.notLiked;
                }
            }
            ret.UserLiked = await likeRepository.UserLikedAnswerAsync(uid, aid);
            ret.LikeNum = await likeRepository.SelectLikesCountForAnswerAsync(aid);
            return ret;
        }
    }
}
