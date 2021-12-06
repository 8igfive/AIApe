using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Buaa.AIBot.Services;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Exceptions;

using BNBCrypt = BCrypt.Net.BCrypt;

namespace Buaa.AIBot.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;
        
        private readonly IUserRepository userRepository;

        private readonly TokenManagement tokenManagement;

        private static readonly Dictionary<AuthLevel, int> intAuth = new Dictionary<AuthLevel, int>
        {
            {AuthLevel.None, 0},
            {AuthLevel.User, 1},
            {AuthLevel.Admin, 2}
        };

        private static readonly Dictionary<string, int> intAuthFromString = new Dictionary<string, int>
        {
            {"Traveler", 0},
            {"User", 1},
            {"Administrator", 2}
        };


        private static readonly Dictionary<int, AuthLevel> AuthLevelFromInt = new Dictionary<int, AuthLevel>
        {
            {0, AuthLevel.None},
            {1, AuthLevel.User},
            {2, AuthLevel.Admin}
        };

        public UserController(IUserService userService, IUserRepository userRepository,
                              IOptions<TokenManagement> tokenManagement)
        {
            this.userService = userService;
            this.userRepository = userRepository;
            this.tokenManagement = tokenManagement.Value;
        }

        /// <summmary>
        /// User sign up.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("signup")]
        public async Task<IActionResult> SignUpAsync(UserBody body)
        {
            body = new UserBody
            {
                Name = (body.Name == null)? "" : body.Name,
                Email = (body.Email == null)? "" : body.Email,
                Password = (body.Password == null) ? "" : body.Password,
                ProfilePhoto = body.ProfilePhoto ?? 1,
            };
            StatusMessageResponse response;
            if (!userService.CheckSignUpBody(body, out response))
            {
                return Ok(response);
            }
            try{
                UserInfo newUser = new UserInfo
                {
                    Email = body.Email,
                    Name = body.Name,
                    Bcrypt = BNBCrypt.HashPassword(body.Password),
                    Auth = body.Name.Equals("root")? AuthLevel.Admin : AuthLevel.User,
                    ProfilePhoto = body.ProfilePhoto
                };
                await userRepository.InsertUserAsync(newUser);
                return Ok(new StatusMessageResponse 
                {
                    Status = "success",
                    Message = "signup success"
                });
            } catch (EmailHasExistException) {
                return Ok(new StatusMessageResponse
                {
                    Status = "emailExisted",
                    Message = "email has been used"
                });
            } catch (NameHasExistException) {
                return Ok(new StatusMessageResponse
                {
                    Status = "nameExisted",
                    Message = "name has been used"
                });
            }
        }

        /// <summary>
        /// User login.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(UserBody body)
        {
            body = new UserBody
            {
                Email = (body.Email == null)? "" : body.Email,
                Password = (body.Password == null) ? "" : body.Password
            };
            string[] tokenAndAuth = await userService.AuthorizeAccountAsync(body);
            if (tokenAndAuth[0].Equals(string.Empty))
            {
                return Ok(new 
                {
                    Status = "fail",
                    Message = "wrong email or password",
                    Token = "",
                    Auth = intAuth[AuthLevel.None],
                    Timeout = 0
                });
            }  
            return Ok(new 
            {
                Status = "success",
                Message = "login success",
                Token = tokenAndAuth[0],
                Auth = intAuthFromString[tokenAndAuth[1]],
                Timeout = tokenManagement.AccessExpiration * 60
            });
        }

        /// <summary>
        /// Get public infomation.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("public_info")]
        public async Task<IActionResult> GetPublicInfoAsync()
        {
            int uid;
            try
            {
                uid = int.Parse(HttpContext.Request.Query["uid"]);
            } catch (ArgumentNullException) {
                return NotFound(new 
                {
                    Status = "userNotExist",
                    Name = "UNKNOWN",
                    ProfilePhoto = 0
                });
            }
            UserInfo userInfo = await userRepository.SelectUserByIdAsync(uid);
            if (userInfo == null)
            {
                return NotFound(new 
                {
                    Status = "userNotExist",
                    Name = "UNKNOWN",
                    ProfilePhoto = 0
                });
            }
            return Ok(new 
            {
                Status = "success",
                Name = userInfo.Name,
                ProfilePhoto = userInfo.ProfilePhoto
            });
        }

        /// <summary>
        /// Get internal infomation.
        /// </summary>
        [Authorize(Policy = "UserAdmin")]
        [HttpGet("internal_info")]
        public async Task<IActionResult> GetInternalInfoAsync()
        {
            int uid;
            uid = userService.GetUidFromParameters(Request);
            uid = (uid == -1)? userService.GetUidFromToken(Request) : uid;
            UserInfo userInfo = await userRepository.SelectUserByIdAsync(uid);
            if (userInfo == null)
            {
                return NotFound(new 
                {
                    Status = "userNotExist",
                    Uid = -1,
                    name = "UNKNOWN",
                    Email = "N@N",
                    ProfilePhoto = 0
                });
            } else {
                return Ok(new 
                {
                    Status = "success",
                    Uid = userInfo.UserId,
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    ProfilePhoto = userInfo.ProfilePhoto
                });
            }
        }

        /// <summary>
        /// Get full infomation.
        /// </summary>
        [Authorize(Policy = "UserAdmin")]
        [HttpGet("full_info")]
        public async Task<IActionResult> GetFullInfoAsync()
        {
            int presentUid = userService.GetUidFromToken(Request);
            int uid;
            uid = userService.GetUidFromParameters(Request);
            uid = (uid == -1)? presentUid : uid;
            if(uid != presentUid)
            {
                AuthLevel presentAuth = userService.GetAuthLevelFromToken(Request);
                if (presentAuth != AuthLevel.Admin)
                {
                    return Unauthorized(new 
                    {
                        Status = "userNotExist",
                        Uid = -1,
                        Name = "",
                        Email = "",
                        Auth = intAuth[presentAuth],
                        ProfilePhoto = 0
                    });
                }
            }
            UserInfo userInfo = await userRepository.SelectUserByIdAsync(uid);
            if (userInfo == null)
            {
                return NotFound(new 
                {
                    Status = "userNotExist",
                    Uid = uid,
                    Name = "",
                    Email = "",
                    Auth = intAuth[AuthLevel.None],
                    ProfilePhoto = 0
                });
            } else {
                return Ok(new 
                {
                    Status = "success",
                    Uid = userInfo.UserId,
                    Name = userInfo.Name,
                    Email = userInfo.Email,
                    Auth = intAuth[userInfo.Auth],
                    ProfilePhoto = userInfo.ProfilePhoto
                });
            }
        }

        /// <summary>
        /// Modify full information.
        /// </summary>
        [Authorize(Policy = "UserAdmin")]
        [HttpPut("modify")]
        public async Task<IActionResult> ModifyAsync(UserBody body)
        {
            int presentUid = userService.GetUidFromToken(Request);
            AuthLevel presentAuth = AuthLevel.None;
            int uid = body.Uid.GetValueOrDefault(-1);
            if (uid <= 0)
            {
                uid = presentUid;
            }
            if (uid != presentUid)
            {
                presentAuth = userService.GetAuthLevelFromToken(Request);
                if (presentAuth != AuthLevel.Admin)
                {
                    return Unauthorized(new 
                    {
                        Status = "fail",
                        Message = "lack of authority"
                    });
                }
            }
            var modifyInfo = new UserInfo()
            {
                UserId = uid
            };
            UserInfo userInfo = await userRepository.SelectUserByIdAsync(uid);
            if (userInfo == null)
            {
                return Conflict(new
                {
                    Status = "userNotExist",
                    Message = "user dose not exist"
                });
            }
            if (body.Name != null)
            {
                if (!userService.CheckName(body.Name))
                {
                    return Conflict(new 
                    {
                        Status = "nameInvalid",
                        Message = "either contains illegal characters or length of name is too long or too short"
                    });
                }
                modifyInfo.Name = body.Name;
            }
            if (body.Password != null)
            {
                if (!userService.CheckPassword(body.Password))
                {
                    return Conflict(new
                    {
                        Status = "passwordInvalid",
                        Message = "either contains illegal characters or length of password is too long or too short"
                    });
                }
                if (BNBCrypt.Verify(body.Password, userInfo.Bcrypt))
                {
                    return Conflict(new
                    {
                        Status = "passwordSame",
                        Message = "new password can not be the same as original one"
                    });
                }
                modifyInfo.Bcrypt = BNBCrypt.HashPassword(body.Password);
            }
            if (body.Auth != null)
            {
                if (presentAuth == AuthLevel.None)
                {
                    presentAuth = userService.GetAuthLevelFromToken(Request);
                }
                if (presentAuth != AuthLevel.Admin)
                {
                    return Unauthorized(new
                    {
                        Status = "fail",
                        Message = "lack of authority"
                    });
                }
                if (uid == presentUid)
                {
                    return Conflict(new
                    {
                        Status = "tryChangeSelfAuth",
                        Message = "try to change authority of current user"
                    });
                }
                if (body.Auth < 1)
                {
                    body.Auth = 1;
                }
                modifyInfo.Auth = AuthLevelFromInt[body.Auth.GetValueOrDefault(0)];
            }
            else
            {
                modifyInfo.Auth = AuthLevel.None;
            }
            if (body.ProfilePhoto != null)
            {
                modifyInfo.ProfilePhoto = body.ProfilePhoto;
            }
            try
            {
                await userRepository.UpdateUserAsync(modifyInfo);
            } catch (NameHasExistException) {
                return Conflict(new
                {
                    Status = "nameExisted",
                    Message = "name has been used by other user"
                });
            }

            return Ok(new
            {
                Status = "success",
                Message = "modify successfully"
            });
        }

        /// <summary>
        /// List questions of a specific user.
        /// </summary>
        [Authorize(Policy = "UserAdmin")]
        [HttpGet("questions")]
        public async Task<IActionResult> QuestionsAsync()
        {
            int uid;
            uid = userService.GetUidFromParameters(Request);
            uid = (uid == -1)? userService.GetUidFromToken(Request) : uid;
            IEnumerable<int> qids = await userRepository.SelectQuestionsIdByIdOrderByModifyTimeAsync(uid);
            if (qids == null)
            {
                return NotFound(new
                {
                    Status = "userNotExist",
                    Message = "user dose not exist",
                    Questions = new int[0]
                });
            } else {
                return Ok(new
                {
                    Status = "success",
                    Message = $"get questions for user{uid} successfully",
                    Questions = qids
                });
            }
        }

        /// <summary>
        /// List questions of a specific user.
        /// </summary>
        [Authorize(Policy = "UserAdmin")]
        [HttpGet("answers")]
        public async Task<IActionResult> AnswersAsync()
        {
            int uid;
            uid = userService.GetUidFromParameters(Request);
            uid = (uid == -1)? userService.GetUidFromToken(Request) : uid;
            var aids = await userRepository.SelectAnswersIdByIdByModifyTimeAsync(uid);
            if (aids == null)
            {
                return NotFound(new
                {
                    Status = "userNotExist",
                    Message = "user dose not exist",
                    Answers = new object[0]
                });
            } else {
                return Ok(new
                {
                    Status = "success",
                    Message = $"get answers for user{uid} successfully",
                    Answers = aids.Select(a => new 
                    {
                        Qid = a.QuestionId,
                        Aid = a.AnswerId
                    })
                });
            }
        }

        /// <summary>
        /// Fresh token, leave alone if the user has changed his password.
        /// </summary>
        [AllowAnonymous]
        [HttpPost("fresh")]
        public async Task<IActionResult> FreshAsync(UserBody body)
        {
            DateTime expiration;
            try
            {
                expiration = userService.GetExpirationFromToken(body);
                if (expiration.CompareTo(DateTime.Now) < 0)
                {
                    return Unauthorized(new
                    {
                        Status = "fail",
                        Message = "token has been expried",
                        Token = "",
                        Timeout = 0
                    });
                }
            } catch (ArgumentException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "invalid token",
                    Token = "",
                    Timeout = 0
                });
            }
            try
            {
                string newToken = await userService.FreshTokenAsync(body);
                return Ok(new
                {
                    Status = "success",
                    Message = "token fresh",
                    Token = newToken,
                    Timeout = tokenManagement.AccessExpiration * 60
                });
            } catch (UserNotExistException) {
                return Unauthorized(new
                {
                    Status = "fail",
                    Message = "user has been removed",
                    Token = "",
                    Timeout = 0
                });
            }
        }
    }
}
