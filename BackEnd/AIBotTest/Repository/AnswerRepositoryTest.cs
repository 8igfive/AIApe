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
    public class AnswerRepositoryTest
    {
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(AnswerRepositoryTest)}@{count++}");
            return builder.Options;
        }

        [Fact]
        public async Task SelectAnswerByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
            int aid;
            string content = "content";
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;

                var answer = new AnswerData()
                {
                    UserId = uid,
                    QuestionId = qid,
                    Content = content,
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByIdAsync(aid);
                Assert.Single(context.Answers);
                Assert.Equal(aid, res.AnswerId);
                Assert.Equal(qid, res.QuestionId);
                Assert.Equal(uid, res.AnswerId);
                Assert.Equal(content, res.Content);
            }
            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByIdAsync(aid + 1);
                Assert.Single(context.Answers);
                Assert.Null(res);
            }
        }

        #region SelectAnswerByQuestionAndUserAsync

        [Fact]
        public async Task SelectAnswerByQuestionAndUserAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
            int aid;
            string content = "content";
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;

                var answer = new AnswerData()
                {
                    UserId = uid,
                    QuestionId = qid,
                    Content = content,
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByQuestionAndUserAsync(qid, uid);
                Assert.Single(context.Answers);
                Assert.Equal(aid, res.AnswerId);
                Assert.Equal(qid, res.QuestionId);
                Assert.Equal(uid, res.AnswerId);
                Assert.Equal(content, res.Content);
            }
        }

        [Fact]
        public async Task SelectAnswerByQuestionAndUserAsync_NoAnswer()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByQuestionAndUserAsync(qid, uid);
                Assert.Empty(context.Answers);
                Assert.Null(res);
            }
        }

        [Fact]
        public async Task SelectAnswerByQuestionAndUserAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
            int aid;
            string content = "content";
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;

                var answer = new AnswerData()
                {
                    UserId = uid,
                    QuestionId = qid,
                    Content = content,
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByQuestionAndUserAsync(qid, uid + 1);
                Assert.Single(context.Answers);
                Assert.Null(res);
            }
        }

        [Fact]
        public async Task SelectAnswerByQuestionAndUserAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
            int aid;
            string content = "content";
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;

                var answer = new AnswerData()
                {
                    UserId = uid,
                    QuestionId = qid,
                    Content = content,
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var res = await answerRepository.SelectAnswerByQuestionAndUserAsync(qid + 1, uid);
                Assert.Single(context.Answers);
                Assert.Null(res);
            }
        }

        #endregion

        #region InsertAnswerAsync

        [Fact]
        public async Task InsertAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                string content = "content";
                var answer = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = content
                };
                int aid = await answerRepository.InsertAnswerAsync(answer);

                Assert.Single(context.Answers);

                var res = await answerRepository.SelectAnswerByIdAsync(aid);
                Assert.Equal(content, res.Content);
                Assert.Equal(uid, res.CreaterId);
                Assert.Equal(qid, res.QuestionId);
            }
        }

        [Fact]
        public async Task InsertAnswerAsync_CreaterIdNull()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var answer = new AnswerInfo()
                {
                    QuestionId = qid,
                    Content = "content"
                };

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await answerRepository.InsertAnswerAsync(answer));

                Assert.Empty(context.Answers);
            }
        }

        [Fact]
        public async Task InsertAnswerAsync_CreaterNotExist()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var answer = new AnswerInfo()
                {
                    CreaterId = uid + 1,
                    QuestionId = qid,
                    Content = "content"
                };

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await answerRepository.InsertAnswerAsync(answer));

                Assert.Empty(context.Answers);
            }
        }

        [Fact]
        public async Task InsertAnswerAsync_Answered()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var answer1 = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = "content1"
                };
                int aid = await answerRepository.InsertAnswerAsync(answer1);

                var answer2 = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = "content2"
                };
                await Assert.ThrowsAsync<UserHasAnswerTheQuestionException>(async () =>
                    await answerRepository.InsertAnswerAsync(answer2));

                Assert.Single(context.Answers);

                var res = await answerRepository.SelectAnswerByIdAsync(aid);
                Assert.Equal(answer1.Content, res.Content);
                Assert.Equal(uid, res.CreaterId);
                Assert.Equal(qid, res.QuestionId);
            }
        }

        [Fact]
        public async Task InsertAnswerAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                var answer = new AnswerInfo()
                {
                    QuestionId = qid + 1,
                    CreaterId = uid,
                    Content = "content"
                };

                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await answerRepository.InsertAnswerAsync(answer));

                Assert.Empty(context.Answers);
            }
        }

        #endregion

        #region UpdateAnswerAsync

        [Fact]
        public async Task UpdateAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                string content = "content";
                var origin = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = content
                };
                int aid = await answerRepository.InsertAnswerAsync(origin);

                var answer = new AnswerInfo()
                {
                    AnswerId = aid,
                    Content = content + content
                };
                await answerRepository.UpdateAnswerAsync(answer);

                Assert.Single(context.Answers);

                var res = await answerRepository.SelectAnswerByIdAsync(aid);
                Assert.Equal(answer.Content, res.Content);
            }
        }

        [Fact]
        public async Task UpdateAnswerAsync_AnswerNotExist()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                string content = "content";
                var origin = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = content
                };
                int aid = await answerRepository.InsertAnswerAsync(origin);

                var answer = new AnswerInfo()
                {
                    AnswerId = aid + 1,
                    Content = content + content
                };
                await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
                    await answerRepository.UpdateAnswerAsync(answer));

                Assert.Single(context.Answers);

                var res = await answerRepository.SelectAnswerByIdAsync(aid);
                Assert.Equal(origin.Content, res.Content);
            }
        }

        #endregion

        [Fact]
        public async Task DeleteAnswerByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            int qid;
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

                var question = new QuestionData()
                {
                    UserId = uid,
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
            }

            using (var context = new DatabaseContext(options))
            {
                IAnswerRepository answerRepository = new AnswerRepository(context, cachePool, globalCancellation);

                string content = "content";
                var origin = new AnswerInfo()
                {
                    QuestionId = qid,
                    CreaterId = uid,
                    Content = content
                };
                int aid = await answerRepository.InsertAnswerAsync(origin);

                await answerRepository.DeleteAnswerByIdAsync(aid);
                Assert.Empty(context.Answers);

                await answerRepository.DeleteAnswerByIdAsync(aid);
                Assert.Empty(context.Answers);
            }
        }
    }
}
