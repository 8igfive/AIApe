using Buaa.AIBot.Bot.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.BetaBot.Status;

namespace Buaa.AIBot.Bot.BetaBot
{
    public enum StatusId
    {
        Welcome,
        GetShorter,
            ShowLimitResult,
            ReduceResultByTags,
                    ShowAllResult,
                        CreateQuestion,

        Reset,
        GetFeedBack,
    }

    public static class Configuration
    {
        public static Dictionary<StatusId, IBotStatusBehaviour<StatusId>> GetStatusBehaviours()
        {
            var list = new List<IBotStatusBehaviour<StatusId>>()
            {
                new WelcomeStatus(),
                new GetShorterStatus(),
                    new ShowLimitResultStatus(),
                    new ReduceResultByTagsStatus(),
                        new ShowAllResultStatus(),
                            new CreateQuestionStatus(),

                new ResetStatus(),
                new GetFeedBackStatus(),
            };
            var ret = new Dictionary<StatusId, IBotStatusBehaviour<StatusId>>();
            foreach (var status in list)
            {
                ret[status.Id] = status;
            }
            return ret;
        }
    }

}
