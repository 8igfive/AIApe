using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using Xunit;
using Buaa.AIBot.Bot.Framework;

namespace AIBotTest.Bot.Framework
{
    public class StatusContainerPoolInMemoryTest
    {
        #region Get equals after Save

        private class BotStatusComparator<IdType> : IEqualityComparer<BotStatus<IdType>>
        {
            public bool Equals(BotStatus<IdType> x, BotStatus<IdType> y)
            {
                var a = x.Status.Equals(y.Status);
                var b = x.Items.OrderBy(kv => kv.Key).SequenceEqual(y.Items.OrderBy(kv => kv.Key));
                var c = x.UserId == y.UserId;
                return a && b && c;
            }

            public int GetHashCode([DisallowNull] BotStatus<IdType> obj)
            {
                return HashCode.Combine(obj.Items, obj.Status);
            }
        }

        [Fact]
        public async Task GetEqualsAfterSave_Simple()
        {
            IStatusContainerPool<int> pool = new StatusContainerPoolInMemory<int>();
            var status = new BotStatus<int>()
            {
                Status = 1,
                UserId = 1
            };
            foreach (var i in Enumerable.Range(0, 4))
            {
                status.Put(i.ToString(), $"msg{i}");
            }
            await pool.SaveStatusAsync(1, status);
            var res = await pool.GetStatusAsync(1);

            Assert.Equal(status, res, new BotStatusComparator<int>());
        }

        [Fact]
        public async Task GetEqualsAfterSave_Cover()
        {
            var comparator = new BotStatusComparator<int>();
            IStatusContainerPool<int> pool = new StatusContainerPoolInMemory<int>();
            var status = new BotStatus<int>()
            {
                Status = 1,
                UserId = 1
            };
            foreach (var i in Enumerable.Range(0, 4))
            {
                status.Put(i.ToString(), $"msg{i}");
            }
            await pool.SaveStatusAsync(1, status);
            var res1 = await pool.GetStatusAsync(1);
            Assert.Equal(status, res1, comparator);

            status.Put("key", "value");
            await pool.SaveStatusAsync(1, status);
            var res2 = await pool.GetStatusAsync(1);
            Assert.Equal(status, res2, comparator);
            Assert.NotEqual(res1, res2, comparator);
        }

        #endregion

        [Fact]
        public async Task GarbageCollectionTest()
        {
            TimeSpan liveTime = TimeSpan.FromMilliseconds(10);
            int gcCount = 8;
            IStatusContainerPool<int> pool = new StatusContainerPoolInMemory<int>(liveTime, gcCount);
            foreach (int i in Enumerable.Range(0, gcCount))
            {
                var status = new BotStatus<int>()
                {
                    UserId = i,
                    Status = i << 2
                };
                foreach (int j in Enumerable.Range(0, 10))
                {
                    status.Put(j.ToString(), $"msg_{i}_{j}");
                }
                await pool.SaveStatusAsync(i, status);
            }
            await Task.Delay(liveTime);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await pool.SaveStatusAsync(gcCount, new BotStatus<int>() { UserId = gcCount, Status = 0 });
            await pool.SaveStatusAsync(gcCount + 1, new BotStatus<int>() { UserId = gcCount + 1, Status = 0 });
            foreach (int i in Enumerable.Range(0, gcCount))
            {
                Assert.Null(await pool.GetStatusAsync(i));
            }
            var ret1 = await pool.GetStatusAsync(gcCount);
            var ret2 = await pool.GetStatusAsync(gcCount + 1);
            stopwatch.Stop();
            if (stopwatch.Elapsed < liveTime)
            {
                Assert.NotNull(ret1);
                Assert.NotNull(ret2);
            }
        }

        [Fact]
        public async Task ConcurrentWithoutGCTest()
        {
            TimeSpan liveTime = TimeSpan.FromMilliseconds(100);
            int gcCount = 128;
            IStatusContainerPool<int> pool = new StatusContainerPoolInMemory<int>(liveTime, gcCount);

            int taskCount = gcCount;
            var tasks = new List<Task>(taskCount);
            foreach (int i in Enumerable.Range(0, taskCount))
            {
                tasks.Add(Task.Run(async () =>
                {
                    int id = i;
                    var status = new BotStatus<int>()
                    {
                        UserId = id,
                        Status = id
                    };
                    await pool.SaveStatusAsync(id, status);
                    int count = -1;
                    while (count < 100)
                    {
                        var s = await pool.GetStatusAsync(id);
                        Assert.NotNull(s);
                        var newCount = s.GetCount(id);
                        Assert.Equal(count + 1, newCount);
                        count = newCount;
                        s.IncreaseCount(id);
                        await pool.SaveStatusAsync(id, s);
                    }
                }));
            }
            await Task.WhenAll(tasks);
        }
    }
}
