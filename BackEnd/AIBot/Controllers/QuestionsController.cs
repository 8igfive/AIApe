using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using Buaa.AIBot.Services;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Controllers.Exceptions;

namespace Buaa.AIBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IQuestionService questionService;

        private readonly IUserService userService;

        private readonly IHotListService hotListService;

        public QuestionsController(IQuestionService questionService, IUserService userService, IHotListService hotListService)
        {
            this.questionService = questionService;
            this.userService = userService;
            this.hotListService = hotListService;
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("like_answer")]
        public async Task<IActionResult> LikeAnswerAsync(LikeAnswerBody body)
        {
            int uid = userService.GetUidFromToken(Request);
            var ret = await questionService.LikeAnswerAsync(uid, body.Aid, body.MarkAsLike);
            string message;
            string mark = body.MarkAsLike ? "mark" : "unmark";
            switch (ret.Status)
            {
                case LikeProduceResult.ResultStatus.success:
                    message = $"success to {mark} answer as like.";
                    break;
                case LikeProduceResult.ResultStatus.alreadyLiked:
                    message = $"fail to mark answer as like because user has already like the answer.";
                    break;
                case LikeProduceResult.ResultStatus.notLiked:
                    message = $"fail to unmark answer as like because user has not liked the answer yet.";
                    break;
                case LikeProduceResult.ResultStatus.answerNotExist:
                    message = $"fail to {mark} answer as like because the answer not exists.";
                    break;
                default:
                    message = $"unknown error: {ret.Status}.";
                    break;
            }
            return Ok(new
            {
                Status = ret.Status.ToString(),
                Message = message,
                Like = ret.UserLiked,
                LikeNum = ret.LikeNum
            });
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("like_question")]
        public async Task<IActionResult> LikeQuestionAsync(LikeQuestionBody body)
        {
            int uid = userService.GetUidFromToken(Request);
            var ret = await questionService.LikeQuestionAsync(uid, body.Qid, body.MarkAsLike);
            string message;
            string mark = body.MarkAsLike ? "mark" : "unmark";
            switch (ret.Status)
            {
                case LikeProduceResult.ResultStatus.success:
                    message = $"success to {mark} question as like.";
                    break;
                case LikeProduceResult.ResultStatus.alreadyLiked:
                    message = $"fail to mark question as like because user has already like the question.";
                    break;
                case LikeProduceResult.ResultStatus.notLiked:
                    message = $"fail to unmark question as like because user has not liked the question yet.";
                    break;
                case LikeProduceResult.ResultStatus.answerNotExist:
                    message = $"fail to {mark} question as like because the answer not exists.";
                    break;
                default:
                    message = $"unknown error: {ret.Status}.";
                    break;
            }
            return Ok(new
            {
                Status = ret.Status.ToString(),
                Message = message,
                Like = ret.UserLiked,
                LikeNum = ret.LikeNum
            });
        }

        [AllowAnonymous]
        [HttpGet("question")]
        public async Task<IActionResult> QuestionAsync()
        {
            int qid = int.Parse(Request.Query["qid"]);
            int? uid = userService.TryGetUidFromToken(Request, out int res) ? res : null;
            try
            {
                QuestionInformation question =  await questionService.GetQuestionAsync(qid, uid);
                return Ok(new
                {
                    Status = "success",
                    Message = $"get question with qid={qid} successfully",
                    Question = question
                });
            } catch (QuestionNotExistException) {
                return NotFound(new 
                {
                    Status = "questionNotExist",
                    Message = $"question with qid={qid} dose not exist",
                    QuestionAsync= new QuestionInformation()
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("answer")]
        public async Task<IActionResult> AnswerAsync()
        {
            int aid = int.Parse(Request.Query["aid"]);
            int? uid = userService.TryGetUidFromToken(Request, out int res) ? res : null;
            try
            {
                AnswerInformation answer = await questionService.GetAnswerAsync(aid, uid);
                return Ok(new
                {
                    Status = "success",
                    Message = $"get answer with aid={aid} successfully",
                    Answer = answer
                });
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = $"answer with aid={aid} dose not exist",
                    Answer = new AnswerInformation()
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("tag")]
        public async Task<IActionResult> TagAsync()
        {
            int tid = int.Parse(Request.Query["tid"]);
            try
            {
                TagInformation tag = await questionService.GetTagAsync(tid);
                return Ok(new
                {
                    Status = "success",
                    Message = $"get tag with tid={tid} successfully",
                    Tag = tag
                });
            } catch (TagNotExistException) {
                return NotFound(new
                {
                    Status = "tagNotExist",
                    Message = $"tag with tid={tid} dose not exist",
                    Tag = new TagInformation()
                });
            }
        }

        [AllowAnonymous]
        [HttpPost("search")]
        public async Task<IActionResult> SearchAsync(QuestionBody body)
        {
            IEnumerable<int> tags = body.Tags ?? new int[0];
            string content = body.Content ?? "";
            return Ok(await questionService.SearchQuestionAsync(content, tags));
        }

        [AllowAnonymous]
        [HttpGet("hotlist")]
        public async Task<IActionResult> HotlistAsync()
        {
            return Ok(await hotListService.GetHotListAsync());
        }

        [AllowAnonymous]
        [HttpPost("questionlist")]
        public async Task<IActionResult> QuestionlistAsync(QuestionBody body)
        {
            IEnumerable<int> tags = (body.Tags == null)? new int[0] : body.Tags;
            int? pt = body.Pt;
            int number = body.Number.GetValueOrDefault(Constants.QuestionListMaxNumber);
            return Ok(await questionService.GetQuestionListAsync(tags, pt, number));
        }

        [AllowAnonymous]
        [HttpGet("taglist")]
        public async Task<IActionResult> TaglistAsync()
        {
            return Ok(await questionService.GetTagListAsync());
        }


        [AllowAnonymous]
        [HttpGet("tagcategory")]
        public async Task<IActionResult> TagCategooryAsync()
        {
            return Ok(await questionService.GetTagCategoryAsync());
        }


        [AllowAnonymous]
        [HttpPost("auto_tag")]
        public async Task<IActionResult> AutoTagAsync(QuestionBody body)
        {
            string title = (body.Title == null) ? "" : body.Title;
            string remarks = (body.Remarks == null) ? "" : body.Remarks;
            var res = await questionService.GenerageTagsForQuestionAsync(title, remarks);
            return Ok(new
            {
                Status = "success",
                Message = "code judgement success",
                Tags = res
            });
        }


        [AllowAnonymous]
        [HttpPost("is_code")]
        public async Task<IActionResult> IsCodeAsync(QuestionBody body)
        {
            string title = (body.Title == null) ? "" : body.Title;
            string remarks = (body.Remarks == null) ? "" : body.Remarks;
            var res = await questionService.QuestionIsCodeAsync(title, remarks);
            return Ok(new
            {
                Status = "success",
                Message = "code judgement success",
                Result = res
            });
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("add_question")]
        public async Task<IActionResult> AddQuestionAsync(QuestionBody body)
        {
            int creater = userService.GetUidFromToken(Request);
            string title = (body.Title == null)? "" : body.Title;
            string remarks = (body.Remarks == null)? "" : body.Remarks;
            IEnumerable<int> tags = (body.Tags == null)? new int[0] : body.Tags;
            try
            {
                int qid = await questionService.AddQuestionAsync(creater, title, remarks, tags);
                return Ok(new
                {
                    Status = "success",
                    Message = "new question add successfully",
                    Qid = qid
                });
            } catch (QuestionTitleTooLongException) {
                return Ok(new
                {
                    Status = "questionTooLong",
                    Message = "describtion of question is too long"
                });
            } catch (UserNotExistException) {
                return Ok(new
                {
                    Status = "userNotExist",
                    Message = "adding question fail for user problem"
                });
            } catch (TagNotExistException) {
                return Ok(new
                {
                    Status = "tagNotExist",
                    Message = "can not resolve tags"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("add_answer")]
        public async Task<IActionResult> AddAnswerAsync(QuestionBody body)
        {
            int creater = userService.GetUidFromToken(Request);
            int qid = body.Qid.GetValueOrDefault(-1);
            string content = (body.Content == null) ? "" : body.Content;
            try
            {
                int aid = await questionService.AddAnswerAsync(creater, qid, content);
                return Ok(new
                {
                    Status = "success",
                    Message = "new answer add successfully",
                    Aid = aid
                });
            } catch (UserHasAnswerTheQuestionException) {
                return Ok(new
                {
                    Status = "answerHasExist",
                    Message = "user has answered this question"
                });
            } catch (UserNotExistException) {
                return Ok(new
                {
                    Status = "userNotExist",
                    Message = "adding answer fail for user problem"
                });
            } catch(QuestionNotExistException) {
                return Ok(new
                {
                    Status = "questionNotExist",
                    Message = "add answer fail because of missing question"
                });
            } // TODO: question/answer too long or too short
        }

        [Authorize(Policy = "Admin")]
        [HttpPost("add_tag")]
        public async Task<IActionResult> AddTagAsync(QuestionBody body)
        {
            string name = (body.Name == null)? "" : body.Name;
            string desc = (body.Desc == null)? "" : body.Desc;
            string category = (body.Category == null) ? "" : body.Category;
            try
            {
                int tid = await questionService.AddTagAsync(name, desc, category);
                return Ok(new
                {
                    Status = "success",
                    Message = "new tag add successfully",
                    Tid = tid
                });
            } catch (TagNameTooLongException) {
                return Ok(new
                {
                    Status = "nameTooLong",
                    Message = "name of tag is too long"
                });
            } catch (TagNameExistException) {
                return Ok(new
                {
                    Status = "nameHasExists",
                    Message = "tag with this name has already existed"
                });
            } catch (UnknownTagCategoryException)
            {
                return Ok(new
                {
                    Status = "unknownCategory",
                    Message = $"no tag-category named {category}"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPut("modify_question")]
        public async Task<IActionResult> ModifyQuestionAsync(QuestionBody body)
        {
            int qid = body.Qid.GetValueOrDefault(-1);
            try
            {
                await checkQidAsync(qid);
            } catch (QuestionNotExistException) {
                return NotFound(new
                {
                    Status = "questionNotExist",
                    Message = $"question with qid={qid} dose not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "your authority is not enough"
                });
            }

            string question = (body.Question == null)? "" : body.Question;
            string remarks = (body.Remarks == null)? "" : body.Remarks;
            int? bestAnswer = body.BestAnswer;
            IEnumerable<int> tags = (body.Tags == null)? new int[0] : body.Tags;

            try
            {
                await questionService.ModifyQuestionAsync(qid, new QuestionModifyItems
                {
                    Title = question,
                    Remarks = remarks,
                    BestAnswer = bestAnswer,
                    Tags = tags
                });
                return Ok(new
                {
                    Status = "success",
                    Message = "question modification successfully"
                });
            } catch (QuestionNotExistException) {
                return NotFound(new
                {
                    Status = "questionNotExist",
                    Message = $"question with qid={qid} dose not exist"
                });
            } catch (QuestionTitleTooLongException) {
                return Ok(new
                {
                    Status = "questionTitleTooLong",
                    Message = "new title is too long"
                });
            } catch (TagNotExistException) {
                return NotFound(new
                {
                    Status = "tagNotExist",
                    Message = "tags include illegal tag(s)"
                });
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = "there is no specific best answer"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPut("modify_answer")]
        public async Task<IActionResult> ModifyAnswerAsync(QuestionBody body)
        {
            int aid = body.Aid.GetValueOrDefault(-1);
            try
            {
                await checkAidAsync(aid);
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = $"answer with aid={aid} dose not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "unauthorized",
                    Message = "your authority is not enough"
                });
            }
            
            string content = (body.Content == null)? "" : body.Content;
            try
            {
                await questionService.ModifyAnswerAsync(aid, content);
                return Ok(new
                {
                    Status = "success",
                    Message = "answer modification successfully"
                });
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = $"answer with aid={aid} dose not exist"
                });
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpPut("modify_tag")]
        public async Task<IActionResult> ModifyTagAsync(QuestionBody body)
        {
            int tid = body.Tid.GetValueOrDefault(-1);
            string name = (body.Name == null)? "" : body.Name;
            string desc = (body.Desc == null)? "" : body.Desc;
            string category = (body.Category == null) ? "" : body.Category;
            try
            {
                await questionService.ModifyTagAsync(tid, name, desc, category);
                return Ok(new
                {
                    Status = "success",
                    Message = "tag modification success"
                });
            } catch (TagNotExistException) {
                return NotFound(new
                {
                    Status = "tagNotExist",
                    Message = $"tag with tid={tid} dose not exist"
                });
            } catch (TagNameTooLongException) {
                return Ok(new
                {
                    Status = "nameTooLong",
                    Message = "new name is too long"
                });
            } catch (TagNameExistException) {
                return Ok(new
                {
                    Status = "nameHasExist",
                    Message = "name has already been used"
                });
            } catch (UnknownTagCategoryException)
            {
                return Ok(new
                {
                    Status = "unknownCategory",
                    Message = $"no tag-category named {category}"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpDelete("delete_question")]
        public async Task<IActionResult> DeleteQuestionAsync(QuestionBody body)
        {
            int qid = body.Qid.GetValueOrDefault(-1);
            try
            {
                await checkQidAsync(qid);
                await hotListService.FreshHotListAsync();
            } catch (QuestionNotExistException) {
                return NotFound(new
                {
                    Status = "questionNotExist",
                    Message = $"question with qid={qid} dose not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "your authority is not enough"
                });
            }
            try
            {
                await questionService.DeleteQuestionAsync(qid);
                return Ok(new
                {
                    Status = "success",
                    Message = "question removed successfully"
                });
            } catch (QuestionNotExistException) {
                return NotFound(new
                {
                    Status = "questionNotExist",
                    Message = $"question with qid={qid} dose not exist"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpDelete("delete_answer")]
        public async Task<IActionResult> DeleteAnswerAsync(QuestionBody body)
        {
            int aid = body.Aid.GetValueOrDefault(-1);
            try
            {
                await checkAidAsync(aid);
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = $"answer with aid={aid} dose not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "unauthorized",
                    Message = "your authority is not enough"
                });
            }
            try
            {
                await questionService.DeleteAnswerAsync(aid);
                return Ok(new
                {
                    Status = "success",
                    Message = "answer removed"
                });
            } catch (AnswerNotExistException) {
                return NotFound(new
                {
                    Status = "answerNotExist",
                    Message = $"answer with aid={aid} dose not exist"
                });
            }
        }

        [Authorize(Policy = "Admin")]
        [HttpDelete("delete_tag")]
        public async Task<IActionResult> DeleteTagAsync(QuestionBody body)
        {
            int tid = body.Tid.GetValueOrDefault(-1);
            try
            {
                await questionService.DeleteTagAsync(tid);
                return Ok(new
                {
                    Status = "success",
                    Message = "tag removed"
                });
            } catch (TagNotExistException) {
                return NotFound(new
                {
                    Status = "tagNotExist",
                    Message = $"tag with tid={tid} dose not exist"
                });
            }
        }

        private async Task checkQidAsync(int qid)
        {
            QuestionInformation qInfo = await questionService.GetQuestionAsync(qid);
            int creator = qInfo.Creator.GetValueOrDefault(-1);
            int uid = userService.GetUidFromToken(Request);
            if (creator != uid)
            {
                AuthLevel auth = userService.GetAuthLevelFromToken(Request);
                if (auth != AuthLevel.Admin)
                {
                    throw new LackofAuthorityException(auth);
                }
            }
        }

        private async Task checkAidAsync(int aid)
        {
            AnswerInformation aInfo = await questionService.GetAnswerAsync(aid);
            int creator = aInfo.Creator.GetValueOrDefault(-1);
            int uid = userService.GetUidFromToken(Request);
            if(creator != uid)
            {
                AuthLevel auth = userService.GetAuthLevelFromToken(Request);
                if(auth != AuthLevel.Admin)
                {
                    throw new LackofAuthorityException(auth);
                }
            }
        }
    }
}
