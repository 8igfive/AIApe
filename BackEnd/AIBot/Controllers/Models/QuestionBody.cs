using System.Collections.Generic;

namespace Buaa.AIBot.Controllers.Models
{
    public class QuestionBody
    {
        public IEnumerable<int> Tags { get; set; }

        public int? Pt { get; set; }

        public int? Number { get; set; }

        public string Title { get; set; }

        public string Remarks { get; set; }

        public int? Qid { get; set; }

        public string Content { get; set; }

        public string Name { get; set; }

        public string Desc { get; set; }

        public string Question { get; set; }

        public int? BestAnswer { get; set; }

        public int? Aid { get; set; }

        public int? Tid { get; set; }

        public string Category { get; set; }
    }
}