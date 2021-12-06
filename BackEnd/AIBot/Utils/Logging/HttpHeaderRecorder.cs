using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

namespace Buaa.AIBot.Utils.Logging
{
    public class HttpHeaderRecorder
    {
        private ILogger<HttpHeaderRecorder> logger;

        public HttpHeaderRecorder(ILogger<HttpHeaderRecorder> logger)
        {
            this.logger = logger;
        }

        public void LogRequest(HttpContext httpContext)
        {
            logger.LogInformation("Request Headers={RequestHeaders}", httpContext.Request.Headers);
        }

        public void LogResponse(HttpContext httpContext)
        {
            logger.LogInformation("Response Headers={ResponsetHeaders}", httpContext.Response.Headers);
        }
    }
}
