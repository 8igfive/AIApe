using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Services.Models
{
    public class FavoriteInformation
    {
        public int Creator { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        [JsonConverter(typeof(DateTimeJsonConverter))]
        public DateTime CreateTime { get; set; }
        public IEnumerable<int> Questions { get; set; }
        public IEnumerable<int> Answers { get; set; }
    }

    public class FavoriteModifyItems
    {
        public string Name { get; set; } = null;
        public string Description { get; set; } = null;
    }
}