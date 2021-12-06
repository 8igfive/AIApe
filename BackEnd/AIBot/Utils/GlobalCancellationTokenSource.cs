using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Buaa.AIBot.Utils
{
    public class GlobalCancellationTokenSource
    {
        public CancellationTokenSource Source { get; set; } = new CancellationTokenSource();
        public CancellationToken Token
        {
            get
            {
                if (Source == null)
                {
                    return CancellationToken.None;
                }
                return Source.Token;
            }
        }
    }
}
