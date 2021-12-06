using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Buaa.AIBot.Services.Models;
using Buaa.AIBot.Services.Exceptions;
using Buaa.AIBot.Services;
using Buaa.AIBot.Repository;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Repository.Models;

namespace AIBotTest.Services
{
    public class QuestionServiceTest
    {
        private Mock<IQuestionRepository> queMock = new Mock<IQuestionRepository>();
        private Mock<IAnswerRepository> ansMock = new Mock<IAnswerRepository>();
        private Mock<ITagRepostory> tagMock = new Mock<ITagRepostory>();
        private Mock<ILikeRepository> likeMock = new Mock<ILikeRepository>();
        private Mock<INLPService> nlpMock = new Mock<INLPService>();

        private IQuestionService CreateQuestionService()
            => new QuestionService(queMock.Object, ansMock.Object, tagMock.Object, likeMock.Object, nlpMock.Object);

        #region GetQuestionAsync

        [Fact]
        public async Task GetQuestionAsync_Basic()
        {
            var questionInfo = new QuestionInfo()
            {
                QuestionId = 1,
                Title = "title",
                Remarks = "remarks",
                CreaterId = 1,
                CreateTime = DateTime.Now - TimeSpan.FromDays(1),
                ModifyTime = DateTime.Now - TimeSpan.FromHours(1),
            };
            var tagDict = new Dictionary<string, int>() { ["tag1"] = 1, ["tag3"] = 3 };
            var answerList = new List<int>() { 3, 5, 7 };
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(questionInfo);
            queMock.
                Setup(qr => qr.SelectTagsForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(tagDict);
            queMock
                .Setup(qr => qr.SelectAnswersForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(answerList);

            var questionService = CreateQuestionService();

            var res = await questionService.GetQuestionAsync(1);

            Assert.Equal(questionInfo.Title, res.Title);
            Assert.Equal(questionInfo.Remarks, res.Remarks);
            Assert.Equal(questionInfo.CreaterId, res.Creator);
            Assert.Equal(questionInfo.CreateTime, res.CreateTime);
            Assert.Equal(questionInfo.ModifyTime, res.ModifyTime);
            Assert.True(answerList.OrderBy(a => a).SequenceEqual(res.Answers.OrderBy(a => a)));
            Assert.True(tagDict.OrderBy(kv => kv.Value).SequenceEqual(res.Tags.OrderBy(kv => kv.Value)));

            queMock.Verify();
        }

        [Fact]
        public async Task GetQuestionAsync_QuestionNotExist1()
        {
            var questionInfo = new QuestionInfo()
            {
                QuestionId = 1,
                Title = "title",
                Remarks = "remarks",
                CreaterId = 1,
                CreateTime = DateTime.Now - TimeSpan.FromDays(1),
                ModifyTime = DateTime.Now - TimeSpan.FromHours(1),
            };
            var tagDict = new Dictionary<string, int>() { ["tag1"] = 1, ["tag3"] = 3 };
            var answerList = new List<int>() { 3, 5, 7 };
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(() => null);
            queMock
                .Setup(qr => qr.SelectTagsForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(tagDict);
            queMock
                .Setup(qr => qr.SelectAnswersForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(answerList);

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionService.GetQuestionAsync(1));
        }

        [Fact]
        public async Task GetQuestionAsync_QuestionNotExist2()
        {
            var questionInfo = new QuestionInfo()
            {
                QuestionId = 1,
                Title = "title",
                Remarks = "remarks",
                CreaterId = 1,
                CreateTime = DateTime.Now - TimeSpan.FromDays(1),
                ModifyTime = DateTime.Now - TimeSpan.FromHours(1),
            };
            var tagDict = new Dictionary<string, int>() { ["tag1"] = 1, ["tag3"] = 3 };
            var answerList = new List<int>() { 3, 5, 7 };
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(questionInfo);
            queMock
                .Setup(qr => qr.SelectTagsForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(() => null);
            queMock
                .Setup(qr => qr.SelectAnswersForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(answerList);

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionService.GetQuestionAsync(1));
        }

        [Fact]
        public async Task GetQuestionAsync_QuestionNotExist3()
        {
            var questionInfo = new QuestionInfo()
            {
                QuestionId = 1,
                Title = "title",
                Remarks = "remarks",
                CreaterId = 1,
                CreateTime = DateTime.Now - TimeSpan.FromDays(1),
                ModifyTime = DateTime.Now - TimeSpan.FromHours(1),
            };
            var tagDict = new Dictionary<string, int>() { ["tag1"] = 1, ["tag3"] = 3 };
            var answerList = new List<int>() { 3, 5, 7 };
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(questionInfo);
            queMock
                .Setup(qr => qr.SelectTagsForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(tagDict);
            queMock
                .Setup(qr => qr.SelectAnswersForQuestionByIdAsync(questionInfo.QuestionId))
                .ReturnsAsync(() => null);

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionService.GetQuestionAsync(1));
        }

        #endregion

        [Fact]
        public async Task GetAnswerAsyncTest()
        {
            var answerInfo = new AnswerInfo()
            {
                AnswerId = 1,
                CreaterId = 1,
                QuestionId = 1,
                Content = "content",
                CreateTime = DateTime.Now - TimeSpan.FromDays(1),
                ModifyTime = DateTime.Now - TimeSpan.FromHours(1),
            };
            ansMock
                .Setup(ar => ar.SelectAnswerByIdAsync(answerInfo.AnswerId))
                .ReturnsAsync(answerInfo);
            ansMock
                .Setup(ar => ar.SelectAnswerByIdAsync(It.Is<int>(v => v != answerInfo.AnswerId)))
                .ReturnsAsync(() => null);

            var questionService = CreateQuestionService();

            var res = await questionService.GetAnswerAsync(1);
            Assert.Equal(answerInfo.Content, res.Content);
            Assert.Equal(answerInfo.CreaterId, res.Creator);
            Assert.Equal(answerInfo.CreateTime, res.CreateTime);
            Assert.Equal(answerInfo.ModifyTime, res.ModifyTime);

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.AnswerNotExistException>(async () =>
                await questionService.GetAnswerAsync(2));

            queMock.Verify();
        }

        [Fact]
        public async Task GetTagAsyncTest()
        {
            var tagInfo = new TagInfo()
            {
                TagId = 1,
                Name = "tag",
                Desc = "desc",
                Category = TagCategory.Other
            };
            tagMock
                .Setup(tr => tr.SelectTagByIdAsync(tagInfo.TagId))
                .ReturnsAsync(tagInfo);
            tagMock
                .Setup(tr => tr.SelectTagByIdAsync(It.Is<int>(tid => tid != tagInfo.TagId)))
                .ReturnsAsync(() => null);

            var questionService = CreateQuestionService();

            var res = await questionService.GetTagAsync(1);
            Assert.Equal(tagInfo.Name, res.Name);
            Assert.Equal(tagInfo.Desc, res.Desc);

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNotExistException>(async () =>
                await questionService.GetTagAsync(2));

            tagMock.Verify();
        }

        #region GetQuestionListAsync

        //[Fact]
        //public async Task GetQuestionListAsync_NoPt()
        //{
        //    IEnumerable<int> tags = new List<int>() { 1, 3, 2, 4 };
        //    int maxLimit = Buaa.AIBot.Constants.QuestionListMaxNumber;
        //    int maxQid = maxLimit * 4;
        //    IEnumerable<int> qids = Enumerable.Range(1, maxQid).ToList();
        //    queMock
        //        .Setup(qr => qr.SelectQuestionsByTagsAsync(tags, int.MaxValue, maxLimit - 1))
        //        .ReturnsAsync(qids.Take(maxLimit - 1));
        //    queMock
        //        .Setup(qr => qr.SelectQuestionsByTagsAsync(tags, int.MaxValue, It.Is<int>(i => i >= maxLimit)))
        //        .ReturnsAsync(qids.Take(maxLimit));

        //    var questionService = CreateQuestionService();

        //    var res1 = await questionService.GetQuestionListAsync(tags, null, maxLimit - 1);
        //    var list1 = res1.ToList();
        //    Assert.Equal(maxLimit - 1, list1.Count);
        //    Assert.True(Enumerable.Range(0, list1.Count).Select(i => maxQid - i).SequenceEqual(list1));

        //    var res2 = await questionService.GetQuestionListAsync(tags, null, maxLimit + 1);
        //    var list2 = res2.ToList();
        //    Assert.Equal(maxLimit, list2.Count);
        //    Assert.True(Enumerable.Range(0, list2.Count).Select(i => maxQid - i).SequenceEqual(list2));
        //}

        //[Fact]
        //public async Task GetQuestionListAsync_WithPt()
        //{
        //    IEnumerable<int> tags = new List<int>() { 1, 3, 2, 4 };
        //    int maxLimit = Buaa.AIBot.Constants.QuestionListMaxNumber;
        //    int maxQid = maxLimit * 4;
        //    IEnumerable<int> qids = Enumerable.Range(1, maxQid).ToList();
        //    queMock
        //        .Setup(qr => qr.SelectQuestionsByTagsAsync(tags, int.MaxValue, maxLimit - 1))
        //        .ReturnsAsync(qids.Take(maxLimit - 1)); ;
        //    queMock
        //        .Setup(qr => qr.SelectQuestionsByTagsAsync(tags, int.MaxValue, It.Is<int>(i => i >= maxLimit)))
        //        .ReturnsAsync(qids.Take(maxLimit)); ;

        //    var questionService = CreateQuestionService();

        //    int start = maxQid / 3;
        //    var res1 = await questionService.GetQuestionListAsync(tags, start + 1, maxLimit - 1);
        //    var list1 = res1.ToList();
        //    Assert.Equal(maxLimit - 1, list1.Count);
        //    Assert.True(Enumerable.Range(0, list1.Count).Select(i => start - i).SequenceEqual(list1));

        //    var res2 = await questionService.GetQuestionListAsync(tags, start + 1, maxLimit + 1);
        //    var list2 = res2.ToList();
        //    Assert.Equal(maxLimit, list2.Count);
        //    Assert.True(Enumerable.Range(0, list2.Count).Select(i => start - i).SequenceEqual(list2));

        //    int newStart = maxQid / 5;
        //    var res3 = await questionService.GetQuestionListAsync(tags, newStart + 1, maxLimit + 1);
        //    var list3 = res3.ToList();
        //    Assert.Equal(newStart, list3.Count);
        //    Assert.True(Enumerable.Range(0, list3.Count).Select(i => newStart - i).SequenceEqual(list3));
        //}

        [Fact]
        public async Task GetQuestionListAsync_ZeroNumber()
        {
            IEnumerable<int> tags = new List<int>() { 1, 3, 2, 4 };

            var questionService = CreateQuestionService();

            var res1 = await questionService.GetQuestionListAsync(tags, null, 0);
            Assert.Empty(res1);

            var res2 = await questionService.GetQuestionListAsync(tags, null, -1);
            Assert.Empty(res2);
        }

        #endregion

        [Fact]
        public async Task GetTagListAsyncTest()
        {
            var tags = new Dictionary<string, int>(
                Enumerable.Range(1, 128)
                .Select(i => new KeyValuePair<string, int>($"tag{i}", i)));
            tagMock
                .Setup(tr => tr.SelectAllTagsAsync())
                .ReturnsAsync(tags);

            var questionServeice = CreateQuestionService();

            var res = await questionServeice.GetTagListAsync();
            Assert.True(res.OrderBy(kv => kv.Value).SequenceEqual(tags.OrderBy(kv => kv.Value)));
        }
        
        private class Result<T>
        {
            public T Value { get; set; }
        }

        #region AddQuestionAsync

        [Fact]
        public async Task AddQuestionAsync_Basic()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int uid = 3;
            int qid = 5;
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.IsAny<QuestionWithListTag>()))
                .Callback((QuestionWithListTag q) => { res.Value = q; })
                .ReturnsAsync(5);

            var questionService = CreateQuestionService();

            string title = "title";
            string remarks = "remarks";
            IEnumerable<int> tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList();
            var ret = await questionService.AddQuestionAsync(uid, title, remarks, tags);

            Assert.Equal(qid, ret);
            queMock.Verify();
            Assert.Equal(title, res.Value.Title);
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(remarks, res.Value.Remarks);
            Assert.True(tags.OrderBy(t => t).SequenceEqual(res.Value.Tags.OrderBy(t => t)));
        }

        [Fact]
        public async Task AddQuestionAsync_TitleTooLong()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int uid = 3;
            int qid = 5;
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.IsAny<QuestionWithListTag>()))
                .Callback((QuestionWithListTag q) => { res.Value = q; })
                .ReturnsAsync(5);

