using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Controllers.Models
{
    public class FavoriteBody
    {
        public int? Fid { get; set; }

        public int? Qid { get; set; }

        public int? Aid { get; set; }

        public bool? MarkAsFavorite { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
    }
}