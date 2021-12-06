using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Moq;

using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Utils;

namespace AIBotTest.Utils
{
    public class ToolsTest
    {
        [Fact]
        public void RangeTest()
        {
            Tools.Range(0, 10, 1);
            Tools.Range(10, 0, -1);
            Assert.Throws<ArgumentException>(() => Tools.Range(0, 10, 0).ToList());
        }
    }
}