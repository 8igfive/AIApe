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
    public class TagRepositoryTest
    {
        private static int count = 0;
        private ICachePool<int> cachePool = new CachePool<int>();
        private Buaa.AIBot.Utils.GlobalCancellationTokenSource globalCancellation = new Buaa.AIBot.Utils.GlobalCancellationTokenSource();

        private DbContextOptions<DatabaseContext> CreateUniqueOptions()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseInMemoryDatabase($"{nameof(TagRepositoryTest)}@{count++}");
            return builder.Options;
        }

        [Fact]
        public async Task SelectTagByIdAsyncTest()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(
                    Enumerable.Range(0, tagNum)
                    .Select(i =>new TagData() 
                    { 
                        Name = $"Tag{i}", 
                        Desc = $"This is tag No.{i}",
                        Category = (int)TagCategory.Other
                    }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                foreach (var tid in await context.Tags.Select(t => t.TagId).ToListAsync())
                {
                    var tagInfo = await tagRepostory.SelectTagByIdAsync(tid);
                    Assert.Equal(tid, tagInfo.TagId);
                }
            }
        }

        #region SelectAllTagsAsync

        [Fact]
        public async Task SelectAllTagsAsync_Basic()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(
                    Enumerable.Range(0, tagNum)
                    .Select(i =>new TagData()
                    {
                        Name = $"Tag{i}",
                        Desc = $"This is tag No.{i}",
                        Category = (int)TagCategory.Other
                    }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var res = await tagRepostory.SelectAllTagsAsync();

                HashSet<int> tags = new HashSet<int>();
                foreach (var pair in res)
                {
                    var search = await tagRepostory.SelectTagByIdAsync(pair.Value);
                    Assert.Equal(search.TagId, pair.Value);
                    Assert.Equal(search.Name, pair.Key);
                    Assert.Equal("Tag", pair.Key.Substring(0, 3));
                    tags.Add(int.Parse(pair.Key.Substring(3)));
                }
                Assert.True(tags.OrderBy(t => t).SequenceEqual(Enumerable.Range(0, tagNum)));
            }
        }

        [Fact]
        public async Task SelectAllTagsAsync_CacheCountUpdate()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(
                    Enumerable.Range(0, tagNum)
                    .Select(i => new TagData()
                    {
                        Name = $"Tag{i}",
                        Desc = $"This is tag No.{i}",
                        Category = (int)TagCategory.Other
                    }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var res1 = await tagRepostory.SelectAllTagsAsync();
                Assert.Equal(tagNum, res1.Count);

                var newTag = new TagInfo()
                {
                    Name = $"Tag{tagNum}",
                    Desc = $"This is tag No.{tagNum}",
                    Category = TagCategory.Other
                };
                int tid = await tagRepostory.InsertTagAsync(newTag);
                var res2 = await tagRepostory.SelectAllTagsAsync();
                Assert.Equal(tagNum + 1, res2.Count);

                await tagRepostory.DeleteTagAsync(tid);
                var res3 = await tagRepostory.SelectAllTagsAsync();
                Assert.Equal(tagNum, res3.Count);

                var res4 = await tagRepostory.SelectAllTagsAsync();
                Assert.Equal(res3.Count, res4.Count);
            }
        }

        [Fact]
        public async Task SelectAllTagsAsync_CacheContentUpdate()
        {
            var options = CreateUniqueOptions();

            int tagNum = 10;
            using (var context = new DatabaseContext(options))
            {
                context.AddRange(
                    Enumerable.Range(0, tagNum)
                    .Select(i => new TagData()
                    {
                        Name = $"Tag{i}",
                        Desc = $"This is tag No.{i}",
                        Category = (int)TagCategory.Other
                    }));
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var res1 = await tagRepostory.SelectAllTagsAsync();
                var tid = res1.Values.First();

                var newTag = new TagInfo()
                {
                    TagId = tid,
                    Name = "mark"
                };
                await tagRepostory.UpdateTagAsync(newTag);

                var res2 = await tagRepostory.SelectAllTagsAsync();
                Assert.True(res2.ContainsKey("mark"));
                Assert.Equal(tid, res2["mark"]);
            }
        }

        #endregion

        #region InsertTagAsync

        [Fact]
        public async Task InsertTagAsync_Basic()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    Name = "Tag0",
                    Desc = "This is tag No.0",
                    Category = TagCategory.Other
                };
                await tagRepostory.InsertTagAsync(newTag);

                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(newTag.Name, tagData.Name);
                Assert.Equal(newTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task InsertTagAsync_NameNull()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    Name = null,
                    Desc = "This is tag No.0",
                    Category = TagCategory.Other
                };
                await Assert.ThrowsAsync<ArgumentNullException>(async () => 
                    await tagRepostory.InsertTagAsync(newTag));

                Assert.Equal(0, await context.Tags.CountAsync());
            }
        }

        [Fact]
        public async Task InsertTagAsync_DescNull()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    Name = "Tag0",
                    Desc = null,
                    Category = TagCategory.Other
                };
                await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                    await tagRepostory.InsertTagAsync(newTag));

                Assert.Equal(0, await context.Tags.CountAsync());
            }
        }

        [Fact]
        public async Task InsertTagAsync_NameTooLong()
        {
            var options = CreateUniqueOptions();
            var nameBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.TagNameMaxLength))
            {
                nameBuilder.Append('t');
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var maxLengthNameTag = new TagInfo()
                {
                    Name = nameBuilder.ToString(),
                    Desc = "This is tag with max length Name",
                    Category = TagCategory.Other
                };
                await tagRepostory.InsertTagAsync(maxLengthNameTag);

                nameBuilder.Append('t');
                var tooLongNameTag = new TagInfo()
                {
                    Name = nameBuilder.ToString(),
                    Desc = "This is tag with max length Name",
                    Category = TagCategory.Other
                }; 
                await Assert.ThrowsAsync<TagNameTooLongException>(async () =>
                    await tagRepostory.InsertTagAsync(tooLongNameTag));

                Assert.Equal(1, await context.Tags.CountAsync());
                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(maxLengthNameTag.Name, tagData.Name);
                Assert.Equal(maxLengthNameTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task InsertTagAsync_NameExists()
        {
            var options = CreateUniqueOptions();
            string name = "Tag";

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var firstTag = new TagInfo()
                {
                    Name = name,
                    Desc = "This is tag with max length Name",
                    Category = TagCategory.Other
                };
                await tagRepostory.InsertTagAsync(firstTag);

                var secondNameTag = new TagInfo()
                {
                    Name = name,
                    Desc = "This is tag with max length Name",
                    Category = TagCategory.Other
                };
                await Assert.ThrowsAsync<TagNameHasExistException>(async () =>
                    await tagRepostory.InsertTagAsync(secondNameTag));

                Assert.Equal(1, await context.Tags.CountAsync());
            }
        }

        #endregion

        #region UpdateTagAsync

        [Fact]
        public async Task UpdateTagAsync_UpdateNameOnly()
        {
            var options = CreateUniqueOptions();
            var originTag = new TagData()
            {
                Name = "tag",
                Desc = "desc",
                Category = (int)TagCategory.Other
            };

            using (var context = new DatabaseContext(options))
            {
                context.Add(originTag);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    TagId = originTag.TagId,
                    Name = "NewName",
                    Desc = null
                };
                await tagRepostory.UpdateTagAsync(newTag);

                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(newTag.Name, tagData.Name);
                Assert.Equal(originTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task UpdateTagAsync_UpdateDescOnly()
        {
            var options = CreateUniqueOptions();
            var originTag = new TagData()
            {
                Name = "tag",
                Desc = "desc",
                Category = (int)TagCategory.Other
            };

            using (var context = new DatabaseContext(options))
            {
                context.Add(originTag);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    TagId = originTag.TagId,
                    Name = null,
                    Desc = "This is tag No.0"
                };
                await tagRepostory.UpdateTagAsync(newTag);

                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(originTag.Name, tagData.Name);
                Assert.Equal(newTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task UpdateTagAsync_UpdateAll()
        {
            var options = CreateUniqueOptions();
            var originTag = new TagData()
            {
                Name = "tag",
                Desc = "desc",
                Category = (int)TagCategory.Other
            };

            using (var context = new DatabaseContext(options))
            {
                context.Add(originTag);
                await context.SaveChangesAsync();
            }

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var newTag = new TagInfo()
                {
                    TagId = originTag.TagId,
                    Name = "NewName",
                    Desc = "NewDesc"
                };
                await tagRepostory.UpdateTagAsync(newTag);

                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(newTag.Name, tagData.Name);
                Assert.Equal(newTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task UpdateTagAsync_NameTooLong()
        {
            var options = CreateUniqueOptions();
            var originTag = new TagData()
            {
                Name = "tag",
                Desc = "desc",
                Category = (int)TagCategory.Other
            };
            using (var context = new DatabaseContext(options))
            {
                context.Add(originTag);
                await context.SaveChangesAsync();
            }
            var nameBuilder = new StringBuilder();
            foreach (var _ in Enumerable.Range(0, Buaa.AIBot.Constants.TagNameMaxLength))
            {
                nameBuilder.Append('t');
            }


            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var maxLengthTag = new TagInfo()
                {
                    TagId = originTag.TagId,
                    Name = nameBuilder.ToString(),
                    Desc = "NewDesc"
                };
                await tagRepostory.UpdateTagAsync(maxLengthTag);

                nameBuilder.Append('t');
                var tooLongTag = new TagInfo()
                {
                    TagId = originTag.TagId,
                    Name = nameBuilder.ToString(),
                    Desc = "NewDesc"
                };
                await Assert.ThrowsAsync<TagNameTooLongException>(async () =>
                    await tagRepostory.UpdateTagAsync(tooLongTag));

                Assert.Equal(1, await context.Tags.CountAsync());
                var tagData = await context.Tags.FirstOrDefaultAsync();
                Assert.Equal(maxLengthTag.Name, tagData.Name);
                Assert.Equal(maxLengthTag.Desc, tagData.Desc);
            }
        }

        [Fact]
        public async Task UpdateTagAsync_NameExists()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var firstTag = new TagInfo()
                {
                    Name = "Tag0",
                    Desc = "This is tag",
                    Category = TagCategory.Other
                };
                await tagRepostory.InsertTagAsync(firstTag);

                var secondNameTag = new TagInfo()
                {
                    Name = "Tag1",
                    Desc = "This is tag",
                    Category = TagCategory.Other
                };
                int tid = await tagRepostory.InsertTagAsync(secondNameTag);

                var newInfo = new TagInfo()
                {
                    TagId = tid,
                    Name = "Tag0",
                    Desc = null,
                    Category = TagCategory.Other
                };
                await Assert.ThrowsAsync<TagNameHasExistException>(async () =>
                    await tagRepostory.UpdateTagAsync(newInfo));

                Assert.Equal(2, await context.Tags.CountAsync());
                var tagInfo = await tagRepostory.SelectTagByIdAsync(tid);
                Assert.Equal(secondNameTag.Name, tagInfo.Name);
                Assert.Equal(secondNameTag.Desc, tagInfo.Desc);
            }
        }

        [Fact]
        public async Task UpdateTagAsync_TagNotExist()
        {
            var options = CreateUniqueOptions();

            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);

                var newInfo = new TagInfo()
                {
                    TagId = 1,
                    Name = "Tag0",
                    Desc = null,
                    Category = TagCategory.Other
                };
                await Assert.ThrowsAsync<TagNotExistException>(async () =>
                    await tagRepostory.UpdateTagAsync(newInfo));

                Assert.Equal(0, await context.Tags.CountAsync());
            }
        }

        #endregion

        [Fact]
        public async Task DeleteTagAsyncTest()
        {
            var options = CreateUniqueOptions();


            using (var context = new DatabaseContext(options))
            {
                ITagRepostory tagRepostory = new TagRepository(context, cachePool, globalCancellation);
                var firstTag = new TagInfo()
                {
                    Name = "Tag0",
                    Desc = "This is tag",
                    Category = TagCategory.Other
                };
                int tid = await tagRepostory.InsertTagAsync(firstTag);

                await tagRepostory.DeleteTagAsync(tid + 1);
                Assert.Equal(1, await context.Tags.CountAsync());

                await tagRepostory.DeleteTagAsync(tid);
                Assert.Equal(0, await context.Tags.CountAsync());
            }
        }
    }
}
