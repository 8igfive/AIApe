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
using Buaa.AIBot.Bot.WorkingModule;
using Buaa.AIBot.Services;
using Moq;


namespace AIBotTest.Bot.WorkingModule
{
    public class NaturalAnswerGeneratorTest
    {
        private Mock<INLPService> nlpMock = new Mock<INLPService>();
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();
        private List<Tuple<int, string>> nlpAddeds = new List<Tuple<int, string>>();


        public NaturalAnswerGeneratorTest()
        {
            nlpMock
                .Setup(nlp => nlp.AddAsync(It.IsAny<int>(), It.IsAny<string>(), NLPService.Languages.Natrual))
                .ReturnsAsync(new double[0])
                .Callback((int qid, string question, NLPService.Languages lang) => nlpAddeds.Add(new Tuple<int, string>(qid, question)))
                ;
        }

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(NaturalAnswerGeneratorTest)}@{count++}");
            return builder.Options;
        }

        private NaturalAnswerGenerator CreateInstance(DatabaseContext context)
        {
            var gcts = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();
            gcts.Source = new System.Threading.CancellationTokenSource();
            return new NaturalAnswerGenerator(context, nlpMock.Object, gcts, null);
        }

        [Fact]
        public async Task AddQuestionAndAnswersAsyncTest()
        {
            var options = CreateUniqueOptions();

            var questions = Enumerable.Range(0, 5).Select(i => $"question{i}").ToList();
            var answers = Enumerable.Range(0, 10).Select(i => $"answer{i}").ToList();
            using (var context = new DatabaseContext(options))
            {
                var generator = CreateInstance(context);
                await generator.AddQuestionAndAnswersAsync(questions, answers);
            }


            using (var context = new DatabaseContext(options))
            {
                var naids = await context.NatrualAnswers
                    .Select(na => na.NatrualAnswerId)
                    .ToListAsync();
                var nqids = await context.NatrualQuestions
                    .Select(nq => nq.NaturalQuestionId)
                    .ToListAsync();
                var naqs = await context.NatrualQuestionAnswerRelations
                    .Select(nqar => new Tuple<int, int>(nqar.NaturalQuestionId, nqar.NatrualAnswerId))
                    .ToListAsync();
                var query = from a in naids from q in nqids select new Tuple<int, int>(q, a);
                Assert.True(query.OrderBy(t => t.Item1).ThenBy(t => t.Item2).SequenceEqual(naqs.OrderBy(t => t.Item1).ThenBy(t => t.Item2)));
            }

            Assert.Equal(5, nlpAddeds.Count);

            foreach (var t in nlpAddeds)
            {
                Assert.True(t.Item1 < 0);
            }
        }

        [Theory]
        [InlineData("[Sad]")]
        [InlineData("[Cute]")]
        [InlineData("[Happy]")]
        [InlineData("[call date]")]
        [InlineData("[call weather]")]
        public async Task GetAnswerAsync_Scripts(string answer)
        {
            var options = CreateUniqueOptions();

            var questions = new string[] { "question" };
            var answers = new string[] { answer };
            int qid;
            using (var context = new DatabaseContext(options))
            {
                var generator = CreateInstance(context);
                await generator.AddQuestionAndAnswersAsync(questions, answers);
                qid = await context.NatrualQuestions.Select(nq => nq.NaturalQuestionId).FirstAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                var generator = CreateInstance(context);

                var res = await generator.GetAnswerAsync(qid);
                Assert.NotEqual(answer, res);
            }
        }

        [Fact]
        public async Task GetAnswerAsync_NegativeQid()
        {
            var options = CreateUniqueOptions();

            var questions = new string[] { "question" };
            var answers = new string[] { "answer" };
            int qid;
            using (var context = new DatabaseContext(options))
            {
                var generator = CreateInstance(context);
                await generator.AddQuestionAndAnswersAsync(questions, answers);
                qid = await context.NatrualQuestions.Select(nq => nq.NaturalQuestionId).FirstAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                var generator = CreateInstance(context);

                var res = await generator.GetAnswerAsync(-qid);
                Assert.Equal("answer", res);
            }
        }
    }
}
