using System;
using System.Collections.Generic;

using Buaa.AIBot.Repository.Models; 

namespace Buaa.AIBot.Controllers.Exceptions
{
    public class ControllerException : Exception
    {
        public ControllerException() : base() { }

        public ControllerException(string msg) 
            : base(msg) { }

        public ControllerException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public class LackofAuthorityException : ControllerException
    {
        private static readonly Dictionary<AuthLevel, string> roleMap = new Dictionary<AuthLevel, string>
        {
            {AuthLevel.None, "Traveler"},
            {AuthLevel.User, "User"},
            {AuthLevel.Admin, "Administrator"}
        };

        public LackofAuthorityException (AuthLevel auth)
            : base("authority of" + roleMap[auth] + " is not enough") { }

        public LackofAuthorityException (AuthLevel auth, Exception innerException)
            : base("authority of" + roleMap[auth] + " is not enough", innerException) { }

    }
}