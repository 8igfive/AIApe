using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Xunit;
using Moq;

using Buaa.AIBot.Services;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot.Controllers;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Models;
using Microsoft.Extensions.Options;

namespace AIBotTest.Controller
{
    public class QuestionsControllerTest
    {
        private Mock<IQuestionService> mockQues;

        private Mock<IUserService> mockUser;

        private Mock<IHotListService> mockHotList;

        private QuestionBody body;

        public QuestionsControllerTest()
        {
            mockQues = new Mock<IQuestionService>();
            mockUser = new Mock<IUserService>();
            mockHotList = new Mock<IHotListService>();
            body = new QuestionBody();
        }

        // [Fact]
        // public async Task QuestionAsync_QuestionNotExist()
        // {

        // }

        // [Fact]
        // public async Task QuestionAsync_Success()
        // {

        // }

        [Fact]
        public async Task QuestionlistAsyncTest()
        {
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.QuestionlistAsync(body);
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task TaglistAsyncTest()
        {
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.TaglistAsync();
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task AddQuestionAsync_QuestionTooLong()
        {
            mockQues.Setup(ques => ques.AddQuestionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new QuestionTitleTooLongException(0, 0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var res = await controller.AddQuestionAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("questionTooLong", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("describtion of question is too long", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddQuestionAsync_UserNotExist()
        {
            mockQues.Setup(ques => ques.AddQuestionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new UserNotExistException(-1));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var res = await controller.AddQuestionAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("adding question fail for user problem", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task UserNotExist_TagNotExis()
        {
            mockQues.Setup(ques => ques.AddQuestionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ThrowsAsync(new TagNotExistException(-1));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var res = await controller.AddQuestionAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("tagNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("can not resolve tags", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddQuestionAsync_Success()
        {
            mockQues.Setup(ques => ques.AddQuestionAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<IEnumerable<int>>()))
            .ReturnsAsync(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var res = await controller.AddQuestionAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new question add successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddAnswerAsync_Success()
        {
            mockQues.Setup(ques => ques.AddAnswerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var res = await controller.AddAnswerAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new answer add successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddAnswerAsync_UserHasAnswerTheQuestion()
        {
            mockQues.Setup(ques => ques.AddAnswerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new UserHasAnswerTheQuestionException(0, 0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var res = await controller.AddAnswerAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("answerHasExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("user has answered this question", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddAnswerAsync_UserNotExist()
        {
            mockQues.Setup(ques => ques.AddAnswerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new UserNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            var res = await controller.AddAnswerAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("userNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            // Assert.Equal("adding question fail for user problem", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task AddTagAsync_Success()
        {
            mockQues.Setup(ques => ques.AddTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var res = await controller.AddTagAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new tag add successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddTagAsync_TagNameTooLong()
        {
            mockQues.Setup(ques => ques.AddTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TagNameTooLongException(0, 0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var res = await controller.AddTagAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("nameTooLong", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("name of tag is too long", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task AddTagAsync_TagNameExist()
        {
            mockQues.Setup(ques => ques.AddTagAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TagNameExistException(""));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var res = await controller.AddTagAsync(body);
            var statRes = Assert.IsType<OkObjectResult>(res);
            var resRet = statRes.Value;
            Assert.Equal("nameHasExists", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("tag with this name has already existed", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }
        #region check
        // [Fact]
        // public async Task CheckQidAsync_Success()
        // {
        //     mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>()))
        //     .ReturnsAsync(new QuestionInformation
        //     {
        //         Creator = 0
        //     });
        //     mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(0);
        // }

        // [Fact]
        // public async Task CheckQidAsync_Fail()
        // {
        //     mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>()))
        //     .ThrowsAsync(new QuestionNotExistException(0));
            
        //     // or

        //     mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>()))
        //     .ReturnsAsync(new QuestionInformation
        //     {
        //         Creator = 0
        //     });
        //     mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(-1);
        //     mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(AuthLevel.None);
        // }
        
        // [Fact]
        // public async Task CheckAidAsync_Success()
        // {
        //     mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>()))
        //     .ReturnsAsync(new AnswerInformation
        //     {
        //         Creator = 0
        //     });
        //     mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(0);
        // }
        
        // [Fact]
        // public async Task CheckAidAsync_Fail()
        // {
        //     mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>()))
        //     .ThrowsAsync(new AnswerNotExistException(0));
            
        //     // or

        //     mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>()))
        //     .ReturnsAsync(new AnswerInformation
        //     {
        //         Creator = 0
        //     });
        //     mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(-1);
        //     mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
        //     .Returns(AuthLevel.None);
        // }
        #endregion
        [Fact]
        public async Task ModifyQuestionAsync_QuestionNotExist1()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ThrowsAsync(new QuestionNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("questionNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"question with qid={body.Qid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task ModifyQuestionAsync_LackofAuthority()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(-1);
            mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.None);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("your authority is not enough", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task ModifyQuestionAsync_Success()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyQuestionAsync(It.IsAny<int>(), It.IsAny<QuestionModifyItems>()));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("question modification successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task ModifyQuestionAsync_QuestionNotExist2()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyQuestionAsync(It.IsAny<int>(), It.IsAny<QuestionModifyItems>()))
            .ThrowsAsync(new QuestionNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("questionNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"question with qid={body.Qid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyQuestionAsync_QuestionTitleTooLong()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyQuestionAsync(It.IsAny<int>(), It.IsAny<QuestionModifyItems>()))
            .ThrowsAsync(new QuestionTitleTooLongException(0, 0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("questionTitleTooLong", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new title is too long", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyQuestionAsync_TagNotExist()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyQuestionAsync(It.IsAny<int>(), It.IsAny<QuestionModifyItems>()))
            .ThrowsAsync(new TagNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("tagNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("tags include illegal tag(s)", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyQuestionAsync_AnswerNotExist()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyQuestionAsync(It.IsAny<int>(), It.IsAny<QuestionModifyItems>()))
            .ThrowsAsync(new AnswerNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.ModifyQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("answerNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("there is no specific best answer", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAnswerAsync_AnswerNotExist1()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ThrowsAsync(new AnswerNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyAnswerAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("answerNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"answer with aid={body.Aid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }


        [Fact]
        public async Task ModifyAnswerAsync_LackofAuthority()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(-1);
            mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.None);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyAnswerAsync(body);
            var statRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("unauthorized", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("your authority is not enough", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAnswerAsync_Success()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyAnswerAsync(It.IsAny<int>(), It.IsAny<string>()));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyAnswerAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("answer modification successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyAnswerAsync_AnswerNotExist2()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.ModifyAnswerAsync(It.IsAny<int>(), It.IsAny<string>()))
            .ThrowsAsync(new AnswerNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyAnswerAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("answerNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"answer with aid={body.Aid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyTagAsync_Success()
        {
            mockQues.Setup(ques => ques.ModifyTagAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyTagAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("tag modification success", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyTagAsync_TagNotExist()
        {
            mockQues.Setup(ques => ques.ModifyTagAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TagNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyTagAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("tagNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"tag with tid={body.Tid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task ModifyTagAsync_TagNameTooLong()
        {
            mockQues.Setup(ques => ques.ModifyTagAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TagNameTooLongException(0, 0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyTagAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("nameTooLong", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("new name is too long", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task ModifyTagAsync_TagNameExist()
        {
            mockQues.Setup(ques => ques.ModifyTagAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new TagNameExistException(""));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.ModifyTagAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("nameHasExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("name has already been used", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteQuestionAsync_QuestionNotExist1()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ThrowsAsync(new QuestionNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("questionNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"question with qid={body.Qid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task DeleteQuestionAsync_LackofAuthority()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(-1);
            mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.None);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.DeleteQuestionAsync(body);
            var statRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("fail", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("your authority is not enough", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task DeleteQuestionAsync_Success()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.DeleteQuestionAsync(It.IsAny<int>()));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.DeleteQuestionAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("question removed successfully", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteQuestionAsync_QuestionNotExist2()
        {
            mockQues.Setup(ques => ques.GetQuestionAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new QuestionInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.DeleteQuestionAsync(It.IsAny<int>()))
            .ThrowsAsync(new QuestionNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var ret = await controller.DeleteQuestionAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("questionNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"question with qid={body.Qid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));

        }

        [Fact]
        public async Task DeleteAnswerAsync_AnswerNotExist1()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ThrowsAsync(new AnswerNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteAnswerAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("answerNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"answer with aid={body.Aid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteAnswerAsync_LackofAuthority()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(-1);
            mockUser.Setup(user => user.GetAuthLevelFromToken(It.IsAny<HttpRequest>()))
            .Returns(AuthLevel.None);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteAnswerAsync(body);
            var statRet = Assert.IsType<UnauthorizedObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("unauthorized", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("your authority is not enough", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteAnswerAsync_Success()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.DeleteAnswerAsync(It.IsAny<int>()));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteAnswerAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("answer removed", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteAnswerAsync_AnswerNotExist2()
        {
            mockQues.Setup(ques => ques.GetAnswerAsync(It.IsAny<int>(), null))
            .ReturnsAsync(new AnswerInformation
            {
                Creator = 0
            });
            mockQues.Setup(ques => ques.DeleteAnswerAsync(It.IsAny<int>()))
            .ThrowsAsync(new AnswerNotExistException(0));
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>()))
            .Returns(0);
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteAnswerAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("answerNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"answer with aid={body.Aid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task DeleteTagAsync_Success()
        {
            mockQues.Setup(ques => ques.DeleteTagAsync(It.IsAny<int>()));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteTagAsync(body);
            var statRet = Assert.IsType<OkObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("success", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal("tag removed", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        }

        [Fact]
        public async Task DeleteTagAsync_TagNotExist()
        {
            mockQues.Setup(ques => ques.DeleteTagAsync(It.IsAny<int>()))
            .ThrowsAsync(new TagNotExistException(0));
            QuestionsController controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);

            var ret = await controller.DeleteTagAsync(body);
            var statRet = Assert.IsType<NotFoundObjectResult>(ret);
            var resRet = statRet.Value;
            Assert.Equal("tagNotExist", resRet.GetType().GetProperty("Status").GetValue(resRet, null));
            Assert.Equal($"tag with tid={body.Tid.GetValueOrDefault(-1)} dose not exist", resRet.GetType().GetProperty("Message").GetValue(resRet, null));
        
        }

        [Fact]
        public async Task LikeAnswerAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>())).Returns(1);
            mockQues.Setup(ques => ques.LikeAnswerAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.success,
                    UserLiked = true,
                    LikeNum = 1
                }
            );

            var body = new LikeAnswerBody
            {
                Aid = 1,
                MarkAsLike = true,
            };

            var ret = await controller.LikeAnswerAsync(body);
            Assert.IsType<OkObjectResult>(ret);
            mockQues.Setup(ques => ques.LikeAnswerAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.alreadyLiked,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeAnswerAsync(body);
            Assert.IsType<OkObjectResult>(ret);
            mockQues.Setup(ques => ques.LikeAnswerAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.notLiked,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeAnswerAsync(body);
            Assert.IsType<OkObjectResult>(ret);
            mockQues.Setup(ques => ques.LikeAnswerAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.answerNotExist,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeAnswerAsync(body);
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task LikeQuestionAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            
            var body = new LikeQuestionBody
            {
                Qid = 1,
                MarkAsLike = true
            };
            
            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>())).Returns(1);
            mockQues.Setup(ques => ques.LikeQuestionAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.success,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            var ret = await controller.LikeQuestionAsync(body);
            Assert.IsType<OkObjectResult>(ret);

            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>())).Returns(1);
            mockQues.Setup(ques => ques.LikeQuestionAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.alreadyLiked,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeQuestionAsync(body);
            Assert.IsType<OkObjectResult>(ret);

            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>())).Returns(1);
            mockQues.Setup(ques => ques.LikeQuestionAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.notLiked,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeQuestionAsync(body);
            Assert.IsType<OkObjectResult>(ret);

            mockUser.Setup(user => user.GetUidFromToken(It.IsAny<HttpRequest>())).Returns(1);
            mockQues.Setup(ques => ques.LikeQuestionAsync
            (
                           It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
                    .ReturnsAsync(new LikeProduceResult
                {
                    Status = LikeProduceResult.ResultStatus.answerNotExist,
                    UserLiked = true,
                    LikeNum = 1
                }
            );
            ret = await controller.LikeQuestionAsync(body);
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task SearchAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            var body = new QuestionBody
            {
                Tags = new int[]
                {
                    1, 2, 3
                },
                Content = ""
            };
            mockQues.Setup(ques => ques.SearchQuestionAsync
            (
                It.IsAny<string>(), It.IsAny<IEnumerable<int>>()
            )).ReturnsAsync(new int[]
            {
                1, 2, 3
            });
            var ret = await controller.SearchAsync(body);
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task HotlistAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            mockHotList.Setup(hot => hot.GetHotListAsync()).ReturnsAsync(new int[]
            {
                1, 2, 3
            });
            var ret = await controller.HotlistAsync();
            Assert.IsType<OkObjectResult>(ret);
        }

        [Fact]
        public async Task TagCategooryAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            var ret = new Dictionary<string, IReadOnlyDictionary<string, int>>();
            ret.Add("1", new Dictionary<string, int>
            {
                {"2", 2}
            });
            mockQues.Setup(ques => ques.GetTagCategoryAsync()).ReturnsAsync(ret);
            var ret1 = await controller.TagCategooryAsync();
            Assert.IsType<OkObjectResult>(ret1);
        }

        [Fact]
        public async Task AutoTagAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            var body = new QuestionBody
            {
                Title = "",
                Remarks = ""
            };
            mockQues.Setup(ques => ques.GenerageTagsForQuestionAsync
            (
                It.IsAny<string>(), It.IsAny<string>()
            )).ReturnsAsync(new Dictionary<string, int>
            {
                {"1", 1}
            });
            var ret1 = await controller.AutoTagAsync(body);
            Assert.IsType<OkObjectResult>(ret1);
        }

        [Fact]
        public async Task IsCodeAsyncTest()
        {
            var controller = new QuestionsController(mockQues.Object, mockUser.Object, mockHotList.Object);
            var body = new QuestionBody
            {
                Title = "",
                Remarks = ""
            };
            mockQues.Setup(ques => ques.QuestionIsCodeAsync
            (
                It.IsAny<string>(), It.IsAny<string>()
            )).ReturnsAsync(true);
            var ret1 = await controller.IsCodeAsync(body);
            Assert.IsType<OkObjectResult>(ret1);
        }
    }

}