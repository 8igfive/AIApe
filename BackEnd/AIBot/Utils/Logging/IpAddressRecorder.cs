using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;


namespace Buaa.AIBot.Utils.Logging
{
    public class IpAddressRecorder
    {
        public static readonly string X_Forwarded_For = "X-Forwarded-For";

        public ILogger<IpAddressRecorder> Logger { get; }

        public IpAddressRecorder(ILogger<IpAddressRecorder> logger)
        {
            this.Logger = logger;
        }

        public IPAddress GetUserAddress(HttpContext context)
        {
            var header = context.Request.Headers;
            if (header.ContainsKey(X_Forwarded_For))
            {
                return IPAddress.Parse(header[X_Forwarded_For]);
            }
            return context.Connection.RemoteIpAddress;
        }
    }
}
