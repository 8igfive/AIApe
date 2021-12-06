using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Buaa.AIBot.Bot;
using System.Text.Json;

namespace Buaa.AIBot.Bot.Framework
{
    /// <summary>
    /// A pool to store all <see cref="IBotStatusContainer{IdType}"/> for this bot.
    /// </summary>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public interface IStatusContainerPool<IdType>
    {
        /// <summary>
        /// Get the status by given <paramref name="sessionId"/>.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns>status if exists and not timeout, or null. </returns>
        Task<BotStatus<IdType>> GetStatusAsync(int sessionId);

        /// <summary>
        /// Save the status. Get the equals one when call <see cref="GetStatusAsync(int)"/> with the same <paramref name="sessionId"/>.
        /// </summary>
        /// <param name="sessionId"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        Task SaveStatusAsync(int sessionId, BotStatus<IdType> status);
    }

    /// <summary>
    /// Implement of <see cref="IStatusContainerPool{IdType}"/>, using memory store.
    /// </summary>
    /// <remarks>
    /// Only one thread can enter this poll, with means that It is concurrent-safe.
    /// It is async, so no Thread will be blocked while waiting.
    /// </remarks>
    /// <typeparam name="IdType"></typeparam>
    public class StatusContainerPoolInMemory<IdType> : IStatusContainerPool<IdType>
    {
        private Dictionary<int, Node> memory = new Dictionary<int, Node>();
        private readonly TimeSpan liveTime;
        private readonly int gcCount;
        private readonly SemaphoreSlim gate = new SemaphoreSlim(1);

        public StatusContainerPoolInMemory()
        {
            liveTime = TimeSpan.FromMinutes(10);
            gcCount = 128;
        }

        public StatusContainerPoolInMemory(TimeSpan liveTime, int gcCount = 128)
        {
            this.liveTime = liveTime;
            this.gcCount = gcCount;
        }

        private class Node
        {
            public DateTime VisitTime { get; set; }
            public string Json { get; set; }
        }

        private void GarbageCollection()
        {
            var tmp = new Dictionary<int, Node>();
            foreach (var pair in memory)
            {
                var span = DateTime.Now - pair.Value.VisitTime;
                if (span < liveTime)
                {
                    tmp.Add(pair.Key, pair.Value);
                }
            }
            memory = tmp;
        }

        public async Task<BotStatus<IdType>> GetStatusAsync(int sessionId)
        {
            await gate.WaitAsync();
            try
            {
                if (memory.TryGetValue(sessionId, out Node value))
                {
                    BotStatus<IdType> ret = JsonSerializer.Deserialize<BotStatus<IdType>>(value.Json);
                    value.VisitTime = DateTime.Now;
                    return ret;
                }
                return null;
            }
            finally
            {
                gate.Release();
            }
        }

        public async Task SaveStatusAsync(int sessionId, BotStatus<IdType> status)
        {
            await gate.WaitAsync();
            try
            {
                string json = JsonSerializer.Serialize(status);
                if (memory.TryGetValue(sessionId, out Node value))
                {
                    value.Json = json;
                    value.VisitTime = DateTime.Now;
                    return;
                }
                if (memory.Count > gcCount)
                {
                    GarbageCollection();
                }
                value = new Node()
                {
                    Json = json,
                    VisitTime = DateTime.Now
                };
                memory.Add(sessionId, value);
            }
            finally
            {
                gate.Release();
            }
        }
    }
}
