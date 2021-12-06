using Xunit;
using System;

using Buaa.AIBot.Controllers.Exceptions;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository.Models;

namespace AIBotTest.Controller.Models
{
    public class LikeQuestionBodyTest
    {
        [Fact]
        public void LQBTest()
        {
            var body = new LikeQuestionBody
            {
                Qid = 0,
                MarkAsLike = true
            };
            Assert.Equal(0, body.Qid);
            Assert.Equal(true, body.MarkAsLike);
        }
    }

    public class LikeAnswerBodyTest
    {
        [Fact]
        public void LABTest()
        {
            var body = new LikeAnswerBody
            {
                Aid = 0,
                MarkAsLike = true
            };
            Assert.Equal(0, body.Aid);
            Assert.Equal(true, body.MarkAsLike);
        }
    }
}