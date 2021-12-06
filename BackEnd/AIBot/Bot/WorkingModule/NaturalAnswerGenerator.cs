using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Buaa.AIBot.Repository.Models;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Services;
using Microsoft.Extensions.Logging;
using Buaa.AIBot.Bot.Framework;
using System.Net;
using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Text;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public class NaturalAnswerGenerator
    {
        private DatabaseContext context;
        private INLPService nlpService;
        private CancellationToken cancellationToken;
        private ILogger<NaturalAnswerGenerator> logger;
        private Regex weatherRegex = new Regex("<input type=\"hidden\" id=\"hidden_title\" value=\"(?<weather>[^>\"]+)\" />");

        public NaturalAnswerGenerator(DatabaseContext context, INLPService nlpService, Utils.GlobalCancellationTokenSource gcts, ILogger<NaturalAnswerGenerator> logger)
        {
            this.context = context;
            this.cancellationToken = gcts.Token;
            this.nlpService = nlpService;
            this.logger = logger;
        }

        public async Task AddQuestionAndAnswersAsync(IEnumerable<string> questions, IEnumerable<string> answers)
        {
            var questionInfos = questions.Select(content => new NaturalQuestion() 
            { 
                Content = content,
                NatrualQuestionAnswerRelations = new List<NatrualQuestionAnswerRelation>()
            }).ToList();
            var answerInfos = answers.Select(content => new NatrualAnswer() 
            { 
                Content = content,
                NatrualQuestionAnswerRelations = new List<NatrualQuestionAnswerRelation>() 
            }).ToList();
            var relations = new List<NatrualQuestionAnswerRelation>();
            foreach (var q in questionInfos)
            {
                foreach (var a in answerInfos)
                {
                    var r = new NatrualQuestionAnswerRelation()
                    {
                        NatrualAnswer = a,
                        NaturalQuestion = q
                    };
                    a.NatrualQuestionAnswerRelations.Add(r);
                    q.NatrualQuestionAnswerRelations.Add(r);
                }
            }
            context.NatrualQuestions.AddRange(questionInfos);
            context.NatrualAnswers.AddRange(answerInfos);
            context.NatrualQuestionAnswerRelations.AddRange(relations);
            await context.SaveChangesAsync(cancellationToken);
            foreach (var q in questionInfos)
            {
                await nlpService.AddAsync(-q.NaturalQuestionId, q.Content, NLPService.Languages.Natrual);
            }
        }

        private string GenerateDate()
        {
            var date = DateTime.Now;
            return $"现在是北京时间{date.Year}年{date.Month}月{date.Day}日{date.Hour}时{date.Minute}分{date.Second}秒哟{Kaomojis.Happy}";
        }

        private async Task<string> GetHtmlForWeatherAsync()
        {
            string url = "http://www.weather.com.cn/weather1d/101010100.shtml";
            var request = WebRequest.CreateHttp(url);
            {
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
            }

            string ret = null;
            var response = (HttpWebResponse)await request.GetResponseAsync();
            {
                var stream = response.GetResponseStream();
                if (response.ContentEncoding != null)
                {
                    if (response.ContentEncoding.ToLower().Contains("gzip"))
                    {
                        using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                            {
                                ret = reader.ReadToEnd();
                            }
                        }
                    }
                    else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    {
                        using (DeflateStream deflate = new DeflateStream(stream, CompressionMode.Decompress))
                        {
                            using (StreamReader reader = new StreamReader(deflate, Encoding.UTF8))
                            {
                                ret = reader.ReadToEnd();
                            }
                        }
                    }
                }
                if (ret == null)
                {
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {

                        ret = reader.ReadToEnd();
                    }
                }
            }
            return ret;
        }

        private async Task<string> GenerateWeatherAsync()
        {
            string html = await GetHtmlForWeatherAsync();
            // logger.LogInformation("html={html}", html);
            var m = weatherRegex.Match(html);
            if (!m.Success)
            {
                return "小猿不太清楚呢，您可以到[url http://www.weather.com.cn]进行查看";
            }
            var info = m.Groups["weather"];
            return $"小猿在北京这边的天气情况是这样的：{info}，您可以到[url http://www.weather.com.cn]查看更多信息哟";
        }

        private async Task<string> FinalProduceAsync(string origin)
        {
            if (origin == "[call date]")
            {
                return GenerateDate();
            }
            if (origin == "[call weather]")
            {
                return await GenerateWeatherAsync();
            }
            origin = origin
                .Replace("[Happy]", Kaomojis.Happy)
                .Replace("[Sad]", Kaomojis.Sad)
                .Replace("[Cute]", Kaomojis.Cute)
                ;
            return origin;
        }

        public async Task<string> GetAnswerAsync(int qid)
        {
            if (qid < 0)
            {
                qid = -qid;
            }
            var answers = await context
                .NatrualQuestionAnswerRelations
                .Where(nqa => nqa.NaturalQuestionId == qid)
                .Select(nqa => nqa.NatrualAnswerId)
                .ToListAsync(cancellationToken);
            if (answers.Count == 0)
            {
                return "小猿出了点问题呜呜呜";
            }
            // logger.LogInformation("qid={qid}, answers={aids}", qid, answers);
            Random rnd = new Random();
            int aid = answers[rnd.Next(answers.Count)];
            var ret = await context
                .NatrualAnswers
                .Where(na => na.NatrualAnswerId == aid)
                .Select(na => na.Content)
                .SingleAsync(cancellationToken);
            return await FinalProduceAsync(ret);
        }
    }
}