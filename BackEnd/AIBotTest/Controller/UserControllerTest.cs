using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Xunit;
using Moq;

using Buaa.AIBot.Services;
using Buaa.AIBot.Controllers;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;
using Microsoft.Extensions.Options;

using BNBCrypt = BCrypt.Net.BCrypt;

namespace AIBotTest.Controller
{

    public class UserControllerTest
    {
        private Mock<IUserService> mockServ;
        
        private Mock<IUserRepository> mockRepo;

        private Mock<IOptions<TokenManagement>> mockTMO;

        private TokenManagement mockTM;

        private UserBody body;

        public UserControllerTest()
        {
            mockServ = new Mock<IUserService>();
            mockRepo = new Mock<IUserRepository>();
            mockTMO = new Mock<IOptions<TokenManagement>>();
            mockTM = new TokenManagement();
            mockTMO.Setup(tmo => tmo.Value).Returns(mockTM);
            body = new UserBody();
        }

        [Fact]
        public async Task SignupAsync_CheckFail()
        {
            StatusMessageResponse response = new StatusMessageResponse();
            mockServ.Setup(serv => serv.CheckSignUpBody(It.IsAny<UserBody>(), out response)).Returns(false);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);
            
            var ret = await controller.SignUpAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = Assert.IsType<StatusMessageResponse>(okRet.Value);
            Assert.Equal(resRet, response);
        }

        [Fact]
        public async Task SignupAsync_Success()
        {
            var response = new StatusMessageResponse();
            mockServ.Setup(serv => serv.CheckSignUpBody(It.IsAny<UserBody>(), out response)).Returns(true);
            mockRepo.Setup(repo => repo.InsertUserAsync(It.IsAny<UserInfo>())).ReturnsAsync(1);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.SignUpAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = Assert.IsType<StatusMessageResponse>(okRet.Value);
            Assert.Equal("success", resRet.Status);
            Assert.Equal("signup success", resRet.Message);
        }

        [Fact]
        public async Task SignupAsync_EmailHasExistException()
        {
            var response = new StatusMessageResponse();
            mockServ.Setup(serv => serv.CheckSignUpBody(It.IsAny<UserBody>(), out response)).Returns(true);
            mockRepo.Setup(repo => repo.InsertUserAsync(It.IsAny<UserInfo>())).
            ThrowsAsync(new EmailHasExistException(""));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.SignUpAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = Assert.IsType<StatusMessageResponse>(okRet.Value);
            Assert.Equal("emailExisted", resRet.Status);
            Assert.Equal("email has been used", resRet.Message);
        }

        [Fact]
        public async Task SignupAsync_NameHasExistException()
        {
            var response = new StatusMessageResponse();
            mockServ.Setup(serv => serv.CheckSignUpBody(It.IsAny<UserBody>(), out response)).Returns(true);
            mockRepo.Setup(repo => repo.InsertUserAsync(It.IsAny<UserInfo>())).
            ThrowsAsync(new NameHasExistException(""));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.SignUpAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = Assert.IsType<StatusMessageResponse>(okRet.Value);
            Assert.Equal("nameExisted", resRet.Status);
            Assert.Equal("name has been used", resRet.Message);
        }

