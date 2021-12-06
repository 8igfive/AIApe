using Xunit;
using System;

using Buaa.AIBot.Controllers.Exceptions;
using Buaa.AIBot.Controllers.Models;
using Buaa.AIBot.Repository.Models;

namespace AIBotTest.Controller.Models
{
    public class TokenManagementTest
    {
        [Fact]
        public void TMTest()
        {
            var token = new TokenManagement
            {
                Secret = "",
                Issuer = "",
                Audience = "",
                AccessExpiration = 0,
                RefreshExpiration = 0
            };
            Assert.Equal("", token.Secret);
            Assert.Equal("", token.Issuer);
            Assert.Equal("", token.Audience);
            Assert.Equal(0, token.AccessExpiration);
            Assert.Equal(0, token.RefreshExpiration);
        }
    }
}