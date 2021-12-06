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
    public class LikeRepositoryTest
    {
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(LikeRepositoryTest)}@{count++}");
            return builder.Options;
        }

        [Fact]
        public async Task SelectLikedQuestionsForUserAsyncTest()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid1;
            int qid2;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var question2 = new QuestionData()
                {
                    User = user.First(),
                    Title = "title2",
                    Remarks = "remarks2",
                };
                context.Add(question2);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question2
                });
                await context.SaveChangesAsync();
                qid1 = question.QuestionId;
                qid2 = question2.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.True((await likeRepository.SelectLikedQuestionsForUserAsync(uids[0]))
                    .OrderBy(q => q).SequenceEqual(new int[] { qid1, qid2 }.OrderBy(q => q)));

                Assert.Null(await likeRepository.SelectLikedQuestionsForUserAsync(uids.Max() + 1));
            }
        }

        [Fact]
        public async Task SelectLikedAnswersForUserAsyncTest()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid1;
            int aid2;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content2",
                };
                context.Add(answer);

                var answer2 = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content2",
                };
                context.Add(answer2);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer2
                });
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid1 = answer.AnswerId;
                aid2 = answer2.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.True((await likeRepository.SelectLikedAnswersForUserAsync(uids[0]))
                    .OrderBy(q => q).SequenceEqual(new int[] { aid1, aid2 }.OrderBy(q => q)));

                Assert.Null(await likeRepository.SelectLikedAnswersForUserAsync(uids.Max() + 1));
            }
        }

        #region SelectLikesCountForQuestionAsync

        [Fact]
        public async Task SelectLikesCountForQuestionAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                context.Add(new LikeQuestion()
                {
                    User = user[1],
                    Question = question
                });
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.Equal(2, await likeRepository.SelectLikesCountForQuestionAsync(qid));
            }
        }

        [Fact]
        public async Task SelectLikesCountForQuestionAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                context.Add(new LikeQuestion()
                {
                    User = user[1],
                    Question = question
                });
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await likeRepository.SelectLikesCountForQuestionAsync(qid + 1));
            }
        }

        #endregion

        #region SelectLikesCountForAnswerAsync

        [Fact]
        public async Task SelectLikesCountForAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                context.Add(new LikeAnswer()
                {
                    User = user[1],
                    Answer = answer
                });
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.Equal(2, await likeRepository.SelectLikesCountForAnswerAsync(aid));
            }
        }

        [Fact]
        public async Task SelectLikesCountForAnswerAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                context.Add(new LikeAnswer()
                {
                    User = user[1],
                    Answer = answer
                });
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
                    await likeRepository.SelectLikesCountForAnswerAsync(aid + 1));
            }
        }

        #endregion

        #region UserLikedQuestionAsync

        [Fact]
        public async Task UserLikedQuestionAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.True(await likeRepository.UserLikedQuestionAsync(uids[0], qid));
                Assert.False(await likeRepository.UserLikedQuestionAsync(uids[1], qid));
            }
        }

        [Fact]
        public async Task UserLikedQuestionAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.UserLikedQuestionAsync(uids.Max() + 1, qid));
            }
        }

        [Fact]
        public async Task UserLikedQuestionAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await likeRepository.UserLikedQuestionAsync(uids[0], qid + 1));
            }
        }

        #endregion

        #region UserLikedAnswerAsync

        [Fact]
        public async Task UserLikedAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                Assert.True(await likeRepository.UserLikedAnswerAsync(uids[0], aid));
                Assert.False(await likeRepository.UserLikedAnswerAsync(uids[2], aid));
            }
        }

        [Fact]
        public async Task UserLikedAnswerAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
                    await likeRepository.UserLikedAnswerAsync(uids[0], aid + 1));
            }
        }

        [Fact]
        public async Task UserLikedAnswerAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.UserLikedAnswerAsync(uids.Max() + 1, aid));
            }
        }

        #endregion

        #region InsertLikeForQuestionAsync

        [Fact]
        public async Task InsertLikeForQuestionAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await likeRepository.InsertLikeForQuestionAsync(uids[0], qid);

                Assert.True(await likeRepository.UserLikedQuestionAsync(uids[0], qid));
                Assert.False(await likeRepository.UserLikedQuestionAsync(uids[1], qid));
            }
        }

        [Fact]
        public async Task InsertLikeForQuestionAsync_HasLiked()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await likeRepository.InsertLikeForQuestionAsync(uids[0], qid);

                await Assert.ThrowsAsync<UserHasLikedTargetException>(async () =>
                    await likeRepository.InsertLikeForQuestionAsync(uids[0], qid));

                Assert.Equal(1, await likeRepository.SelectLikesCountForQuestionAsync(qid));
            }
        }

        [Fact]
        public async Task InsertLikeForQuestionAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.InsertLikeForQuestionAsync(uids.Max() + 1, qid));

                Assert.Equal(0, await likeRepository.SelectLikesCountForQuestionAsync(qid));
                foreach (int uid in uids)
                {
                    Assert.Empty((await likeRepository.SelectLikedQuestionsForUserAsync(uid)));
                }
            }
        }

        [Fact]
        public async Task InsertLikeForQuestionAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await likeRepository.InsertLikeForQuestionAsync(uids[0], qid + 1));

                Assert.Equal(0, await likeRepository.SelectLikesCountForQuestionAsync(qid));
                foreach (int uid in uids)
                {
                    Assert.Empty((await likeRepository.SelectLikedQuestionsForUserAsync(uid)));
                }
            }
        }

        #endregion

        #region InsertLikeForAnswerAsync

        [Fact]
        public async Task InsertLikeForAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await likeRepository.InsertLikeForAnswerAsync(uids[0], aid);

                Assert.True(await likeRepository.UserLikedAnswerAsync(uids[0], aid));
                Assert.False(await likeRepository.UserLikedAnswerAsync(uids[1], aid));
            }
        }

        [Fact]
        public async Task InsertLikeForAnswerAsync_HasLiked()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await likeRepository.InsertLikeForAnswerAsync(uids[0], aid);

                await Assert.ThrowsAsync<UserHasLikedTargetException>(async () =>
                    await likeRepository.InsertLikeForAnswerAsync(uids[0], aid));

                Assert.Equal(1, await likeRepository.SelectLikesCountForAnswerAsync(aid));
            }
        }

        [Fact]
        public async Task InsertLikeForAnswerAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.InsertLikeForAnswerAsync(uids.Max() + 1, aid));

                Assert.Equal(0, await likeRepository.SelectLikesCountForAnswerAsync(aid));
                foreach (int uid in uids)
                {
                    Assert.Empty((await likeRepository.SelectLikedAnswersForUserAsync(uid)));
                }
            }
        }

        [Fact]
        public async Task InsertLikeForAnswerAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);
                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
                    await likeRepository.InsertLikeForAnswerAsync(uids[0], aid + 1));

                Assert.Equal(0, await likeRepository.SelectLikesCountForAnswerAsync(aid));
                foreach (int uid in uids)
                {
                    Assert.Empty((await likeRepository.SelectLikedAnswersForUserAsync(uid)));
                }
            }
        }

        #endregion

        #region DeleteLikeForQuestionAsync

        [Fact]
        public async Task DeleteLikeForQuestionAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);


                Assert.True(await likeRepository.UserLikedQuestionAsync(uids[0], qid));
                await likeRepository.DeleteLikeForQuestionAsync(uids[0], qid);
                Assert.False(await likeRepository.UserLikedQuestionAsync(uids[0], qid));
            }
        }

        [Fact]
        public async Task DeleteLikeForQuestionAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.DeleteLikeForQuestionAsync(uids.Max() + 1, qid));
                Assert.Equal(1, await likeRepository.SelectLikesCountForQuestionAsync(qid));
            }
        }

        [Fact]
        public async Task DeleteLikeForQuestionAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<QuestionNotExistException>(async () =>
                    await likeRepository.DeleteLikeForQuestionAsync(uids[0], qid + 1));
                Assert.Equal(1, await likeRepository.SelectLikesCountForQuestionAsync(qid));
            }
        }

        [Fact]
        public async Task DeleteLikeForQuestionAsync_NotLike()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeQuestion()
                {
                    User = user[0],
                    Question = question
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotLikedTargetException>(async () =>
                    await likeRepository.DeleteLikeForQuestionAsync(uids[1], qid));
                Assert.Equal(1, await likeRepository.SelectLikesCountForQuestionAsync(qid));
            }
        }

        #endregion

        #region DeleteLikeFroAnswerAsync

        [Fact]
        public async Task DeleteLikeFroAnswerAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);


                Assert.True(await likeRepository.UserLikedAnswerAsync(uids[0], aid));
                await likeRepository.DeleteLikeFroAnswerAsync(uids[0], aid);
                Assert.False(await likeRepository.UserLikedAnswerAsync(uids[0], aid));
            }
        }

        [Fact]
        public async Task DeleteLikeFroAnswerAsync_UserNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotExistException>(async () =>
                    await likeRepository.DeleteLikeFroAnswerAsync(uids.Max() + 1, aid));
                Assert.Equal(1, await likeRepository.SelectLikesCountForAnswerAsync(aid));
            }
        }

        [Fact]
        public async Task DeleteLikeFroAnswerAsync_QuestionNotExist()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<AnswerNotExistException>(async () =>
                    await likeRepository.DeleteLikeFroAnswerAsync(uids[0], aid + 1));
                Assert.Equal(1, await likeRepository.SelectLikesCountForAnswerAsync(aid));
            }
        }

        [Fact]
        public async Task DeleteLikeFroAnswerAsync_NotLike()
        {
            var options = CreateUniqueOptions();

            int[] uids;
            int qid;
            int aid;
            int userCount = 3;
            using (var context = new DatabaseContext(options))
            {
                var user = Enumerable.Range(0, userCount).Select(i => new UserData()
                {
                    Name = $"user{i}",
                    Email = $"user{i}@buaa",
                    Bcrypt = "bcrypt",
                    Auth = AuthLevel.User
                }).ToArray();
                context.AddRange(user);

                var question = new QuestionData()
                {
                    User = user.First(),
                    Title = "title",
                    Remarks = "remarks",
                };
                context.Add(question);

                var answer = new AnswerData()
                {
                    User = user.Last(),
                    Question = question,
                    Content = "content",
                };
                context.Add(answer);

                context.Add(new LikeAnswer()
                {
                    User = user[0],
                    Answer = answer
                });

                await context.SaveChangesAsync();
                qid = question.QuestionId;
                uids = user.Select(u => u.UserId).ToArray();
                aid = answer.AnswerId;
            }

            using (var context = new DatabaseContext(options))
            {
                ILikeRepository likeRepository = new LikeRepository(context, cachePool, globalCancellation);

                await Assert.ThrowsAsync<UserNotLikedTargetException>(async () =>
                    await likeRepository.DeleteLikeFroAnswerAsync(uids[1], aid));
                Assert.Equal(1, await likeRepository.SelectLikesCountForAnswerAsync(qid));
            }
        }
        #endregion
    }
}
