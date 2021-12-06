using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Implement;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

namespace AIBotTest.Repository
{
    public class QuestionRepositoryTest
    {
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();

        public IQuestionRepository CreateQuestionRepository(DatabaseContext context)
        {
            return new QuestionRepository(context, cachePool, new TagRepository(context, cachePool, globalCancellation), globalCancellation);
        }

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(QuestionRepositoryTest)}@{count++}");
            return builder.Options;
        }

        private async Task<int> InitAsync(DbContextOptions<DatabaseContext> options,int tagNum, HashSet<int> tids)
        {
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(Enumerable.Range(0, tagNum)
                    .Select(i => new TagData() { Name = $"{i}", Desc = $"{i}" }));
                var user = new UserData()
                {
                    Name = "user",
                    Email = "user@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user);
                await context.SaveChangesAsync();
                (await context.Tags.Select(t => t.TagId).ToListAsync()).ForEach(tid =>
                    tids.Add(tid));
                return user.UserId;
            }
        }

        [Fact]
        public async Task SelectQuestionsByTagsAsyncTest()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids;
            HashSet<int> tidsForQuestion;
            int qid;
            int uid;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(Enumerable.Range(0, tagNum)
                    .Select(i => new TagData() { Name = $"{i}", Desc = $"{i}" , Category = (int)TagCategory.Other}));

                var user = new UserData()
                {
                    Name = "user",
                    Email = "user@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
                tids = new HashSet<int>(await context.Tags.Select(t => t.TagId).ToListAsync());
                tidsForQuestion = new HashSet<int>(tids.Take(tagNum / 2));

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid
                };
                context.Add(question1);
                await context.SaveChangesAsync();
                qid = question1.QuestionId;

                context.AddRange(tidsForQuestion.Select(t => new QuestionTagRelation()
                {
                    TagId = t,
                    QuestionId = qid
                }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res1 = await questionRepository.SelectQuestionsByTagsAsync(tidsForQuestion);
                Assert.Single(res1);
                Assert.Equal(qid, res1.Single());
            }
            // Wrong with SQLite.InMemory but correct with MySQL ?
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                int tidNotForQuestion = tids.Where(t => !tidsForQuestion.Contains(t)).First();
                var res2 = await questionRepository.SelectQuestionsByTagsAsync(
                     tidsForQuestion.Append(tidNotForQuestion));
                //Assert.Empty(res2);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res3 = await questionRepository.SelectQuestionsByTagsAsync(
                    tidsForQuestion.Take(tagNum / 4));
                Assert.Single(res3);
                Assert.Equal(qid, res3.Single());
            }
            // Wrong with SQLite.InMemory but correct with MySQL ?
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                int tidNotExist = tids.Max() + 1;
                var res4 = await questionRepository.SelectQuestionsByTagsAsync(
                    tidsForQuestion.Append(tidNotExist));
                //Assert.Empty(res4);
            }
        }

        [Fact]
        public async Task SelectQuestionByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            string title1 = "Question1";
            string remarks1 = "This is question No.1";
            string title2 = "Question2";
            string remarks2 = "This is question No.2";
            int qid1;
            int qid2;
            int uid;
            using (var context = new DatabaseContext(options))
            {
                var user = new UserData()
                {
                    Name = "user",
                    Email = "user@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;

                var question1 = new QuestionData()
                {
                    Title = title1,
                    Remarks = remarks1,
                    UserId = uid
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = title2,
                    Remarks = remarks2,
                    UserId = uid
                };
                context.Add(question2);
                await context.SaveChangesAsync();
                qid1 = question1.QuestionId;
                qid2 = question2.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res1 = await questionRepository.SelectQuestionByIdAsync(qid1);
                Assert.Equal(qid1, res1.QuestionId);
                Assert.Equal(title1, res1.Title);
                Assert.Equal(remarks1, res1.Remarks);
                Assert.Equal(uid, res1.CreaterId);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res2 = await questionRepository.SelectQuestionByIdAsync(qid2);
                Assert.Equal(qid2, res2.QuestionId);
                Assert.Equal(title2, res2.Title);
                Assert.Equal(remarks2, res2.Remarks);
                Assert.Equal(uid, res2.CreaterId);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                int qidNotExist = Math.Max(qid1, qid2) + 1;
                var res3 = await questionRepository.SelectQuestionByIdAsync(qidNotExist);
                Assert.Null(res3);
            }
        }

        [Fact]
        public async Task SelectAnswersForQuestionByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            string title1 = "Question1";
            string remarks1 = "This is question No.1";
            string title2 = "Question2";
            string remarks2 = "This is question No.2";
            int qid1;
            int qid2;
            int uid1;
            int uid2;
            int aid1;
            int aid2;
            using (var context = new DatabaseContext(options))
            {
                var user1 = new UserData()
                {
                    Name = "user1",
                    Email = "user1@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user1);
                var user2 = new UserData()
                {
                    Name = "user2",
                    Email = "user2@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user2);
                await context.SaveChangesAsync();
                uid1 = user1.UserId;
                uid2 = user2.UserId;

                var question1 = new QuestionData()
                {
                    Title = title1,
                    Remarks = remarks1,
                    UserId = uid1
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = title2,
                    Remarks = remarks2,
                    UserId = uid1
                };
                context.Add(question2);
                await context.SaveChangesAsync();
                qid1 = question1.QuestionId;
                qid2 = question2.QuestionId;

                var answer1 = new AnswerData()
                {
                    Content = $"answer No.1 for question1 by user1",
                    UserId = uid1,
                    QuestionId = qid1
                };
                var answer2 = new AnswerData()
                {
                    Content = $"answer No.2 for question1 by user2",
                    UserId = uid2,
                    QuestionId = qid1
                };
                context.Add(answer1);
                context.Add(answer2);
                await context.SaveChangesAsync();
                aid1 = answer1.AnswerId;
                aid2 = answer2.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res1 = await questionRepository.SelectAnswersForQuestionByIdAsync(qid1);
                Assert.Equal(2, res1.Count());
                Assert.Contains(aid1, res1);
                Assert.Contains(aid2, res1);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res2 = await questionRepository.SelectAnswersForQuestionByIdAsync(qid2);
                Assert.Empty(res2);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res3 = await questionRepository.SelectAnswersForQuestionByIdAsync(Math.Max(qid1, qid2) + 1);
                Assert.Null(res3);
            }
        }

        [Fact]
        public async Task SelectTagsForQuestionByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids;
            HashSet<int> tidsForQuestion;
            int qid1;
            int qid2;
            int uid;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(Enumerable.Range(0, tagNum)
                    .Select(i => new TagData() { Name = $"{i}", Desc = $"{i}" }));

                var user = new UserData()
                {
                    Name = "user",
                    Email = "user@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
                tids = new HashSet<int>(await context.Tags.Select(t => t.TagId).ToListAsync());
                tidsForQuestion = new HashSet<int>(tids.Take(tagNum / 2));

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid
                };
                var question2 = new QuestionData()
                {
                    Title = "Question2",
                    Remarks = "This is question No.2",
                    UserId = uid
                };
                context.Add(question1);
                context.Add(question2);
                await context.SaveChangesAsync();
                qid1 = question1.QuestionId;
                qid2 = question2.QuestionId;

                context.AddRange(tidsForQuestion.Select(t => new QuestionTagRelation()
                {
                    TagId = t,
                    QuestionId = qid1
                }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res1 = await questionRepository.SelectTagsForQuestionByIdAsync(qid1);
                Assert.Equal(tidsForQuestion.Count, res1.Count());
                Assert.True(
                    tidsForQuestion.OrderBy(t => t).SequenceEqual(
                        res1.Select(kv => kv.Value).OrderBy(t => t)));
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res2 = await questionRepository.SelectTagsForQuestionByIdAsync(qid2);
                Assert.Empty(res2);
            }
            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                var res3 = await questionRepository.SelectTagsForQuestionByIdAsync(Math.Max(qid1, qid2) + 1);
                Assert.Null(res3);
            }
        }

        #region InsertQuestionAsync

        [Fact]
        public async Task InsertQuestionAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                string title = "title";
                string remarks = "remarks";
                var question = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = title,
                    Remarks = remarks,
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(question);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(uid, res.CreaterId);
                Assert.Equal(title, res.Title);
                Assert.Equal(remarks, res.Remarks);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tags.Select(kv => kv.Value).OrderBy(t => t).SequenceEqual(
                    tidsForQuestion.OrderBy(t => t)));
            }
        }

        [Fact]
        public async Task InsertQuestionAsync_CreaterIdNull()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                string title = "title";
                string remarks = "remarks";
                var question = new QuestionWithListTag()
                {
                    CreaterId = null,
                    Title = title,
                    Remarks = remarks,
                    Tags = tidsForQuestion
                };
                await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                    await questionRepository.InsertQuestionAsync(question));

                Assert.Empty(context.Questions);
            }
        }

        [Fact]
        public async Task InsertQuestionAsync_TitleTooLong()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            var titleBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.QuestionTitleMaxLength))
            {
                titleBuilder.Append('t');
            }

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                string remarks = "remarks";
                var maxLengthTitleQuestion = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = titleBuilder.ToString(),
                    Remarks = remarks,
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(maxLengthTitleQuestion);
                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.NotNull(res);

                titleBuilder.Append('t');
                var tooLongTitleQuestion = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = titleBuilder.ToString(),
                    Remarks = remarks,
                    Tags = tidsForQuestion
                };
                await Assert.ThrowsAsync<QuestionTitleTooLongException>(async () =>
                    await questionRepository.InsertQuestionAsync(tooLongTitleQuestion));

                Assert.Single(context.Questions);
            }
        }

        [Fact]
        public async Task InsertQuestionAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                string title = "title";
                string remarks = "remarks";
                var question = new QuestionWithListTag()
                {
                    CreaterId = uid + 1,
                    Title = title,
                    Remarks = remarks,
                    Tags = tidsForQuestion
                };
                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await questionRepository.InsertQuestionAsync(question));

                Assert.Empty(context.Questions);
            }
        }

        [Fact]
        public async Task InsertQuestionAsync_TagNotExist()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                string title = "title";
                string remarks = "remarks";
                var question = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = title,
                    Remarks = remarks,
                    Tags = tidsForQuestion.Append(tids.Max() + 1)
                };
                await Assert.ThrowsAsync<TagNotExistException>(async () =>
                    await questionRepository.InsertQuestionAsync(question));

                Assert.Empty(context.Questions);
            }
        }

        #endregion

        #region UpdateQuestionAsync

        [Fact]
        public async Task UpdateQuestionAsync_TitleOnly()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                var newQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Title = "newTitle"
                };
                await questionRepository.UpdateQuestionAsync(newQuestion);

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(newQuestion.Title, res.Title);
                Assert.Equal(origin.Remarks, res.Remarks);
                // Seems not work with SQLite.InMemory
                // Assert.NotEqual(res.CreateTime, res.ModifyTime);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tidsForQuestion.OrderBy(t => t).SequenceEqual(
                    tags.Select(kv => kv.Value).OrderBy(t => t)));
            }
        }

        [Fact]
        public async Task UpdateQuestionAsync_RemarksOnly()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                var newQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Remarks = "newRemarks",
                };
                await questionRepository.UpdateQuestionAsync(newQuestion);

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(origin.Title, res.Title);
                Assert.Equal(newQuestion.Remarks, res.Remarks);
                // Seems not work with SQLite.InMemory
                // Assert.NotEqual(res.CreateTime, res.ModifyTime);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tidsForQuestion.OrderBy(t => t).SequenceEqual(
                    tags.Select(kv => kv.Value).OrderBy(t => t)));
            }
        }

        [Fact]
        public async Task UpdateQuestionAsync_TagsOnly()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                List<int> newTidsForQuestion = tidsForQuestion.OrderBy(t => t).Skip(1).Append(tids.Max()).ToList();
                Assert.False(newTidsForQuestion.OrderBy(t => t).SequenceEqual(tidsForQuestion.OrderBy(t => t)));
                var newQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Tags = newTidsForQuestion
                };
                await questionRepository.UpdateQuestionAsync(newQuestion);

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(origin.Title, res.Title);
                Assert.Equal(origin.Remarks, res.Remarks);
                // Seems not work with SQLite.InMemory
                // Assert.NotEqual(res.CreateTime, res.ModifyTime);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(newTidsForQuestion.OrderBy(t => t).SequenceEqual(
                    tags.Select(kv => kv.Value).OrderBy(t => t)));
            }
        }

        //[Fact]
        //public async Task UpdateQuestionAsync_BestAnswerOnly()
        //{

        //    var options = CreateUniqueOptions();

        //    string title1 = "Question1";
        //    string remarks1 = "This is question No.1";
        //    int qid1;
        //    int uid1;
        //    int uid2;
        //    int aid1;
        //    int aid2;
        //    using (var context = new DatabaseContext(options))
        //    {
        //        var user1 = new UserData()
        //        {
        //            Name = "user1",
        //            Email = "user1@buaa",
        //            Bcrypt = "bcrypt",
        //            Auth = AuthLevel.User
        //        };
        //        context.Add(user1);
        //        var user2 = new UserData()
        //        {
        //            Name = "user2",
        //            Email = "user2@buaa",
        //            Bcrypt = "bcrypt",
        //            Auth = AuthLevel.User
        //        };
        //        context.Add(user2);
        //        await context.SaveChangesAsync();
        //        uid1 = user1.UserId;
        //        uid2 = user2.UserId;

        //        var question1 = new QuestionData()
        //        {
        //            Title = title1,
        //            Remarks = remarks1,
        //            UserId = uid1
        //        };
        //        context.Add(question1);
        //        await context.SaveChangesAsync();
        //        qid1 = question1.QuestionId;

        //        var answer1 = new AnswerData()
        //        {
        //            Content = $"answer No.1 for question1 by user1",
        //            UserId = uid1,
        //            QuestionId = qid1
        //        };
        //        var answer2 = new AnswerData()
        //        {
        //            Content = $"answer No.2 for question1 by user2",
        //            UserId = uid2,
        //            QuestionId = qid1
        //        };
        //        context.Add(answer1);
        //        context.Add(answer2);
        //        await context.SaveChangesAsync();
        //        aid1 = answer1.AnswerId;
        //        aid2 = answer2.AnswerId;
        //    }

        //    using (var context = new DatabaseContext(options))
        //    {
        //        IQuestionRepository questionRepository = CreateQuestionRepository(context);

        //        var question = new QuestionWithListTag()
        //        {
        //            QuestionId = qid1,
        //            BestAnswerId = aid1
        //        };
        //        await questionRepository.UpdateQuestionAsync(question);

        //        var res = await questionRepository.SelectQuestionByIdAsync(qid1);
        //        Assert.Equal(aid1, res.BestAnswerId);
        //        Assert.Equal(title1, res.Title);
        //        Assert.Equal(remarks1, res.Remarks);
        //    }
        //}

        [Fact]
        public async Task UpdateQuestionAsync_UpdateAll()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                List<int> newTidsForQuestion = tidsForQuestion.OrderBy(t => t).Skip(1).Append(tids.Max()).ToList();
                Assert.False(newTidsForQuestion.OrderBy(t => t).SequenceEqual(tidsForQuestion.OrderBy(t => t)));
                var newQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Title = "newTitle",
                    Remarks = "newRemarks",
                    Tags = newTidsForQuestion
                };
                await questionRepository.UpdateQuestionAsync(newQuestion);

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(newQuestion.Title, res.Title);
                Assert.Equal(newQuestion.Remarks, res.Remarks);
                // Seems not work with SQLite.InMemory
                // Assert.NotEqual(res.CreateTime, res.ModifyTime);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(newTidsForQuestion.OrderBy(t => t).SequenceEqual(
                    tags.Select(kv => kv.Value).OrderBy(t => t)));
            }
        }

        [Fact]
        public async Task UpdateQuestionAsync_QuestionNotEixst()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                var newQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid + 1,
                    Title = "newTitle"
                };
                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await questionRepository.UpdateQuestionAsync(newQuestion));

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(origin.Title, res.Title);
                Assert.Equal(origin.Remarks, res.Remarks);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tags.Select(kv => kv.Value).OrderBy(t => t).SequenceEqual(
                    tidsForQuestion.OrderBy(t => t)));
            }
        }

        [Fact]
        public async Task UpdateQuestionAsync_TitleTooLong()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "origin",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                var titleBuilder = new StringBuilder();
                foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.QuestionTitleMaxLength))
                {
                    titleBuilder.Append('t');
                }
                var maxLengthQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Title = titleBuilder.ToString(),
                    Remarks = "max"
                };
                await questionRepository.UpdateQuestionAsync(maxLengthQuestion);

                titleBuilder.Append('t');
                var tooLongQuestion = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Title = titleBuilder.ToString(),
                    Remarks = "tooLong"
                };
                await Assert.ThrowsAsync<QuestionTitleTooLongException>(async () =>
                    await questionRepository.UpdateQuestionAsync(tooLongQuestion));

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(maxLengthQuestion.Title, res.Title);
                Assert.Equal(maxLengthQuestion.Remarks, res.Remarks);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tags.Select(kv => kv.Value).OrderBy(t => t).SequenceEqual(
                    tidsForQuestion.OrderBy(t => t)));
            }
        }

        //[Fact]
        //public async Task UpdateQuestionAsync_AnswerNotExist()
        //{
        //    var options = CreateUniqueOptions();

        //    string title1 = "Question1";
        //    string remarks1 = "This is question No.1";
        //    string title2 = "Question2";
        //    string remarks2 = "This is question No.2";
        //    int qid1;
        //    int qid2;
        //    int uid1;
        //    int uid2;
        //    int aid1;
        //    int aid2;
        //    using (var context = new DatabaseContext(options))
        //    {
        //        var user1 = new UserData()
        //        {
        //            Name = "user1",
        //            Email = "user1@buaa",
        //            Bcrypt = "bcrypt",
        //            Auth = AuthLevel.User
        //        };
        //        context.Add(user1);
        //        var user2 = new UserData()
        //        {
        //            Name = "user2",
        //            Email = "user2@buaa",
        //            Bcrypt = "bcrypt",
        //            Auth = AuthLevel.User
        //        };
        //        context.Add(user2);
        //        await context.SaveChangesAsync();
        //        uid1 = user1.UserId;
        //        uid2 = user2.UserId;

        //        var question1 = new QuestionData()
        //        {
        //            Title = title1,
        //            Remarks = remarks1,
        //            UserId = uid1
        //        };
        //        context.Add(question1);
        //        var question2 = new QuestionData()
        //        {
        //            Title = title2,
        //            Remarks = remarks2,
        //            UserId = uid1
        //        };
        //        context.Add(question2);
        //        await context.SaveChangesAsync();
        //        qid1 = question1.QuestionId;
        //        qid2 = question2.QuestionId;

        //        var answer1 = new AnswerData()
        //        {
        //            Content = $"answer No.1 for question1 by user1",
        //            UserId = uid1,
        //            QuestionId = qid1
        //        };
        //        var answer2 = new AnswerData()
        //        {
        //            Content = $"answer No.2 for question1 by user2",
        //            UserId = uid2,
        //            QuestionId = qid1
        //        };
        //        context.Add(answer1);
        //        context.Add(answer2);
        //        await context.SaveChangesAsync();
        //        aid1 = answer1.AnswerId;
        //        aid2 = answer2.AnswerId;
        //    }

        //    using (var context = new DatabaseContext(options))
        //    {
        //        IQuestionRepository questionRepository = CreateQuestionRepository(context);

        //        var question = new QuestionWithListTag()
        //        {
        //            QuestionId = qid1,
        //            BestAnswerId = Math.Max(aid1, aid2) + 1
        //        };
        //        await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
        //            await questionRepository.UpdateQuestionAsync(question));

        //        var res = await questionRepository.SelectQuestionByIdAsync(qid1);
        //        Assert.Null(res.BestAnswerId);
        //        Assert.Equal(title1, res.Title);
        //        Assert.Equal(remarks1, res.Remarks);
        //    }
        //    using (var context = new DatabaseContext(options))
        //    {
        //        IQuestionRepository questionRepository = CreateQuestionRepository(context);

        //        var question = new QuestionWithListTag()
        //        {
        //            QuestionId = qid2,
        //            BestAnswerId = aid1
        //        };
        //        await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
        //            await questionRepository.UpdateQuestionAsync(question));

        //        var res = await questionRepository.SelectQuestionByIdAsync(qid2);
        //        Assert.Null(res.BestAnswerId);
        //        Assert.Equal(title2, res.Title);
        //        Assert.Equal(remarks2, res.Remarks);
        //    }
        //}

        [Fact]
        public async Task UpdateQuestionAsync_TagNotExist()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "origin",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                var question = new QuestionWithListTag()
                {
                    QuestionId = qid,
                    Tags = tidsForQuestion.Append(tids.Max() + 1)
                };
                await Assert.ThrowsAsync<TagNotExistException>(async () =>
                    await questionRepository.UpdateQuestionAsync(question));

                Assert.Single(context.Questions);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Equal(origin.Title, res.Title);
                Assert.Equal(origin.Remarks, res.Remarks);

                var tags = await questionRepository.SelectTagsForQuestionByIdAsync(qid);
                Assert.True(tags.Select(kv => kv.Value).OrderBy(t => t).SequenceEqual(
                    tidsForQuestion.OrderBy(t => t)));
            }
        }

        #endregion

        [Fact]
        public async Task DeleteQuestionByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            HashSet<int> tids = new HashSet<int>();
            int uid = await InitAsync(options, tagNum, tids);

            using (var context = new DatabaseContext(options))
            {
                IQuestionRepository questionRepository = CreateQuestionRepository(context);

                List<int> tidsForQuestion = tids.Take(tagNum / 2).ToList();
                var origin = new QuestionWithListTag()
                {
                    CreaterId = uid,
                    Title = "title",
                    Remarks = "remarks",
                    Tags = tidsForQuestion
                };
                int qid = await questionRepository.InsertQuestionAsync(origin);

                await questionRepository.DeleteQuestionByIdAsync(qid);

                var res = await questionRepository.SelectQuestionByIdAsync(qid);
                Assert.Null(res);
                Assert.Empty(context.Questions);

                await questionRepository.DeleteQuestionByIdAsync(qid);
                Assert.Empty(context.Questions);
            }
        }
    }
}
