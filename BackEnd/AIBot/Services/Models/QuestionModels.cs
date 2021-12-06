using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using System.Text.Json.Serialization;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Services.Models
{
    public class QuestionInformation
    {
        public string Title { get; set; }
        public string Remarks { get; set; }
        public int? Creator { get; set; }
        public bool? Like { get; set; }
        public int LikeNum { get; set; }
        public int HotValue { get; set; }
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime HotFreshTime { get; set; }
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime CreateTime { get; set; }
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime ModifyTime { get; set; }
        public Dictionary<string, int> Tags { get; set; }
        public IEnumerable<int> Answers { get; set; }
    }

    public class AnswerInformation
    {
        public string Content { get; set; }
        public int? Creator { get; set; }
        public bool? Like { get; set; }
        public int LikeNum { get; set; }
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime CreateTime { get; set; }
        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime ModifyTime { get; set; }
    }

    public class TagInformation
    {
        [JsonConverter(typeof(EnumJsonConverter<Repository.Models.TagCategory>))]
        public Repository.Models.TagCategory Category { get; set; }
        public string Name { get; set; }
        public string Desc { get; set; }
    }

    public class QuestionModifyItems
    {
        public string Title { get; set; } = null;
        public string Remarks { get; set; } = null;
        public int? BestAnswer { get; set; } = null;
        public IEnumerable<int> Tags { get; set; } = null;
    }
}