            var questionService = CreateQuestionService();

            var titleBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.QuestionTitleMaxLength))
            {
                titleBuilder.Append('t');
            }

            string title = titleBuilder.ToString();
            string remarks = "remarks";
            IEnumerable<int> tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList();
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionTitleTooLongException>(async () =>
                await questionService.AddQuestionAsync(uid, title + 't', remarks, tags));

            Assert.Null(res.Value);

            var ret = await questionService.AddQuestionAsync(uid, title, remarks, tags);

            Assert.Equal(qid, ret);
            queMock.Verify();
            Assert.Equal(title, res.Value.Title);
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(remarks, res.Value.Remarks);
            Assert.True(tags.OrderBy(t => t).SequenceEqual(res.Value.Tags.OrderBy(t => t)));
        }

        [Fact]
        public async Task AddQuestionAsync_UserNotExist()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int uid = 3;
            int qid = 5;
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.Is<QuestionWithListTag>(q => q.CreaterId != uid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.UserNotExistException(uid));
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.Is<QuestionWithListTag>(q => q.CreaterId == uid)))
                .Callback((QuestionWithListTag q) => { res.Value = q; })
                .ReturnsAsync(5);

            var questionService = CreateQuestionService();


            string title = "title";
            string remarks = "remarks";
            IEnumerable<int> tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList();
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.UserNotExistException>(async () =>
                await questionService.AddQuestionAsync(uid + 1, title, remarks, tags));

            Assert.Null(res.Value);

            var ret = await questionService.AddQuestionAsync(uid, title, remarks, tags);

            Assert.Equal(qid, ret);
            queMock.Verify();
            Assert.Equal(title, res.Value.Title);
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(remarks, res.Value.Remarks);
            Assert.True(tags.OrderBy(t => t).SequenceEqual(res.Value.Tags.OrderBy(t => t)));
        }

        [Fact]
        public async Task AddQuestionAsync_TagNotExist()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int uid = 3;
            int qid = 5;
            int tidNotExist = 4;
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.Is<QuestionWithListTag>(q => q.Tags.Contains(tidNotExist))))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNotExistException(uid));
            queMock
                .Setup(qr => qr.InsertQuestionAsync(It.Is<QuestionWithListTag>(q => !q.Tags.Contains(tidNotExist))))
                .Callback((QuestionWithListTag q) => { res.Value = q; })
                .ReturnsAsync(5);

            var questionService = CreateQuestionService();


            string title = "title";
            string remarks = "remarks";
            IEnumerable<int> tags = Enumerable.Range(0, 13)
                .Select(i => i << 2).Where(i => i != tidNotExist).ToList();
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNotExistException>(async () =>
                await questionService.AddQuestionAsync(uid, title, remarks, tags.Append(tidNotExist)));

            Assert.Null(res.Value);

            var ret = await questionService.AddQuestionAsync(uid, title, remarks, tags);

            Assert.Equal(qid, ret);
            queMock.Verify();
            Assert.Equal(title, res.Value.Title);
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(remarks, res.Value.Remarks);
            Assert.True(tags.OrderBy(t => t).SequenceEqual(res.Value.Tags.OrderBy(t => t)));
        }

        #endregion

        #region AddAnswerAsync

        [Fact]
        public async Task AddAnswerAsync_Basic()
        {
            Result<AnswerInfo> res = new Result<AnswerInfo>();
            int uid = 2;
            int qid = 3;
            int aid = 4;
            string content = "content";
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.IsAny<AnswerInfo>()))
                .Callback((AnswerInfo a) => res.Value = a)
                .ReturnsAsync(aid);

            var questionService = CreateQuestionService();

            int ret = await questionService.AddAnswerAsync(uid, qid, content);

            Assert.Equal(aid, ret);
            ansMock.Verify();
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(content, res.Value.Content);
        }

        [Fact]
        public async Task AddAnswerAsync_Answered()
        {
            Result<AnswerInfo> res = new Result<AnswerInfo>();
            int uid = 2;
            int qid = 3;
            int aid = 4;
            string content = "content";
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.CreaterId == uid && a.QuestionId == qid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.UserHasAnswerTheQuestionException(uid, qid));
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.CreaterId != uid || a.QuestionId != qid)))
                .Callback((AnswerInfo a) => res.Value = a)
                .ReturnsAsync(aid);

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.UserHasAnswerTheQuestionException>(async () =>
                await questionService.AddAnswerAsync(uid, qid, content));

            Assert.Null(res.Value);

            int ret = await questionService.AddAnswerAsync(uid + 1, qid, content);

            Assert.Equal(aid, ret);
            ansMock.Verify();
            Assert.Equal(uid + 1, res.Value.CreaterId);
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(content, res.Value.Content);
        }

        [Fact]
        public async Task AddAnswerAsync_UserNotExist()
        {
            Result<AnswerInfo> res = new Result<AnswerInfo>();
            int uid = 2;
            int qid = 3;
            int aid = 4;
            string content = "content";
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.CreaterId == uid)))
                .Callback((AnswerInfo a) => res.Value = a)
                .ReturnsAsync(aid);
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.CreaterId != uid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.UserNotExistException(uid));

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.UserNotExistException>(async () =>
                await questionService.AddAnswerAsync(uid + 1, qid, content));

            Assert.Null(res.Value);

            int ret = await questionService.AddAnswerAsync(uid, qid, content);

            Assert.Equal(aid, ret);
            ansMock.Verify();
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(content, res.Value.Content);
        }

        [Fact]
        public async Task AddAnswerAsync_QuestionNotExist()
        {
            Result<AnswerInfo> res = new Result<AnswerInfo>();
            int uid = 2;
            int qid = 3;
            int aid = 4;
            string content = "content";
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.QuestionId == qid)))
                .Callback((AnswerInfo a) => res.Value = a)
                .ReturnsAsync(aid);
            ansMock
                .Setup(ar => ar.InsertAnswerAsync(It.Is<AnswerInfo>(a => a.QuestionId != qid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.QuestionNotExistException(uid));

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionService.AddAnswerAsync(uid, qid + 1, content));

            Assert.Null(res.Value);

            int ret = await questionService.AddAnswerAsync(uid, qid, content);

            Assert.Equal(aid, ret);
            ansMock.Verify();
            Assert.Equal(uid, res.Value.CreaterId);
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(content, res.Value.Content);
        }

        #endregion

        #region AddTagAsync

        [Fact]
        public async Task AddTagAsync_Basic()
        {
            Result<TagInfo> res = new Result<TagInfo>();
            int tid = 2;
            tagMock
                .Setup(tr => tr.InsertTagAsync(It.IsAny<TagInfo>()))
                .Callback((TagInfo t) => res.Value = t)
                .ReturnsAsync(tid);

            string name = "tag";
            string desc = "desc";

            var questionService = CreateQuestionService();

            int ret = await questionService.AddTagAsync(name, desc, "Other");
            Assert.Equal(tid, ret);
            tagMock.Verify();
            Assert.Equal(name, res.Value.Name);
            Assert.Equal(desc, res.Value.Desc);
        }

        [Fact]
        public async Task AddTagAsync_NameTooLong()
        {
            Result<TagInfo> res = new Result<TagInfo>();
            int tid = 2;
            tagMock
                .Setup(tr => tr.InsertTagAsync(It.IsAny<TagInfo>()))
                .Callback((TagInfo t) => res.Value = t)
                .ReturnsAsync(tid);

            var nameBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.TagNameMaxLength))
            {
                nameBuilder.Append('t');
            }

            string name = nameBuilder.ToString();
            string desc = "desc";

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNameTooLongException>(async () =>
                await questionService.AddTagAsync(name + 't', desc, "Other"));
            Assert.Null(res.Value);

            int ret = await questionService.AddTagAsync(name, desc, "Other");
            Assert.Equal(tid, ret);
            tagMock.Verify();
            Assert.Equal(name, res.Value.Name);
            Assert.Equal(desc, res.Value.Desc);
        }

        [Fact]
        public async Task AddTagAsync_NameExist()
        {
            Result<TagInfo> res = new Result<TagInfo>();
            string name = "tag";
            string desc = "desc";
            tagMock
                .Setup(tr => tr.InsertTagAsync(It.Is<TagInfo>(t => t.Name == name)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNameHasExistException(name));


            var questionService = CreateQuestionService();

            res.Value = null;
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNameExistException>(async () =>
                await questionService.AddTagAsync(name, desc, "Other"));
            Assert.Null(res.Value);
        }

        #endregion

        #region ModifyQuestionAsync

        [Fact]
        public async Task ModifyQuestionAsync_Basic()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int qid = 2;
            queMock
                .Setup(qr => qr.UpdateQuestionAsync(It.IsAny<QuestionWithListTag>()))
                .Callback((QuestionWithListTag q) => res.Value = q);

            var questionService = CreateQuestionService();

            var item = new QuestionModifyItems()
            {
                Title = "title",
                Remarks = "remarks",
                BestAnswer = 4,
                Tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList()
            };
            await questionService.ModifyQuestionAsync(qid, item);

            queMock.Verify();
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(item.Title, res.Value.Title);
            Assert.Equal(item.Remarks, res.Value.Remarks);
            Assert.True(item.Tags.OrderBy(t => t).SequenceEqual(res.Value.Tags.OrderBy(t => t)));
        }

        [Fact]
        public async Task ModifyQuestionAsync_QuestionNotExist()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int qid = 2;
            queMock
                .Setup(qr => qr.UpdateQuestionAsync(It.Is<QuestionWithListTag>(q => q.QuestionId == qid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.QuestionNotExistException(qid));

            var questionService = CreateQuestionService();

            var item = new QuestionModifyItems()
            {
                Title = "title",
                Remarks = "remarks",
                BestAnswer = 4,
                Tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList()
            };
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionService.ModifyQuestionAsync(qid, item));
            Assert.Null(res.Value);
        }

        [Fact]
        public async Task ModifyQuestionAsync_TitleTooLong()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int qid = 2;
            int max = Buaa.AIBot.Constants.QuestionTitleMaxLength;
            queMock
                .Setup(qr => qr.UpdateQuestionAsync(It.Is<QuestionWithListTag>(q => q.Title.Length > max)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.QuestionTitleTooLongException(0, max));
            queMock
                .Setup(qr => qr.UpdateQuestionAsync(It.Is<QuestionWithListTag>(q => q.Title.Length <= max)))
                .Callback((QuestionWithListTag q) => res.Value = q);


            var questionService = CreateQuestionService();

            var titleBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.QuestionTitleMaxLength))
            {
                titleBuilder.Append('t');
            }
            var item = new QuestionModifyItems()
            {
                Title = titleBuilder.ToString() + 't',
                Remarks = "remarks",
                BestAnswer = 4,
                Tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList()
            };
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionTitleTooLongException>(async () =>
                await questionService.ModifyQuestionAsync(qid, item));
            Assert.Null(res.Value);

            item.Title = titleBuilder.ToString();
            await questionService.ModifyQuestionAsync(qid, item);
            Assert.Equal(qid, res.Value.QuestionId);
            Assert.Equal(item.Title, res.Value.Title);
        }

        [Fact]
        public async Task ModifyQuestionAsync_TagNotExist()
        {
            Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
            int qid = 2;
            int tid = 4;
            queMock
                .Setup(qr => qr.UpdateQuestionAsync(It.Is<QuestionWithListTag>(q => q.Tags.Contains(tid))))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNotExistException(tid));

            var questionService = CreateQuestionService();

            var item = new QuestionModifyItems()
            {
                Title = "title",
                Remarks = "remarks",
                BestAnswer = 4,
                Tags = Enumerable.Range(0, 13).Select(i => i << 2).Where(t => t != tid).Append(tid).ToList()
            };
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNotExistException>(async () =>
                await questionService.ModifyQuestionAsync(qid, item));
            Assert.Null(res.Value);
        }

        //[Fact]
        //public async Task ModifyQuestionAsync_AnswerNotExist()
        //{
        //    Result<QuestionWithListTag> res = new Result<QuestionWithListTag>();
        //    int qid = 2;
        //    int aid = 3;
        //    queMock
        //        .Setup(qr => qr.UpdateQuestionAsync(It.Is<QuestionWithListTag>(q => q.BestAnswerId == aid)))
        //        .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.AnswerNotExistException(aid));

        //    var questionService = CreateQuestionService();

        //    var item = new QuestionModifyItems()
        //    {
        //        Title = "title",
        //        Remarks = "remarks",
        //        BestAnswer = aid,
        //        Tags = Enumerable.Range(0, 13).Select(i => i << 2).ToList()
        //    };
        //    await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.AnswerNotExistException>(async () =>
        //        await questionService.ModifyQuestionAsync(qid, item));
        //    Assert.Null(res.Value);
        //}

        #endregion

        #region ModifyAnswerAsync

        [Fact]
        public async Task ModifyAnswerAsync_Basic()
        {
            Result<AnswerInfo> res = new Result<AnswerInfo>();
            int aid = 2;
            ansMock
                .Setup(ar => ar.UpdateAnswerAsync(It.IsAny<AnswerInfo>()))
                .Callback((AnswerInfo a) => res.Value = a);

            var questionService = CreateQuestionService();

            string content = "content";
            await questionService.ModifyAnswerAsync(aid, content);
            queMock.Verify();
            Assert.Equal(aid, res.Value.AnswerId);
            Assert.Equal(content, res.Value.Content);
        }

        [Fact]
        public async Task ModifyAnswerAsync_AnswerNotExist()
        {
            int aid = 2;
            ansMock
                .Setup(ar => ar.UpdateAnswerAsync(It.Is<AnswerInfo>(a => a.AnswerId == aid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.AnswerNotExistException(aid));

            var questionService = CreateQuestionService();

            string content = "content";
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.AnswerNotExistException>(async () =>
                await questionService.ModifyAnswerAsync(aid, content));
        }

        #endregion

        #region ModifyTagAsync

        [Fact]
        public async Task ModifyTagAsync_Basic()
        {
            Result<TagInfo> res = new Result<TagInfo>();
            int tid = 2;
            tagMock
                .Setup(tr => tr.UpdateTagAsync(It.IsAny<TagInfo>()))
                .Callback((TagInfo t) => res.Value = t);

            string name = "tag";
            string desc = "desc";

            var questionService = CreateQuestionService();

            await questionService.ModifyTagAsync(tid, name, desc, "Other");
            tagMock.Verify();
            Assert.Equal(tid, res.Value.TagId);
            Assert.Equal(name, res.Value.Name);
            Assert.Equal(desc, res.Value.Desc);
        }

        [Fact]
        public async Task ModifyTagAsync_TagNotExist()
        {
            int tid = 2;
            string name = "tag";
            string desc = "desc";
            tagMock
                .Setup(tr => tr.UpdateTagAsync(It.Is<TagInfo>(t => t.TagId == tid)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNotExistException(tid));

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNotExistException>(async () =>
                await questionService.ModifyTagAsync(tid, name, desc, "Other"));
        }

        [Fact]
        public async Task ModifyTagAsync_NameTooLong()
        {
            Result<TagInfo> res = new Result<TagInfo>();
            int tid = 2;
            int max = Buaa.AIBot.Constants.TagNameMaxLength;
            tagMock
                .Setup(tr => tr.UpdateTagAsync(It.Is<TagInfo>(t => t.Name.Length > max)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNameTooLongException(0, max));
            tagMock
                .Setup(tr => tr.UpdateTagAsync(It.Is<TagInfo>(t => t.Name.Length <= max)))
                .Callback((TagInfo t) => res.Value = t);

            var nameBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, max))
            {
                nameBuilder.Append('t');
            }

            string name = nameBuilder.ToString();
            string desc = "desc";

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNameTooLongException>(async () =>
                await questionService.ModifyTagAsync(tid, name + 't', desc, "Other"));
            Assert.Null(res.Value);

            await questionService.ModifyTagAsync(tid, name, desc, "Other");
            tagMock.Verify();
            Assert.Equal(tid, res.Value.TagId);
            Assert.Equal(name, res.Value.Name);
            Assert.Equal(desc, res.Value.Desc);
        }

        [Fact]
        public async Task ModifyTagAsync_NameExist()
        {
            int tid = 2;
            string name = "tag";
            string desc = "desc";
            tagMock
                .Setup(tr => tr.UpdateTagAsync(It.Is<TagInfo>(t => t.Name == name)))
                .ThrowsAsync(new Buaa.AIBot.Repository.Exceptions.TagNameHasExistException(name));

            var questionService = CreateQuestionService();

            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNameExistException>(async () =>
                await questionService.ModifyTagAsync(tid, name, desc, "Other"));
        }

        #endregion

        [Fact]
        public async Task DeleteQuestionAsync()
        {
            Result<int> res = new Result<int>();
            int qid = 2;
            queMock
                .Setup(qr => qr.DeleteQuestionByIdAsync(It.IsAny<int>()))
                .Callback((int q) => res.Value = q);
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(qid))
                .ReturnsAsync(new QuestionInfo());
            queMock
                .Setup(qr => qr.SelectQuestionByIdAsync(It.Is<int>(i => i != qid)))
                .ReturnsAsync(() => null);

            var questionServices = CreateQuestionService();

            await questionServices.DeleteQuestionAsync(qid);
            queMock.Verify(qr => qr.DeleteQuestionByIdAsync(It.IsAny<int>()));
            Assert.Equal(qid, res.Value);

            res.Value = 0;
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.QuestionNotExistException>(async () =>
                await questionServices.DeleteQuestionAsync(qid + 1));
            Assert.Equal(0, res.Value);
        }

        [Fact]
        public async Task DeleteAnswerAsync()
        {
            Result<int> res = new Result<int>();
            int aid = 2;
            ansMock
                .Setup(ar => ar.DeleteAnswerByIdAsync(It.IsAny<int>()))
                .Callback((int a) => res.Value = a);
            ansMock
                .Setup(ar => ar.SelectAnswerByIdAsync(aid))
                .ReturnsAsync(new AnswerInfo());
            ansMock
                .Setup(qr => qr.SelectAnswerByIdAsync(It.Is<int>(i => i != aid)))
                .ReturnsAsync(() => null);

            var questionServices = CreateQuestionService();

            await questionServices.DeleteAnswerAsync(aid);
            ansMock.Verify(ar => ar.DeleteAnswerByIdAsync(It.IsAny<int>()));
            Assert.Equal(aid, res.Value);

            res.Value = 0;
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.AnswerNotExistException>(async () =>
                await questionServices.DeleteAnswerAsync(aid + 1));
            Assert.Equal(0, res.Value);
        }

        [Fact]
        public async Task DeleteTagAsync()
        {
            Result<int> res = new Result<int>();
            int tid = 2;
            tagMock
                .Setup(tr => tr.DeleteTagAsync(It.IsAny<int>()))
                .Callback((int t) => res.Value = t);
            tagMock
                .Setup(tr => tr.SelectTagByIdAsync(tid))
                .ReturnsAsync(new TagInfo());
            tagMock
                .Setup(tr => tr.SelectTagByIdAsync(It.Is<int>(i => i != tid)))
                .ReturnsAsync(() => null);

            var questionServices = CreateQuestionService();

            await questionServices.DeleteTagAsync(tid);
            tagMock.Verify(tr => tr.DeleteTagAsync(It.IsAny<int>()));
            Assert.Equal(tid, res.Value);

            res.Value = 0;
            await Assert.ThrowsAsync<Buaa.AIBot.Services.Exceptions.TagNotExistException>(async () =>
                await questionServices.DeleteTagAsync(tid + 1));
            Assert.Equal(0, res.Value);
        }
    }
}























