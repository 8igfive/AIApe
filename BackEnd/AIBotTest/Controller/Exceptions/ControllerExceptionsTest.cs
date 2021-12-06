using Xunit;
using System;

using Buaa.AIBot.Controllers.Exceptions;
using Buaa.AIBot.Repository.Models;

namespace AIBotTest.Controller.Exceptions
{
    public class LackofAuthorityExceptionTest
    {
        [Fact]
        public void LAETest()
        {
            var exception1 = new LackofAuthorityException(AuthLevel.User);
            var exception2 = new LackofAuthorityException(AuthLevel.None, new Exception(""));
        }
    }
}