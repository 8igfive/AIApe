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
using BCrypt;
using BNBCrypt = BCrypt.Net.BCrypt;

namespace AIBotTest.Repository
{
    public class UserRepositoryTest
    {
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(UserRepositoryTest)}@{count++}");
            return builder.Options;
        }

        [Fact]
        public async Task SelectUserByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            string name = "user";
            string email = "user@buaa";
            string password = "bcrypt";
            string bcrypt = BNBCrypt.HashPassword(password);
            AuthLevel auth = AuthLevel.User;
            using (var context = new DatabaseContext(options))
            {
                var user = new UserData()
                {
                    Name = name,
                    Email = email,
                    Bcrypt = bcrypt,
                    Auth = auth,
                    ProfilePhoto = 1
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
            }

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(uid, res.UserId);
                Assert.Equal(name, res.Name);
                Assert.Equal(email, res.Email);
                Assert.Equal(auth, res.Auth);
                Assert.Equal(bcrypt, res.Bcrypt);
                Assert.True(BNBCrypt.Verify(password, res.Bcrypt));
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByIdAsync(uid + 1);
                Assert.Null(res);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task SelectUserByEmailAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            string name = "user";
            string email = "user@buaa";
            string password = "bcrypt";
            string bcrypt = BNBCrypt.HashPassword(password);
            AuthLevel auth = AuthLevel.User;
            using (var context = new DatabaseContext(options))
            {
                var user = new UserData()
                {
                    Name = name,
                    Email = email,
                    Bcrypt = bcrypt,
                    Auth = auth,
                    ProfilePhoto = 1
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
            }

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByEmailAsync(email);
                Assert.Equal(uid, res.UserId);
                Assert.Equal(name, res.Name);
                Assert.Equal(email, res.Email);
                Assert.Equal(auth, res.Auth);
                Assert.Equal(bcrypt, res.Bcrypt);
                Assert.True(BNBCrypt.Verify(password, res.Bcrypt));
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByEmailAsync(email + ".edu.cn");
                Assert.Null(res);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task SelectUserByNameAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            string name = "user";
            string email = "user@buaa";
            string password = "bcrypt";
            AuthLevel auth = AuthLevel.User;
            string bcrypt = BNBCrypt.HashPassword(password);
            using (var context = new DatabaseContext(options))
            {
                var user = new UserData()
                {
                    Name = name,
                    Email = email,
                    Bcrypt = bcrypt,
                    Auth = auth,
                    ProfilePhoto = 1
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
            }

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByNameAsync(name);
                Assert.Equal(uid, res.UserId);
                Assert.Equal(name, res.Name);
                Assert.Equal(email, res.Email);
                Assert.Equal(auth, res.Auth);
                Assert.Equal(bcrypt, res.Bcrypt);
                Assert.True(BNBCrypt.Verify(password, res.Bcrypt));
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByNameAsync("Dr." + name);
                Assert.Null(res);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task SelectBcryptByEmailAsyncTest()
        {
            var options = CreateUniqueOptions();

            int uid;
            string name = "user";
            string email = "user@buaa";
            string password = "bcrypt";
            AuthLevel auth = AuthLevel.User;
            using (var context = new DatabaseContext(options))
            {
                var user = new UserData()
                {
                    Name = name,
                    Email = email,
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Auth = auth,
                    ProfilePhoto = 1
                };
                context.Add(user);
                await context.SaveChangesAsync();
                uid = user.UserId;
            }

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectBcryptByEmailAsync(email);
                Assert.True(BNBCrypt.Verify(password, res));
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var res = await userRepository.SelectUserByEmailAsync(email + ".edu.cn");
                Assert.Null(res);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task SelectQuestionsIdByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

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
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user1);
                var user2 = new UserData()
                {
                    Name = "user2",
                    Email = "user2@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user2);
                await context.SaveChangesAsync();
                uid1 = user1.UserId;
                uid2 = user2.UserId;

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid1
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = "Question2",
                    Remarks = "This is question No.2",
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
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdAsync(uid1);
                Assert.Equal(2, res.Count());
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdAsync(uid2);
                Assert.Empty(res);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdAsync(Math.Max(uid1, uid2) + 1);
                Assert.Null(res);
            }
        }

        [Fact]
        public async Task SelectQuestionsIdByIdOrderByModifyTimeAsyncTest()
        {
            var options = CreateUniqueOptions();

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
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user1);
                var user2 = new UserData()
                {
                    Name = "user2",
                    Email = "user2@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user2);
                await context.SaveChangesAsync();
                uid1 = user1.UserId;
                uid2 = user2.UserId;

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid1
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = "Question2",
                    Remarks = "This is question No.2",
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
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdOrderByModifyTimeAsync(uid1);
                Assert.Equal(2, res.Count());

                // seems not work with SQLite.InMemory
                //var first = await context.Questions.FindAsync(res.First());
                //var secont = await context.Questions.FindAsync(res.Last());
                //Assert.True(first.ModifyTime < secont.ModifyTime);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdOrderByModifyTimeAsync(uid2);
                Assert.Empty(res);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectQuestionsIdByIdOrderByModifyTimeAsync(Math.Max(uid1, uid2) + 1);
                Assert.Null(res);
            }
        }

        [Fact]
        public async Task SelectAnswersIdByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

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
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user1);
                var user2 = new UserData()
                {
                    Name = "user2",
                    Email = "user2@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user2);
                await context.SaveChangesAsync();
                uid1 = user1.UserId;
                uid2 = user2.UserId;

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid1
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = "Question2",
                    Remarks = "This is question No.2",
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
                    Content = $"answer No.2 for question2 by user1",
                    UserId = uid1,
                    QuestionId = qid2
                };
                context.Add(answer1);
                context.Add(answer2);
                await context.SaveChangesAsync();
                aid1 = answer1.AnswerId;
                aid2 = answer2.AnswerId;
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdAsync(uid1);
                Assert.Equal(2, res.Count());
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdAsync(uid2);
                Assert.Empty(res);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdAsync(Math.Max(uid1, uid2) + 1);
                Assert.Null(res);
            }
        }

        [Fact]
        public async Task SelectAnswersIdByIdByModifyTimeAsyncTest()
        {
            var options = CreateUniqueOptions();

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
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user1);
                var user2 = new UserData()
                {
                    Name = "user2",
                    Email = "user2@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                context.Add(user2);
                await context.SaveChangesAsync();
                uid1 = user1.UserId;
                uid2 = user2.UserId;

                var question1 = new QuestionData()
                {
                    Title = "Question1",
                    Remarks = "This is question No.1",
                    UserId = uid1
                };
                context.Add(question1);
                var question2 = new QuestionData()
                {
                    Title = "Question2",
                    Remarks = "This is question No.2",
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
                    Content = $"answer No.2 for question2 by user1",
                    UserId = uid1,
                    QuestionId = qid2
                };
                context.Add(answer1);
                context.Add(answer2);
                await context.SaveChangesAsync();
                aid1 = answer1.AnswerId;
                aid2 = answer2.AnswerId;
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdByModifyTimeAsync(uid1);
                Assert.Equal(2, res.Count());

                // seems not work with SQLite.InMemory
                //var first = await context.Questions.FindAsync(res.First());
                //var secont = await context.Questions.FindAsync(res.Last());
                //Assert.True(first.ModifyTime < secont.ModifyTime);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdByModifyTimeAsync(uid2);
                Assert.Empty(res);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);
                var res = await userRepository.SelectAnswersIdByIdByModifyTimeAsync(Math.Max(uid1, uid2) + 1);
                Assert.Null(res);
            }
        }

        #region InsertUserAsync

        [Fact]
        public async Task InsertUserAsync_Basic()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string password = "password";
                var user = new UserInfo()
                {
                    Email = "user@buaa",
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                int uid = await userRepository.InsertUserAsync(user);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(user.Email, res.Email);
                Assert.Equal(user.Name, res.Name);
                Assert.Equal(user.Auth, res.Auth);
                Assert.Equal(user.Bcrypt, res.Bcrypt);
                Assert.True(BNBCrypt.Verify(password, res.Bcrypt));
            }
        }

        [Fact]
        public async Task InsertUserAsync_EmailNull()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string password = "password";
                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_BcryptNull()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Email = "user@buaa",
                    Name = "user",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_NameNull()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string password = "password";
                var user = new UserInfo()
                {
                    Email = "user@buaa",
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_AuthNone()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string password = "password";
                var user = new UserInfo()
                {
                    Email = "user@buaa",
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_EmailTooLong()
        {
            var options = CreateUniqueOptions();

            string password = "password";
            var emailBuilder = new StringBuilder();
            string head = "user@";
            emailBuilder.Append(head);
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.UserEmailMaxLength - head.Length))
            {
                emailBuilder.Append('e');
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    Email = emailBuilder.ToString() + "t",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<UserEmailToLongException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    Email = emailBuilder.ToString(),
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                await userRepository.InsertUserAsync(user);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_NameTooLong()
        {
            var options = CreateUniqueOptions();

            string password = "password";
            var nameBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.UserNameMaxLength))
            {
                nameBuilder.Append('n');
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = nameBuilder.ToString() + "n",
                    Email = "user@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<UserNameToLongException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = nameBuilder.ToString(),
                    Email = "user@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                await userRepository.InsertUserAsync(user);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_WrongBrcyptLength()
        {
            var options = CreateUniqueOptions();

            string password = "password";
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password) + '=',
                    Name = "user",
                    Email = "user@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<UserBycryptLengthException>(async () =>
                    await userRepository.InsertUserAsync(user));

                Assert.Empty(context.Users);
            }
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var user = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user",
                    Email = "user@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                await userRepository.InsertUserAsync(user);
                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_EmailExist()
        {
            var options = CreateUniqueOptions();

            string password = "password";
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string email = "user@buaa";
                var first = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user1",
                    Email = email,
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                await userRepository.InsertUserAsync(first);

                Assert.Single(context.Users);

                var second = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = "user2",
                    Email = email,
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<EmailHasExistException>(async () =>
                    await userRepository.InsertUserAsync(second));

                Assert.Single(context.Users);
            }
        }

        [Fact]
        public async Task InsertUserAsync_NameExist()
        {
            var options = CreateUniqueOptions();

            string password = "password";
            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string name = "user";
                var first = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = name,
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                await userRepository.InsertUserAsync(first);

                Assert.Single(context.Users);

                var second = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword(password),
                    Name = name,
                    Email = "user2@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };

                await Assert.ThrowsAsync<NameHasExistException>(async () =>
                    await userRepository.InsertUserAsync(second));

                Assert.Single(context.Users);
            }
        }

        #endregion

        #region UpdateUserAsync

        [Fact]
        public async Task UpdateUserAsync_NameOnly()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Name = "user2",
                };
                await userRepository.UpdateUserAsync(user);
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(origin.Bcrypt, res.Bcrypt);
                Assert.Equal(origin.Email, res.Email);
                Assert.Equal(user.Name, res.Name);
                Assert.Equal(origin.Auth, res.Auth);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_EmailIgnored()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Email = "user2@buaa",
                };
                await userRepository.UpdateUserAsync(user);
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(origin.Bcrypt, res.Bcrypt);
                Assert.Equal(origin.Email, res.Email);
                Assert.Equal(origin.Name, res.Name);
                Assert.Equal(origin.Auth, res.Auth);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_AuthOnly()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Auth = AuthLevel.Admin
                };
                await userRepository.UpdateUserAsync(user);
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(origin.Bcrypt, res.Bcrypt);
                Assert.Equal(origin.Email, res.Email);
                Assert.Equal(origin.Name, res.Name);
                Assert.Equal(user.Auth, res.Auth);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_BcryptOnly()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password1"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Bcrypt = BNBCrypt.HashPassword("password2"),
                };
                await userRepository.UpdateUserAsync(user);
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(user.Bcrypt, res.Bcrypt);
                Assert.Equal(origin.Email, res.Email);
                Assert.Equal(origin.Name, res.Name);
                Assert.Equal(origin.Auth, res.Auth);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_UpdateAll()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password1"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Bcrypt = BNBCrypt.HashPassword("password2"),
                    Name = "user2",
                    Auth = AuthLevel.Admin
                };
                await userRepository.UpdateUserAsync(user);
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(user.Bcrypt, res.Bcrypt);
                Assert.Equal(origin.Email, res.Email);
                Assert.Equal(user.Name, res.Name);
                Assert.Equal(user.Auth, res.Auth);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_NameTooLong()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = "user1",
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var nameBuilder = new StringBuilder();
                foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.UserNameMaxLength))
                {
                    nameBuilder.Append('n');
                }
                var maxLengthNameUser = new UserInfo()
                {
                    UserId = uid,
                    Name = nameBuilder.ToString(),
                };
                await userRepository.UpdateUserAsync(maxLengthNameUser);
                Assert.Single(context.Users);

                var res1 = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(maxLengthNameUser.Name, res1.Name);

                nameBuilder.Append('n');
                var tooLongNameUser = new UserInfo()
                {
                    UserId = uid,
                    Name = nameBuilder.ToString(),
                };
                await Assert.ThrowsAsync<UserNameToLongException>(async () =>
                    await userRepository.UpdateUserAsync(tooLongNameUser));
                Assert.Single(context.Users);

                var res2 = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(maxLengthNameUser.Name, res2.Name);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_NameEixst()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string name1 = "user";
                var origin1 = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = name1,
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid1 = await userRepository.InsertUserAsync(origin1);

                string name2 = "user2";
                var origin2 = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = name2,
                    Email = "user2@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid2 = await userRepository.InsertUserAsync(origin2);

                var user = new UserInfo()
                {
                    UserId = uid2,
                    Name = name1,
                };
                await Assert.ThrowsAsync<NameHasExistException>(async () =>
                    await userRepository.UpdateUserAsync(user));

                var res = await userRepository.SelectUserByIdAsync(uid2);
                Assert.Equal(origin2.Name, res.Name);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_UserNotEixst()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string name = "user";
                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = name,
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid + 1,
                    Name = name,
                };
                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await userRepository.UpdateUserAsync(user));
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(origin.Name, res.Name);
            }
        }

        [Fact]
        public async Task UpdateUserAsync_WrongBcryptLength()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string name = "user";
                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = name,
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                var user = new UserInfo()
                {
                    UserId = uid,
                    Bcrypt = BNBCrypt.HashPassword("password") + '=',
                };
                await Assert.ThrowsAsync<UserBycryptLengthException>(async () =>
                  await userRepository.UpdateUserAsync(user));
                Assert.Single(context.Users);

                var res = await userRepository.SelectUserByIdAsync(uid);
                Assert.Equal(origin.Bcrypt, res.Bcrypt);
            }
        }

        #endregion

        [Fact]
        public async Task DeleteUserByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                IUserRepository userRepository = new UserRepository(context, cachePool, globalCancellation);

                string name = "user";
                var origin = new UserInfo()
                {
                    Bcrypt = BNBCrypt.HashPassword("password"),
                    Name = name,
                    Email = "user1@buaa",
                    Auth = AuthLevel.User,
                    ProfilePhoto = 1
                };
                int uid = await userRepository.InsertUserAsync(origin);
                Assert.Single(context.Users);

                await userRepository.DeleteUserByIdAsync(uid);
                Assert.Empty(context.Users);

                await userRepository.DeleteUserByIdAsync(uid);
                Assert.Empty(context.Users);
            }
        }
    }
}
