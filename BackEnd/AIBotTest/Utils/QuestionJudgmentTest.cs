using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

using Moq;

using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Utils;
using static Buaa.AIBot.Utils.QuestionJudgement;

namespace AIBotTest.Utils
{
    public class QuestionJudgmentTest
    {
        [Fact]
        public void Simple()
        {
            var tags = new Dictionary<TagCategory, IEnumerable<int>>()
            {
                [TagCategory.Lang] = new int[] { 1, 2, 3 },
                [TagCategory.Env] = new int[] { 4, 5, 6 },
                [TagCategory.Other] = new int[] { 7, 8, 9 }
            };

            var questions = new List<IQuestionTagInfo>()
            {
                new QuestionTagInfo()
                {
                    Qid = 1,
                    Tags = new Dictionary<TagCategory, IEnumerable<int>>()
                    {
                        [TagCategory.Lang] = new int[]{ },
                        [TagCategory.Env] = new int[]{ 4, },
                        [TagCategory.Other] = new int[]{ 7, },

                    }
                },
                new QuestionTagInfo()
                {
                    Qid = 2,
                    Tags = new Dictionary<TagCategory, IEnumerable<int>>()
                    {
                        [TagCategory.Lang] = new int[]{ },
                        [TagCategory.Env] = new int[]{ 5, },
                        [TagCategory.Other] = new int[]{ 7, },

                    }
                },
                new QuestionTagInfo()
                {
                    Qid = 3,
                    Tags = new Dictionary<TagCategory, IEnumerable<int>>()
                    {
                        [TagCategory.Lang] = new int[]{ },
                        [TagCategory.Env] = new int[]{ },
                        [TagCategory.Other] = new int[]{ 7, },
                    }
                },
            };

            var selected = new Dictionary<TagCategory, IEnumerable<int>>()
            {
                [TagCategory.Lang] = new int[] { },
                [TagCategory.Env] = new int[] { 4, 5 },
                [TagCategory.Other] = new int[] { 7 }
            };

            var res = QuestionJudgement.GetFilteredQuestions(questions, selected);
            Assert.Contains(1, res);
            Assert.Contains(2, res);
            Assert.DoesNotContain(3, res);
        }

        [Fact]
        public void IsCodeTest()
        {
            var title = "这是一个简单的标题";
            var content = "<?php// Start the session (pretty important!)session_start();// Establish a link to the database$dbLink = mysql_connect('', '', '');if (!$dbLink) die('Can\'t establish a connection to the database: ' . mysql_error());$dbSelected = mysql_select_db('', $dbLink);if (!$dbSelected) die ('We\'re connected, but can\'t use the table: ' . mysql_error());$isUserLoggedIn = false;  $query = 'SELECT * FROM users WHERE session_id = \"' . session_id() . '\" LIMIT 1';  $userResult = mysql_query($query);  if(mysql_num_rows($userResult) == 1) {  $_SESSION['user'] = mysql_fetch_assoc($userResult);  $isUserLoggedIn = true; } else {  if(basename($_SERVER['PHP_SELF']) != 'conectare.php') {  header('Location: conectare.php');  exit;  } } ?>";
            Assert.Equal(true, QuestionJudgement.IsCode(title, content));
            content = "123";
            Assert.Equal(false, QuestionJudgement.IsCode(title, content));
            content = "{}();=```````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````````";
            Assert.Equal(true, QuestionJudgement.IsCode(title, content));
        }

        [Fact]
        public async Task GenerageTagsForQuestionAsyncTest()
        {
            var mockTP = new Mock<Buaa.AIBot.Repository.ITagRepostory>();
            mockTP.Setup(tp => tp.SelectAllTagsAsync()).ReturnsAsync(new Dictionary<string, int>
            {
                {"Linux", 1},
                {"Windows", 2},
                {"macOS", 3},
                {"Dev C++", 4},
                {"Visual C++", 5},
                {"VS Code", 6},
                {"Visual Studio", 7},
                {"gcc", 8},
                {"clang", 9},
                {"msvc", 10},
                {"代码", 11},
                {"工具使用", 12},
                {"标准库", 13},
                {"第三方库", 14},
                {"语句", 15},
                {"关键字", 16},
                {"算法", 17},
                {"网络", 18}
            });
            await QuestionJudgement.GenerageTagsForQuestionAsync(mockTP.Object, 
            "这是一个标题", "这一个正文");
        }
    }
}
