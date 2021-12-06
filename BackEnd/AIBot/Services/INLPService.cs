using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Buaa.AIBot.Utils;
using System.Net;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace Buaa.AIBot.Services
{
    public interface INLPService
    {
        Task<double[]> AddAsync(int qid, string question, NLPService.Languages language);
        Task DeleteAsync(int qid);
        Task<List<double[]>> EmbeddingsAsync(List<string> sentences);
        Task<List<double[]>> EmbeddingsAsync(params string[] sentences);
        Task<List<Tuple<int, double>>> RetrievalAsync(string question, int num, IEnumerable<NLPService.Languages> languages);
        Task<string> SelectAsync(string reply, IEnumerable<string> prompts);
        Task<string> SelectAsync(string reply, params string[] prompts);
        Task<List<int>> CheckqidsAsync(IEnumerable<int> qids);
    }

    public class NLPService : INLPService
    {
        public class Options
        {
            public string BaseUrl { get; set; }
            public string Name { get; set; }
            public string Password { get; set; }
            public readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        }

        private ILogger<NLPService> logger;
        private GlobalCancellationTokenSource globalCancellationTokenSource;
        private Options options;


        public NLPService(ILogger<NLPService> logger, GlobalCancellationTokenSource globalCancellationTokenSource, Options options)
        {
            if (string.IsNullOrEmpty(options.BaseUrl))
            {
                throw new ArgumentNullException(nameof(options.BaseUrl));
            }
            if (string.IsNullOrEmpty(options.Name))
            {
                throw new ArgumentNullException(nameof(options.Name));
            }
            if (string.IsNullOrEmpty(options.Password))
            {
                throw new ArgumentNullException(nameof(options.Password));
            }
            this.logger = logger;
            this.globalCancellationTokenSource = globalCancellationTokenSource;
            this.options = options;
        }

        private class UnauthorizedException : Exception
        {
            public UnauthorizedException()
            {
            }

            public UnauthorizedException(string message) : base(message)
            {
            }

            public UnauthorizedException(string message, Exception innerException) : base(message, innerException)
            {
            }

            protected UnauthorizedException(SerializationInfo info, StreamingContext context) : base(info, context)
            {
            }
        }

        private async Task<string> PostResultAsync(string url, Dictionary<string, object> body)
        {
            await options.semaphoreSlim.WaitAsync();
            await Task.Delay(TimeSpan.FromSeconds(1));
            try
            {
                var fullUrl = options.BaseUrl + url;
                var request = WebRequest.CreateHttp(fullUrl);
                {
                    request.Method = "POST";
                    var stream = await request.GetRequestStreamAsync();
                    body["name"] = options.Name;
                    body["password"] = options.Password;
                    var json = JsonSerializer.Serialize(body);
                    request.ContentType = "application/json";
                    body["password"] = "__PASSWORD__";
                    logger.LogInformation("POST to {fullUrl} with body:{body}", fullUrl, JsonSerializer.Serialize(body));
                    await stream.WriteAsync(Encoding.UTF8.GetBytes(json));
                }

                string ret;
                var response = (HttpWebResponse)await request.GetResponseAsync();
                {
                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        logger.LogError("Unauthorized!");
                        throw new UnauthorizedException("wrong name or password when calling nlp-service");
                    }
                    var stream = response.GetResponseStream();
                    // if (response.ContentEncoding.ToLower().Contains("gzip"))
                    // {
                    //     using (GZipStream gzip = new GZipStream(stream, CompressionMode.Decompress))
                    //     {
                    //         using (StreamReader reader = new StreamReader(gzip, Encoding.UTF8))
                    //         {
                    //             ret = reader.ReadToEnd();
                    //         }
                    //     }
                    // }
                    // else if (response.ContentEncoding.ToLower().Contains("deflate"))
                    // {
                    //     using (DeflateStream deflate = new DeflateStream(stream, CompressionMode.Decompress))
                    //     {
                    //         using (StreamReader reader = new StreamReader(deflate, Encoding.UTF8))
                    //         {
                    //             ret = reader.ReadToEnd();
                    //         }

                    //     }
                    // }
                    // else
                    // {
                    // }
                    using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                    {

                        ret = reader.ReadToEnd();
                    }
                }
                return ret;
            }
            finally
            {
                options.semaphoreSlim.Release();
            }
        }

        #region embeddings

        private class EmbeddingResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public List<double[]> Embeddings { get; set; }
        }

        public Task<List<double[]>> EmbeddingsAsync(params string[] sentences)
        {
            return EmbeddingsAsync(sentences.ToList());
        }

        public async Task<List<double[]>> EmbeddingsAsync(List<string> sentences)
        {
            var body = new Dictionary<string, object>()
            {
                ["sentences"] = sentences
            };
            var json = await PostResultAsync("/api/embeddings", body);

            var res = JsonSerializer.Deserialize<EmbeddingResult>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response fail with message: {msg}", res.Message);
                return null;
            }
            return res.Embeddings;
        }

        #endregion

        #region retrieval

        public class RetrievalResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public List<List<double>> Results { get; set; }

            public override string ToString()
            {
                if (Results == null)
                {
                    return $"{nameof(RetrievalResult)}{{{nameof(Status)}={Status}, {nameof(Message)}={Message}, {nameof(Results)}=null}}";
                }
                string res = string.Join(", ", Results.Select(t => $"({t[0]}, {t[1]})"));
                return $"{nameof(RetrievalResult)}{{{nameof(Status)}={Status}, {nameof(Message)}={Message}, {nameof(Results)}=[{res}]}}";
            }
        }

        private class QidEqualityComparer : IEqualityComparer<Tuple<int, double>>
        {
            public bool Equals(Tuple<int, double> x, Tuple<int, double> y)
            {
                return x.Item1 == y.Item2;
            }

            public int GetHashCode([DisallowNull] Tuple<int, double> obj)
            {
                return obj.GetHashCode();
            }
        }

        public enum Languages
        {
            C, Java, Python, SQL, Natrual, Other
        }

        public async Task<List<Tuple<int, double>>> RetrievalAsync(string question, int num, IEnumerable<Languages> languages)
        {
            if (num <= 0)
            {
                throw new ArgumentException($"argument {nameof(num)} cannot be less than zero.");
            }
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException($"argument {nameof(question)} cannot be empty or whitespace.");
            }
            var body = new Dictionary<string, object>()
            {
                ["languages"] = languages.Select(l => l.ToString()).ToList(),
                ["presorting"] = false,
                ["question"] = question,
                ["number"] = num
            };
            var json = await PostResultAsync("/api/retrieval", body);
            var res = JsonSerializer.Deserialize<RetrievalResult>(json, new JsonSerializerOptions()
            { 
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response fail with message: {msg}", res.Message);
                return null;
            }
            return res.Results
                .Select(l => new Tuple<int, double>((int)l[0], l[1]))
                .Distinct(new QidEqualityComparer())
                .OrderByDescending(t => t.Item2)
                .ToList();
        }

        #endregion

        #region add

        private class AddResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public double[] Embedding { get; set; }
        }

        public async Task<double[]> AddAsync(int qid, string question, Languages language)
        {
            if (string.IsNullOrWhiteSpace(question))
            {
                throw new ArgumentException($"argument {nameof(question)} cannot be empty or whitespace.");
            }
            var body = new Dictionary<string, object>()
            {
                ["language"] = language.ToString(),
                ["question"] = question,
                ["qid"] = qid
            };
            var json = await PostResultAsync("/api/add", body);

            var res = JsonSerializer.Deserialize<AddResult>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response faile with message: {msg}", res.Message);
                return null;
            }
            return res.Embedding;
        }

        #endregion

        #region delete

        private class DeleteResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
        }

        public async Task DeleteAsync(int qid)
        {
            var body = new Dictionary<string, object>()
            {
                ["qid"] = qid
            };
            var json = await PostResultAsync("/api/delete", body);

            var res = JsonSerializer.Deserialize<DeleteResult>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response faile with message: {msg}", res.Message);
            }
        }

        #endregion

        #region select

        private class SelectResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public string Prompt { get; set; }
        }

        public Task<string> SelectAsync(string reply, params string[] prompts)
        {
            return SelectAsync(reply, prompts.ToList());
        }

        public async Task<string> SelectAsync(string reply, IEnumerable<string> prompts)
        {
            var body = new Dictionary<string, object>()
            {
                ["reply"] = reply,
                ["prompts"] = prompts
            };
            var json = await PostResultAsync("/api/select", body);

            var res = JsonSerializer.Deserialize<SelectResult>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response faile with message: {msg}", res.Message);
                return null;
            }
            return res.Prompt;
        }

        #endregion

        #region checkqids

        private class CheckqidsResult
        {
            public string Status { get; set; }
            public string Message { get; set; }
            public List<int> Qids { get; set; }
        }

        public async Task<List<int>> CheckqidsAsync(IEnumerable<int> qids)
        {
            var body = new Dictionary<string, object>()
            {
                ["qids"] = qids
            };
            var json = await PostResultAsync("/api/checkqids", body);
            logger.LogInformation("nlp-service response body: {msg}", json);

            var res = JsonSerializer.Deserialize<CheckqidsResult>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            if (res.Status == "fail")
            {
                logger.LogWarning("nlp-service response fail with message: {msg}", res.Message);
                return null;
            }
            return res.Qids;
        }


        #endregion
    }
}
