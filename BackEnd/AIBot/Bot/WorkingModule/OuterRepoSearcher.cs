using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Repository;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class OuterRepoSearcher
    {
        private ICrawlerOuterRepository crawler;

        public OuterRepoSearcher(ICrawlerOuterRepository crawler)
        {
            this.crawler = crawler;
        }

        public async Task SendSearchResultAsync(IBotStatusContainer status, IBotSender sender)
        {
            bool found = false;
            string simpleDesc;
            if (status.TryGet<string>(ConstantStrings.SimpleDescribe, out simpleDesc))
            {
                var results = await crawler.SearchAsync(simpleDesc);
                foreach (var res in results)
                {
                    found = true;
                    sender.AddMessage(res.Title + "：", newLine: false).AddUrl(res.Url);
                }
            }
            if (!found)
            {
                sender.AddMessage($"抱歉，未能帮您搜索到相关答案{Kaomojis.Sad}");
            }
            sender.NewScope();
        }
    }
}
