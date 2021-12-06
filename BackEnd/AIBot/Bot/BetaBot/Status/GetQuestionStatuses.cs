using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot.WorkingModule;
using static Buaa.AIBot.Bot.WorkingModule.InnerRepoSearcher;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Bot.BetaBot.Status
{

    public class WelcomeStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.Welcome;

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;

            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage(SentenceGeneration.Choose($"你好，我是小猿，有关编程的问题都可以来问我{Kaomojis.Happy}"))
                    ;
            }

            return Task.CompletedTask;
        }

        protected override async Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var worker = context.Worker;
            var msg = context.Receiver.UserMessage;

            if (AIBot.Utils.QuestionJudgement.IsCode("", msg))
            {
                context.Sender.AddMessage(SentenceGeneration.Choose("检测到您的问题中可能含有代码，小猿建议您可以尝试一下代码分析功能，通过导航栏即可打开。"));
            }

            
            var searcher = worker.GetInnerRepoSearcher();
            var searchTask = searcher.SearchAsync(msg, true);
            if (msg.Length <= AIBot.Constants.QuestionTitleMaxLength / 2)
            {
                status.Put(Constants.Key.Checking, Constants.Value.CheckingShort);
                status.Put(Constants.Key.ShortQuestion, msg);
            }
            else
            {
                status.Put(Constants.Key.Checking, Constants.Value.CheckingLong);
                status.Put(Constants.Key.LongQuestion, msg);
            }
            var res = await searchTask;
            var logger = context.Worker.Get<ILogger<WelcomeStatus>>();
            logger.LogInformation("QuestionMatches:\n{questionMatches}", string.Join("\n", res.Select(q => new Tuple<int, double>(q.Qid, q.Score))));
            
            var first = res.FirstOrDefault();
            if (first != default && first.Qid < 0)
            {
                var ans = await worker.GetNaturalAnswerGenerator().GetAnswerAsync(first.Qid);
                context.Sender.AddMessage(ans);
                status.Clear();
                status.IncreaseCount(Id);
                return Id;
            }

            status.ClearCount(Id);
            status.Put(Constants.Key.QuestionMatches, res);
            var questions = status.CalculateSortedSelectedQuestions(null, res);
            logger.LogInformation("SortedSelectedQuestionMatches:\n[{sortedSelectedQuestionMatches}]", string.Join(", ", questions));
            if (questions.Count == 0)
            {
                if (status.Get<string>(Constants.Key.Checking) == Constants.Value.CheckingLong)
                {
                    if (status.TryGet<string>(Constants.Key.ShortQuestion, out _))
                    {
                        return StatusId.CreateQuestion;
                    }
                    else
                    {
                        return StatusId.GetShorter;
                    }
                }
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"抱歉，小猿暂时未能帮您找到有用的信息{Kaomojis.Sad}"
                        ))
                    .NewScope();
                return StatusId.CreateQuestion;
            }
            status.Put(Constants.Key.Cached_SortedSelectedMatches, questions);
            if (questions.Count <= 3)
            {
                return StatusId.ShowAllResult;
            }
            if (questions.Count <= 10)
            {
                return StatusId.ShowLimitResult;
            }
            var reducingTags = status.GetReducingTags(out var canBeEmpty, out var category);
            {
                var selectedTags = status.GetSelectedTags();
                foreach (var c in canBeEmpty)
                {
                    selectedTags[c] = new HashSet<int>();
                }
                status.Put(Constants.Key.SelectedTags, selectedTags);
            }

            if (reducingTags != null)
            {
                return ReduceResultByTagsStatus.PrepareToEnter(status, reducingTags, category);
            }

            return StatusId.ShowLimitResult;
        }
    }


    public class GetShorterStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.GetShorter;
        public static readonly string SimplizeQuestion = "精简问题（请直接在聊天框输入）";
        public static readonly string CreateQuestion = "创建问题";

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            var sender = context.Sender;

            if (status.GetCount(Id) == 0)
            {
                sender
                    .AddMessage(SentenceGeneration.Choose($"您的问题描述太长了{Kaomojis.Sad}，精简结果可能会帮助我帮您找到合适和回答，最好能够少于{AIBot.Constants.QuestionTitleMaxLength / 2}字。"))
                    ;
            }
            status.IncreaseCount(Id);

            sender
                .AddMessage(SentenceGeneration.Choose($"您可以选择在聊天框输入精简后的问题，或者选择创建问题等待他人回答。"))
                .AddPrompt(SimplizeQuestion)
                .AddPrompt(CreateQuestion)
                ;
            return Task.CompletedTask;
        }

        protected override async Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var worker = context.Worker;
            var msg = context.Receiver.UserMessage;


            if (msg.Length <= 4)
            {
                if (CreateQuestion == msg)
                {
                    context.Sender
                        .AddMessage(SentenceGeneration.Choose($"您选择跳过精简问题咯{Kaomojis.Cute}"))
                        .NewScope()
                        ;
                    return StatusId.CreateQuestion;
                }
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"虽然但是，您的问题也精得太简了吧{Kaomojis.Sad}",
                        $"您给的“精简版”问题是不是精简的过头了呀{Kaomojis.Frightened}"
                    ))
                    .NewScope()
                    ;
                return Id;
            }
            if (msg.Length > AIBot.Constants.QuestionTitleMaxLength)
            {
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"再精简一点，拜托了{Kaomojis.Fighting}",
                        $"还是，有一丢丢长啊……麻烦再精简一些吧{Kaomojis.Fighting}"
                        ))
                    .NewScope()
                    ;
                return Id; 
            }
            if (SimplizeQuestion == msg)
            {
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"人家说了让你直接在聊天框输入了啦，为什么还是要点这个选项啊{Kaomojis.Sad}",
                        $"你这样显得我没法让这个选项卡无法选中就很菜，给点面子好不好{Kaomojis.Cute}"
                        ))
                    .NewScope()
                    ;
                return Id;
            }

            status.ClearCount(Id);
            status.Put(Constants.Key.Checking, Constants.Value.CheckingShort);
            status.Put(Constants.Key.ShortQuestion, msg);

            var res = await worker.GetInnerRepoSearcher().SearchAsync(msg, false);
            status.Put(Constants.Key.QuestionMatches, res);
            var questions = status.CalculateSortedSelectedQuestions(null, res);
            if (questions.Count == 0)
            {
                if (status.Get<string>(Constants.Key.Checking) == Constants.Value.CheckingLong)
                {
                    if (status.TryGet<string>(Constants.Key.ShortQuestion, out _))
                    {
                        return StatusId.CreateQuestion;
                    }
                    else
                    {
                        return StatusId.GetShorter;
                    }
                }
            }
            status.Put(Constants.Key.Cached_SortedSelectedMatches, questions);
            if (questions.Count <= 3)
            {
                return StatusId.ShowAllResult;
            }
            if (questions.Count <= 10)
            {
                return StatusId.ShowLimitResult;
            }
            var reducingTags = status.GetReducingTags(out var canBeEmpty, out var category);
            {
                var selectedTags = status.GetSelectedTags();
                foreach (var c in canBeEmpty)
                {
                    selectedTags[c] = new HashSet<int>();
                }
                status.Put(Constants.Key.SelectedTags, selectedTags);
            }

            if (reducingTags != null)
            {
                return ReduceResultByTagsStatus.PrepareToEnter(status, reducingTags, category);
            }

            return StatusId.ShowLimitResult;
        }
    }
}
