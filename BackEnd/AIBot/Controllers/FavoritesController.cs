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
    public class FavoritesController : ControllerBase
    {
        private readonly IFavoriteService favoriteService;

        private readonly IUserService userService;

        public FavoritesController(IFavoriteService favoriteService, IUserService userService)
        {
            this.favoriteService = favoriteService;
            this.userService = userService;
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("add_favorite")]
        public async Task<IActionResult> AddFavoriteAsync(FavoriteBody body)
        {
            int creator = userService.GetUidFromToken(Request);
            string name = (body.Name == null)? "" : body.Name;
            string description = (body.Description == null)? "" : body.Description;
            try
            {
                int fid = await favoriteService.AddFavoriteAsync(creator, name, description);
                return Ok(new
                {
                    Status = "success",
                    Message = "new favorite add successfully"
                });
            } catch (FavoriteNameTooLongException) {
                return Ok(new
                {
                    Status = "favoriteTooLong",
                    Message = "name of question is too long"
                });
            } catch (UserNotExistException) {
                return Ok(new
                {
                    Status = "userNotExist",
                    Message = "adding favorite fail for user problem"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpDelete("delete_favorite")]
        public async Task<IActionResult> DeleteFavoriteAsync(FavoriteBody body)
        {
            int fid = body.Fid.GetValueOrDefault(-1);
            try
            {
                await checkFidAsync(fid);
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} does not exist"
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
                await favoriteService.DeleteFavoriteAsync(fid);
                return Ok(new
                {
                    Status = "success",
                    Message = "favorite removed successfully"
                });
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} dose not exist"
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPut("modify_favorite")]
        public async Task<IActionResult> ModifyFavoriteAsync(FavoriteBody body)
        {
            int fid = body.Fid.GetValueOrDefault(-1);
            try
            {
                await checkFidAsync(fid);
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} dose not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "your authority is not enough"
                });
            }

            string name = body.Name;
            string description  = body.Description;

            try
            {
                await favoriteService.ModifyFavoriteAsync(fid, new FavoriteModifyItems
                {
                    Name = name,
                    Description = description
                });
                return Ok(new
                {
                    Status = "success",
                    Message = "favorite modification successfully"
                });
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} dose not exist"
                });
            } catch (FavoriteNameTooLongException) {
                return Ok(new
                {
                    Status = "favoriteNameTooLong",
                    Message = "new name is too long"
                });
            }
        }

        [AllowAnonymous]
        [HttpGet("favorite")]
        public async Task<IActionResult> FavoriteAsync()
        {
            int fid = int.Parse(Request.Query["fid"]);
            try
            {
                FavoriteInformation favorite = await favoriteService.GetFavoriteAsync(fid);
                return Ok(new
                {
                    Status = "success",
                    Message = $"get favorite with fid={fid} successfully",
                    Favorite = favorite
                });
            } catch (FavoriteNotExistException) {
                return NotFound(new 
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} dose not exist",
                    Favorite = new FavoriteInformation()
                });
            }
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpPost("collect_question")]
        public async Task<IActionResult> CollecQuestionAsync(FavoriteBody body)
        {
            int uid = userService.GetUidFromToken(Request);
            int fid = body.Fid.GetValueOrDefault(-1);
            int qid = body.Qid.GetValueOrDefault(-1);
            bool markAsCollected = body.MarkAsFavorite.GetValueOrDefault(false);
            try
            {
                await checkFidAsync(fid);
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} does not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "your authority is not enough"
                });
            }

            var ret = await favoriteService.CollectQuestionAsync(uid, fid, qid, markAsCollected);
            string message;
            string mark = body.MarkAsFavorite.GetValueOrDefault(false) ? "mark" : "unmark";
            switch (ret.Status)
            {
                case CollectProduceResult.ResultStatus.success:
                    message = $"success to {mark} question as favorite.";
                    break;
                case CollectProduceResult.ResultStatus.alreadyCollected:
                    message = $"fail to mark question as favorite because user has already collected the question.";
                    break;
                case CollectProduceResult.ResultStatus.notCollected:
                    message = $"fail to unmark question as favorite because user has not collected the question yet.";
                    break;
                case CollectProduceResult.ResultStatus.questionNotExist:
                    message = $"fail to {mark} question as favorite because the answer not exists.";
                    break;
                default:
                    message = $"unknown error: {ret.Status}.";
                    break;
            }
            return Ok(new
            {
                Status = ret.Status.ToString(),
                Message = message,
                Collected = ret.Collected,
                CollectNum = ret.CollectNum
            });
        }
        
        [Authorize(Policy = "UserAdmin")]
        [HttpPost("collect_answer")]
        public async Task<IActionResult> CollecAnswerAsync(FavoriteBody body)
        { 
            int uid = userService.GetUidFromToken(Request);
            int fid = body.Fid.GetValueOrDefault(-1);
            int aid = body.Aid.GetValueOrDefault(-1);
            bool markAsCollected = body.MarkAsFavorite.GetValueOrDefault(false);
            try
            {
                await checkFidAsync(fid);
            } catch (FavoriteNotExistException) {
                return NotFound(new
                {
                    Status = "favoriteNotExist",
                    Message = $"favorite with fid={fid} does not exist"
                });
            } catch (LackofAuthorityException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "your authority is not enough"
                });
            }
            var ret = await favoriteService.CollectAnswerAsync(uid, fid, aid, markAsCollected);
            string message;
            string mark = body.MarkAsFavorite.GetValueOrDefault(false) ? "mark" : "unmark";
            switch (ret.Status)
            {
                case CollectProduceResult.ResultStatus.success:
                    message = $"success to {mark} answer as favorite.";
                    break;
                case CollectProduceResult.ResultStatus.alreadyCollected:
                    message = $"fail to mark answer as favorite because user has already collected the answer.";
                    break;
                case CollectProduceResult.ResultStatus.notCollected:
                    message = $"fail to unmark answer as favorite because user has not collected the answer yet.";
                    break;
                case CollectProduceResult.ResultStatus.answerNotExist:
                    message = $"fail to {mark} answer as favorite because the answer not exists.";
                    break;
                default:
                    message = $"unknown error: {ret.Status}.";
                    break;
            }
            return Ok(new
            {
                Status = ret.Status.ToString(),
                Message = message,
                Collected = ret.Collected,
                CollectNum = ret.CollectNum
            });
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpGet("question_in_favorite")]
        public async Task<IActionResult> QuestionInFavoriteAsync()
        {
            int fid = int.Parse(Request.Query["fid"]);
            int qid = int.Parse(Request.Query["qid"]);
            string status;
            string message;
            bool collected;
            try
            {
                collected = await favoriteService.QuestionInFavoriteAsync(fid, qid);
                return Ok(new 
                {
                    Status = "success",
                    Message = "check whether quesiton is in favorite successfully",
                    Collected = collected
                });
            }
            catch (FavoriteNotExistException)
            {
                collected = false;
                status = "fail";
                message = $"favorite with fid={fid} not exist";
            }
            catch (QuestionNotExistException)
            {
                collected = false;
                status = "fail";
                message = $"question with qid={qid} not exist";
            }
            return NotFound(new 
            {
                Status = status,
                Message = message,
                Collected = collected
            });
        }

        [Authorize(Policy = "UserAdmin")]
        [HttpGet("answer_in_favorite")]
        public async Task<IActionResult> AnswerInFavoriteAsync()
        {
            int fid = int.Parse(Request.Query["fid"]);
            int aid = int.Parse(Request.Query["aid"]);
            string status;
            string message;
            bool collected;
            try
            {
                collected = await favoriteService.AnswerInFavoriteAsync(fid, aid);
                return Ok(new 
                {
                    Status = "success",
                    Message = "check whether answer is in favorite successfully",
                    Collected = collected
                });
            }
            catch (FavoriteNotExistException)
            {
                collected = false;
                status = "fail";
                message = $"favorite with fid={fid} not exist";
            }
            catch (AnswerNotExistException)
            {
                collected = false;
                status = "fail";
                message = $"answer with aid={aid} not exist";
            }
            return NotFound(new 
            {
                Status = status,
                Message = message,
                Collected = collected
            });
        }

        private async Task checkFidAsync(int fid)
        {
            FavoriteInformation fInfo = await favoriteService.GetFavoriteAsync(fid);
            int creator = fInfo.Creator;
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
    }
}
