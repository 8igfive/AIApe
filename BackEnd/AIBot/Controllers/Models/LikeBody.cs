using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Controllers.Models
{
    public class LikeQuestionBody
    {
        public int Qid { get; set; }
        public bool MarkAsLike { get; set; }
    }
    public class LikeAnswerBody
    {
        public int Aid { get; set; }
        public bool MarkAsLike { get; set; }
    }
}
