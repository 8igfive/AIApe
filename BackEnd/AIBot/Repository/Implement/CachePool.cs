using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Buaa.AIBot.Repository.Implement
{
    public enum CacheId: int
    {
        None = default,
        Tag,
        Question,
    }

    public static class CacheIdTools
    {
        public static void Set(this ICachePool<int> pool, CacheId id, object value)
        {
            pool.Set((int)id, value);
        }

        public static T Get<T>(this ICachePool<int> pool, CacheId id)
        {
            return pool.Get<T>((int)id);
        }

        public static T GetOrDefault<T>(this ICachePool<int> pool, CacheId id)
        {
            return pool.GetOrDefault<T>((int)id);
        }

        public static Task<IDisposable> LockAsync(this ICachePool<int> pool, CacheId id)
        {
            return pool.LockAsync((int)id);
        }
    }

    public interface ICachePool<TK>
    {
        TV Get<TV>(TK key);
        TV GetOrDefault<TV>(TK key);
        void Set(TK key, object value);
        Task<IDisposable> LockAsync(TK key);
    }

    public class CachePool<TK> : ICachePool<TK>
    {
        private Dictionary<TK, object> dic = new Dictionary<TK, object>();
        private ConcurrentDictionary<TK, SemaphoreSlim> locks = new ConcurrentDictionary<TK, SemaphoreSlim>();
        //private readonly SemaphoreSlim globalLock = new SemaphoreSlim(1);
        public static readonly CachePool<TK> DEFAULT = new CachePool<TK>();

        public object this[TK key]
        {
            get
            {
                return dic[key];
            }
            set
            {
                dic[key] = value;
            }
        }

        public void Set(TK key, object value)
        {
            dic[key] = value;
        }

        public TV GetOrDefault<TV>(TK key)
        {
            if (dic.TryGetValue(key, out object res))
            {
                return (TV)res;
            }
            return default(TV);
        }

        public TV Get<TV>(TK key)
        {
            return (TV)dic[key];
        }

        public async Task<IDisposable> LockAsync(TK key)
        {
            await locks.GetOrAdd(key, k => new SemaphoreSlim(1)).WaitAsync();
            return new LockHandler(this, key);
        }

        private class LockHandler : IDisposable
        {
            private CachePool<TK> cachePool;
            private TK key;

            public LockHandler(CachePool<TK> cachePool, TK key)
            {
                this.cachePool = cachePool;
                this.key = key;
            }

            public void Dispose()
            {
                cachePool.locks[key].Release();
            }
        }
    }
}
