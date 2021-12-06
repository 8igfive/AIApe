using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;
using System.Net;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository
{

    public interface ICrawlerOuterRepository
    {
        Task<List<SearchResult>> SearchAsync(string question, string proxy=null);
    }
    
    public class BaiduCrawlerRepository : ICrawlerOuterRepository
    {
        private readonly static string searchHead = "http://www.baidu.com/s?ie=utf-8&wd=";

        private readonly static string searchTail = "&pn=0";

        private readonly static Dictionary<char, string> charMap = new Dictionary<char, string>
        {
            {' ', "%20"},
            {'\"', "%22"},
            {'#', "%23"},
            {'%', "%25"},
            {'&', "%26"},
            {'(', "%28"},
            {')', "%29"},
            {'+', "%2B"},
            {',', "%2C"},
            {'/', "%2F"},
            {':', "%3A"},
            {';', "%3B"},
            {'<', "%3C"},
            {'=', "%3D"},
            {'>', "%3E"},
            {'?', "%3F"},
            {'@', "%40"},
            {'\\', "5C"},
            {'|', "%7C"}
        };

        private event EventHandler<OnStartEventArgs> onStart;

        private event EventHandler<OnCompletedEventArgs> onCompleted;

        private event EventHandler<OnErrorEventArgs> onError;

        private List<SearchResult> results;
        
        private readonly ILogger<BaiduCrawlerRepository> logger;

        private readonly CancellationToken token;

        private string pageSource;
        private static CookieContainer CookiesContainer { get; set; }

        private static List<string> websites = new List<string>
        {
            "博客园",
            "菜鸟教程",
            "知乎",
            "百度知道",
            "CSDN",
            "简书"
        };

        private readonly static int maxWebCount = 3;

        private static readonly Regex reAnswer = new Regex(@"<h3\s*class\s*=\s*(.|\n)*?>\s*<a(.|\n)*?href\s*=\s*""(?<url>.*?)""(.|\n)*?>(?<content>(.|\n)*?)</a>\s*</h3>");
        public BaiduCrawlerRepository(ILogger<BaiduCrawlerRepository> logger, GlobalCancellationTokenSource globalCancellationTokenSource)
        {
            if (CookiesContainer == null)
            {
                CookiesContainer = new CookieContainer();
            }

            results = new List<SearchResult>();
            onStart = (s, e) => 
            {
                // Console.WriteLine("START");
            };
            onCompleted = (s, e) =>
            {
                var matches = reAnswer.Matches(pageSource);
                token.ThrowIfCancellationRequested();
                int localCount = 0;
                foreach (Match match in matches)
                {
                    if (localCount >= maxWebCount)
                    {
                        break;
                    }
                    string clearTitle = ClearString(match.Groups["content"].ToString());
                    foreach (string web in websites)
                    {
                        if (clearTitle.Contains(web))
                        {
                            results.Add(new SearchResult
                            {
                                Url = match.Groups["url"].ToString(),
                                Title = clearTitle
                            });
                            localCount += 1;
                            break;
                        }
                    }
                }
            };
            onError = (s, e) =>
            { };
            this.token = globalCancellationTokenSource.Token;
            this.logger = logger;
        }

        private HttpWebRequest getRequest(string query, string proxy=null)
        {
            Uri url = new Uri(searchHead + query + searchTail);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Accept = "*/*";
            request.ServicePoint.Expect100Continue = false;
            request.ServicePoint.UseNagleAlgorithm = false;
            request.AllowWriteStreamBuffering = false;
            request.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            request.ContentType = "application/x-www-form-urlencoded";
            request.AllowAutoRedirect = false;
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/50.0.2661.102 Safari/537.36";
            request.Timeout = 5000;
            request.KeepAlive = false;
            request.Method = "GET";
            if (proxy != null)
            {
                request.Proxy = new WebProxy(proxy);
            }
            request.CookieContainer = CookiesContainer;
            request.ServicePoint.ConnectionLimit = int.MaxValue;
            return request;
        }

        public async Task<List<SearchResult>> SearchAsync(string question, string proxy=null)
        {
            onStart(this, new OnStartEventArgs
            {
                Question = question
            });
            StringBuilder encodeQuestion = new StringBuilder();
            foreach(char c in question)
            {
                if (charMap.ContainsKey(c))
                {
                    encodeQuestion.Append(charMap[c]);
                } else {
                    encodeQuestion.Append(c);
                }
            }
            string queryQuestion = encodeQuestion.ToString();
            HttpWebRequest request = getRequest(queryQuestion, proxy);
            Stopwatch watch = new Stopwatch();
            watch.Start();

            await Task.Run(async () =>
            {
                try
                {
                    using(HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
                    {
                        token.ThrowIfCancellationRequested();
                        foreach (Cookie cookie in response.Cookies)
                        {
                            CookiesContainer = new CookieContainer();
                            CookiesContainer.Add(cookie);
                        }
                        if (response.ContentEncoding.ToLower().Contains("gzip"))
                        {
                            using (GZipStream stream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }
                            }
                        } else if (response.ContentEncoding.ToLower().Contains("deflate")) {
                            using (DeflateStream stream = new DeflateStream(response.GetResponseStream(), CompressionMode.Decompress))
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {
                                    pageSource = reader.ReadToEnd();
                                }

                            }
                        } else {
                            using (Stream stream = response.GetResponseStream())
                            {
                                using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                                {

                                    pageSource= reader.ReadToEnd();
                                }
                            }
                        }
                    } 
                    request.Abort();
                    watch.Stop();
                    var milliseconds = watch.ElapsedMilliseconds;
                    onCompleted(this, new OnCompletedEventArgs
                    {
                        Question = question,
                        PageSource = pageSource,
                        Milliseconds = milliseconds
                    });
                } catch (Exception e) {
                    onError(this, new OnErrorEventArgs
                    {
                        Question = question,
                        Error = e
                    });
                }
            });
            return results;
        }
        private static string ClearString(string origin)
        {
            Regex reElement = new Regex(@"\<[^\>]*?\>");
            return reElement.Replace(origin, "");
        }
    }
}