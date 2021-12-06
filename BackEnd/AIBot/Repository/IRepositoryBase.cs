using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Buaa.AIBot.Repository.Models;


namespace Buaa.AIBot.Repository
{
    public interface IRepositoryBase
    {
        CancellationToken CancellationToken { get; set; }
        DatabaseContext Context { get; set; }
    }
}
