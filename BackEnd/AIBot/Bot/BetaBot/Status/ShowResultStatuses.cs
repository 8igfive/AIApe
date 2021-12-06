using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.Framework;
using Buaa.AIBot.Bot.WorkingModule;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Services;

namespace Buaa.AIBot.Bot.BetaBot.Status
{
    public static class ShowResultStatuses
    {
    }

    public class ReduceResultByTagsStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.ReduceResultByTags;
        public static readonly string Skip = "跳过";
        public static readonly string SkipButton = "【跳过】";

        public static StatusId PrepareToEnter(IBotStatusContainer status, IReadOnlyDictionary<int, string> reducingTags, TagCategory category)
        {
            status.Put(Constants.Key.Cached_ReducingTags, reducingTags);
            status.Put(Constants.Key.Cached_CheckingTagCategory, category);
            return StatusId.ReduceResultByTags;
        }

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            if (status.GetCount(Id) == 0)
            {
                context.Sender
                    .AddMessage(SentenceGeneration.Choose(
                        $"小猿为您找到的信息有一些过多了，通过选择一些标签可以缩减范围。"
                        ))
                    ;
            }
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"您可以通过直接点击选项卡来选择单个标签，或者点击{SkipButton}来跳过选择。"
                    ))
                .AddMessage(SentenceGeneration.Choose(
                    $"如果您想选择多个标签（只要满足任意一个您选择的标签的信息就会被保留），请直接通过聊天框输入，并用逗号隔开。"
                    ))
                ;
            var reducingTags = status.Get<Dictionary<int, string>>(Constants.Key.Cached_ReducingTags);
            foreach (var tag in reducingTags.Values.Append(SkipButton))
            {
                context.Sender.AddPrompt(tag);
            }
            return Task.CompletedTask;
        }

        private List<int> FreshTagInfoAndGetSortedSelectedQuestions(IBotStatusContainer status, HashSet<int> selected)
        {
            var category = status.Get<TagCategory>(Constants.Key.Cached_CheckingTagCategory);
            var selectedTags = status.GetSelectedTags();
            selectedTags[category] = selected; new HashSet<int>();
            var questions = status.CalculateSortedSelectedQuestions(selectedTags);
            if (questions.Count == 0)
            {
                return null;
            }
            status.Put(Constants.Key.SelectedTags, selectedTags);
            status.Put(Constants.Key.Cached_SortedSelectedMatches, questions);
            return questions;
        }

        private StatusId DecideNext(IBotStatusContainer status, List<int> sortedSelectedQuestions = null)
        {
            sortedSelectedQuestions ??= status.GetSortedSelectedQuestions();
            if (sortedSelectedQuestions.Count < 3)
            {
                return StatusId.ShowAllResult;
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

        protected override Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var msg = context.Receiver.UserMessage;
            var tokens = msg.Split(new char[]{',', '，'}, StringSplitOptions.RemoveEmptyEntries);

            var reducingTags = status.Get<Dictionary<int, string>>(Constants.Key.Cached_ReducingTags);
            var dict = new Dictionary<string, int>(reducingTags.Select(kv => new KeyValuePair<string, int>(kv.Value, kv.Key)));
            var notRecognized = new List<string>();
            var selected = new HashSet<int>();

            foreach (var token in tokens)
            {
                if (dict.TryGetValue(token, out var tid))
                {
                    selected.Add(tid);
                }
                else
                {
                    notRecognized.Add(token);
                }
            }

            var sender = context.Sender;
            if (notRecognized.Count > 0)
            {
                var skipLikes = new List<string>();
                var others = new List<string>();
                foreach (var tag in notRecognized)
                {
                    if (tag.Contains(Skip))
                    {
                        skipLikes.Add(tag);
                    }
                    else
                    {
                        others.Add(tag);
                    }
                }
                if (others.Count > 0)
                {
                    sender
                        .AddMessage(SentenceGeneration.Choose(
                            $"这些标签我不认识{Kaomojis.Sad}",
                            $"请准确地输入标签的名字{Kaomojis.Sad}"
                            ), newLine: false)
                        .AddMessage($"“{string.Join("”，“", others)}”");
                }
                else
                {
                    sender.AddMessage(SentenceGeneration.Choose(
                        $"您选择了跳过这些标签的选择{Kaomojis.Cute}"
                        ))
                        ;
                    var newQuestion = FreshTagInfoAndGetSortedSelectedQuestions(status, new HashSet<int>());
                    return Task.FromResult(DecideNext(status, newQuestion));
                }
                if (skipLikes.Count > 0)
                {
                    sender
                        .AddMessage(SentenceGeneration.Choose(
                            $"“{Skip}”不是一个标签，请不要把它混在里面啦"
                            ))
                        ;
                }
                return Task.FromResult(Id);
            }
            else
            {
                var newQuestion = FreshTagInfoAndGetSortedSelectedQuestions(status, selected);
                return Task.FromResult(DecideNext(status, newQuestion));
            }
        }
    }

    public class ShowLimitResultStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.ShowLimitResult;
        public static readonly string Solved = "是";
        public static readonly string Unsolve = "否";

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"小猿为您找到了如下信息：",
                    $"来看下这些信息："
                    ))
                ;
            foreach (var qid in status.GetSortedSelectedQuestions().Take(3))
            {
                context.Sender.AddQuestion(qid);
            }
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"请问这些信息能解决您的问题吗？",
                    $"请问您的问题解决了吗？"
                    ))
                .AddPrompt(Solved)
                .AddPrompt(Unsolve)
                ;
            return Task.CompletedTask;
        }

        private StatusId DecideNext(IBotStatusContainer status)
        {
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

            return StatusId.ShowAllResult;
        }

        protected override async Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var msg = context.Receiver.UserMessage;

            if (msg == Solved)
            {
                return status.DecideFeedbackOrWelcome();
            }
            if (msg == Unsolve)
            {
                return DecideNext(status);
            }
            var res = await context.Worker.Get<INLPService>().SelectAsync(msg, Solved, Unsolve);
            if (res == Solved)
            {
                return status.DecideFeedbackOrWelcome();
            }
            return DecideNext(status);
        }
    }

    public class ShowAllResultStatus : BetaBotStatusBehaviour
    {
        public override StatusId Id => StatusId.ShowAllResult;
        public static readonly string Solved = "是";
        public static readonly string Unsolve = "否";

        protected override Task ProduceEnterAsync(IBotStatusContainer status, IBotEnterContext context)
        {
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"这里是小猿为您找到的所有信息：",
                    $"小猿为您找到的所有信息都在这里了："
                    ))
                ;
            foreach (var qid in status.GetSortedSelectedQuestions().Take(10))
            {
                context.Sender.AddQuestion(qid);
            }
            context.Sender
                .AddMessage(SentenceGeneration.Choose(
                    $"请问这些信息能解决您的问题吗？",
                    $"请问您的问题解决了吗？"
                    ))
                .AddPrompt(Solved)
                .AddPrompt(Unsolve)
                ;
            return Task.CompletedTask;
        }

        private StatusId DecideNext(IBotStatusContainer status)
        {
            if (status.TryGet(Constants.Key.ShortQuestion, out string _))
            {
                return StatusId.CreateQuestion;
            }
            return StatusId.GetShorter;
        }

        protected override async Task<StatusId> ProduceExitAsync(IBotStatusContainer status, IBotExitContext context)
        {
            var msg = context.Receiver.UserMessage;

            if (msg == Solved)
            {
                return status.DecideFeedbackOrWelcome();
            }
            if (msg == Unsolve)
            {
                return DecideNext(status);
            }
            var res = await context.Worker.Get<INLPService>().SelectAsync(msg, Solved, Unsolve);
            if (res == Solved)
            {
                return status.DecideFeedbackOrWelcome();
            }
            return DecideNext(status);
        }
    }
}