        [Fact]
        public async Task LoginAsync_Fail()
        {
            mockServ.Setup(serv => serv.AuthorizeAccountAsync(It.IsAny<UserBody>())).ReturnsAsync(new string[]
            {
               string.Empty,
               "" 
            });
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.LoginAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("wrong email or password", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
            Assert.Equal(0, resRet.GetType().GetProperty("Timeout").GetValue(resRet, null));
        }

        [Fact]
        public async Task LoginAsync_Success()
        {
            mockServ.Setup(serv => serv.AuthorizeAccountAsync(It.IsAny<UserBody>())).ReturnsAsync(new string[]
            {
                "temp",
                "Traveler"
            });
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);
            mockTM.AccessExpiration = 30;
            
            var ret = await controller.LoginAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("login success", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
            Assert.Equal(30 * 60, resRet.GetType().GetProperty("Timeout").GetValue(resRet, null));
        }


        [Fact]
        public async Task GetInternalInfoAsync_Success()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(0);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(0)).ReturnsAsync
            (
                new UserInfo
                {
                    UserId = 0,
                    Name = "test",
                    Email = "test@aiape.icu"
                }
            );
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.GetInternalInfoAsync();
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("test@aiape.icu", resRet.GetType().GetProperty("Email").GetValue(resRet, null));
        }

        [Fact]
        public async Task GetInternalInfoAsync_Fail()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(0);
            UserInfo userInfo = null;
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(0)).ReturnsAsync(userInfo);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.GetInternalInfoAsync();
            var okRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("N@N", resRet.GetType().GetProperty("Email").GetValue(resRet, null));
        }

        [Fact]
        public async Task GetFullInfoAsync_Unauthorized()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(0);
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockServ.Setup(serv => serv.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.User);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.GetFullInfoAsync();
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal(-1, resRet.GetType().GetProperty("Uid").GetValue(resRet, null));
        }

        [Fact]
        public async Task GetFullInfoAsync_NotFound()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(0);
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            UserInfo userInfo = null;
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(0)).ReturnsAsync(userInfo);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.GetFullInfoAsync();
            var okRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal(0, resRet.GetType().GetProperty("Uid").GetValue(resRet, null));
            Assert.Equal("", resRet.GetType().GetProperty("Name").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task GetFullInfoAsync_Ok()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(0);
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(0)).ReturnsAsync
            (
                new UserInfo
                {
                    UserId = 1,
                    Name = "test",
                    Email = "test@aiape.icu",
                    Auth = AuthLevel.Admin
                }
            );
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.GetFullInfoAsync();
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal(1, resRet.GetType().GetProperty("Uid").GetValue(resRet, null));
            Assert.Equal(2, resRet.GetType().GetProperty("Auth").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_LackOfAuth()
        {
            body.Uid = 2;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockServ.Setup(serv => serv.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.User);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("lack of authority", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_UserNotExist()
        {
            body.Uid = 1;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            UserInfo userInfo = null;
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(userInfo);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("user dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_NameInvalid()
        {
            body.Uid = 1;
            body.Name = "!^*#";
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            mockServ.Setup(serv => serv.CheckName("!^*#"))
            .Returns(false);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("nameInvalid", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("either contains illegal characters or length of name is too long or too short", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_PasswordInvalid()
        {
            body.Uid = 1;
            body.Password = "!";
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            mockServ.Setup(serv => serv.CheckPassword("!"))
            .Returns(false);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("passwordInvalid", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("either contains illegal characters or length of password is too long or too short", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task ModifyAsync_PasswordSame()
        {
            body.Uid = 1;
            body.Password = "!";
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo
            {
                Bcrypt = BNBCrypt.HashPassword(body.Password)
            });
            mockServ.Setup(serv => serv.CheckPassword("!"))
            .Returns(true);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("passwordSame", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new password can not be the same as original one", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task ModifyAsync_LackOfAuth1()
        {
            body.Uid = 1;
            body.Auth = 2;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockServ.Setup(serv => serv.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.User);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("lack of authority", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_TryChangeSelfAuth()
        {
            body.Uid = 1;
            body.Auth = 2;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockServ.Setup(serv => serv.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.Admin);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("tryChangeSelfAuth", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("try to change authority of current user", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_NameExisted()
        {
            body.Uid = 1;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            mockRepo.Setup(repo => repo.UpdateUserAsync(It.IsAny<UserInfo>()))
            .ThrowsAsync(new NameHasExistException(""));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<ConflictObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("nameExisted", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("name has been used by other user", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAsync_Success()
        {
            body.Uid = 1;
            mockServ.Setup(serv => serv.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectUserByIdAsync(1)).ReturnsAsync(new UserInfo());
            mockRepo.Setup(repo => repo.UpdateUserAsync(It.IsAny<UserInfo>()));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.ModifyAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("modify successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task QuestionsAsync_NotFound()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(1);
            IEnumerable<int> qids = null;
            mockRepo.Setup(repo => repo.SelectQuestionsIdByIdOrderByModifyTimeAsync(1)).ReturnsAsync(qids);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.QuestionsAsync();
            var okRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("user dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task QuestionsAsync_Ok()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectQuestionsIdByIdOrderByModifyTimeAsync(1)).ReturnsAsync(new int[0]);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.QuestionsAsync();
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("get questions for user1 successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AnswersAsync_NotFound()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(1);
            IEnumerable<AnswerIdInfo> aids = null;
            mockRepo.Setup(repo => repo.SelectAnswersIdByIdByModifyTimeAsync(1)).ReturnsAsync(aids);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.AnswersAsync();
            var okRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("user dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AnswersAsync_Ok()
        {
            mockServ.Setup(serv => serv.GetUidFromParameters(It.IsAny<HttpRequest>()))
            .Returns(1);
            mockRepo.Setup(repo => repo.SelectAnswersIdByIdByModifyTimeAsync(1)).ReturnsAsync(new AnswerIdInfo[0]);
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.AnswersAsync();
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("get answers for user1 successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task Fresh_Async_TokenInvalid()
        {
            mockServ.Setup(serv => serv.GetExpirationFromToken(It.IsAny<UserBody>())).Throws(new ArgumentException());
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.FreshAsync(body);
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("invalid token", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }



        [Fact]
        public async Task FreshAsync_Expired()
        {
            mockServ.Setup(serv => serv.GetExpirationFromToken(It.IsAny<UserBody>())).Returns(DateTime.Now.AddMinutes(-30));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.FreshAsync(body);
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("token has been expried", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task FreshAsync_UserNotExist()
        {
            mockServ.Setup(serv => serv.GetExpirationFromToken(It.IsAny<UserBody>())).Returns(DateTime.Now.AddMinutes(30));
            mockServ.Setup(serv => serv.FreshTokenAsync(It.IsAny<UserBody>())).Throws(new UserNotExistException(-1));
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.FreshAsync(body);
            var okRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("user has been removed", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }



        [Fact]
        public async Task FreshAsync_Success()
        {
            mockServ.Setup(serv => serv.GetExpirationFromToken(It.IsAny<UserBody>())).Returns(DateTime.Now.AddMinutes(30));
            mockServ.Setup(serv => serv.FreshTokenAsync(It.IsAny<UserBody>())).ReturnsAsync("");
            UserController controller = new UserController(mockServ.Object, mockRepo.Object, mockTMO.Object);

            var ret = await controller.FreshAsync(body);
            var okRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = okRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("token fresh", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }
    }
}