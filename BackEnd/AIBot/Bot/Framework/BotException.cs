using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework.Exceptions
{
    /// <summary>
    /// Base Exception of Bot Framwork.
    /// </summary>
    public class BotException : Exception
    {
        public BotException() : base() { }
        public BotException(string msg) : base(msg) { }
        public BotException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    ///// <summary>
    ///// No Session was found.
    ///// </summary>
    ///// <remarks>the session may be timeout or haven't been created. </remarks>
    //public class SessionNullException : BotException
    //{
    //    public SessionNullException(int uid) :
    //        base($"User with uid={uid}'s bot session may has been timeout or not start yet.")
    //    { }
    //}
}
