using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework
{
    public static class Extensions
    {
        public static string CountKey<IdType>(IdType id)
        {
            return $"__COUNT_{id}__";
        }

        public static int GetCount<IdType>(this IBotStatusContainer status, IdType id)
        {
            return status.GetOrDefault(CountKey(id), 0);
        }

        public static void IncreaseCount<IdType>(this IBotStatusContainer status, IdType id)
        {
            status.Put(CountKey(id), status.GetCount(id) + 1);
        }

        public static void ClearCount<IdType>(this IBotStatusContainer status, IdType id)
        {
            status.Remove(CountKey(id));
        }
    }
}
