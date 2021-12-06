using System;
using System.Linq;
using System.Text.Json;
using Xunit;
using Buaa.AIBot.Bot.Framework;
using System.Collections.Generic;

namespace AIBotTest.Bot.Framework
{
    public class ExtentionsTest
    {
        #region Count

        [Fact]
        public void Count_Simple()
        {
            IBotStatusContainer status = new BotStatus<int>();

            var ret1 = status.GetCount(0);
            status.IncreaseCount(0);
            var ret2 = status.GetCount(0);
            var ret3 = status.GetCount(1);
            status.IncreaseCount(0);
            var ret4 = status.GetCount(0);
            status.ClearCount(0);
            var ret5 = status.GetCount(0);

            Assert.Equal(0, ret1);
            Assert.Equal(1, ret2);
            Assert.Equal(0, ret3);
            Assert.Equal(2, ret4);
            Assert.Equal(0, ret5);
        }

        [Fact]
        public void Count_Many()
        {
            IBotStatusContainer status = new BotStatus<int>();

            var ret1 = status.GetCount(0);
            status.IncreaseCount(0);
            var ret2 = status.GetCount(0);
            var ret3 = status.GetCount(1);
            status.IncreaseCount(1);
            var ret4 = status.GetCount(0);
            var ret5 = status.GetCount(1);
            status.ClearCount(0);
            var ret6 = status.GetCount(0);
            var ret7 = status.GetCount(1);
            status.ClearCount(1);
            var ret8 = status.GetCount(0);
            var ret9 = status.GetCount(1);

            Assert.Equal(0, ret1);
            Assert.Equal(1, ret2);
            Assert.Equal(0, ret3);
            Assert.Equal(1, ret4);
            Assert.Equal(1, ret5);
            Assert.Equal(0, ret6);
            Assert.Equal(1, ret7);
            Assert.Equal(0, ret8);
            Assert.Equal(0, ret9);
        }

        #endregion
    }
}
