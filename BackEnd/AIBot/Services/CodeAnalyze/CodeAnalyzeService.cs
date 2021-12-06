using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Utils;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Buaa.AIBot.Services.CodeAnalyze
{
    public interface ICodeAnalyzeService
    {
        Task<CodeAnalyzeService.CodeAnalyzeResult> AnalyzeAsync(string code);
    }

    public class CodeAnalyzeService : ICodeAnalyzeService
    {
        public class CodeAnalyzeResult
        {
            public class CodeAnalyzeMessage
            {
                [JsonConverter(typeof(EnumJsonConverter<InfoLevel>))]
                public InfoLevel Level { get; set; }
                public string Desc { get; set; }
                public int Line { get; set; }
                public int Column { get; set; }
            }

            public string SourceCode { get; set; }
            public List<CodeAnalyzeMessage> Messages { get; set; }
        }

        private ICppCheckCallerFactory cppCheckCallerFactory;
        private ILogger<CodeAnalyzeService> logger;
        private Utils.GlobalCancellationTokenSource globalCancellationTokenSource;
        private CppCheckResultTanslation tanslation;

        public CodeAnalyzeService(ICppCheckCallerFactory cppCheckCallerFactory, ILogger<CodeAnalyzeService> logger, GlobalCancellationTokenSource globalCancellationTokenSource, CppCheckResultTanslation tanslation)
        {
            this.cppCheckCallerFactory = cppCheckCallerFactory;
            this.logger = logger;
            this.globalCancellationTokenSource = globalCancellationTokenSource;
            this.tanslation = tanslation;
        }

        private static readonly IReadOnlySet<string> ignoredCategories = new HashSet<string>()
        {
            "missingIncludeSystem",
            "missingInclude"
        };

        private bool TryGetDescription(CppCheckResult cppCheckResult, out string res)
        {
            if (ignoredCategories.Contains(cppCheckResult.Category))
            {
                res = null;
                return false;
            }
            res = tanslation.Translate(cppCheckResult);
            return true;
        }

        public async Task<CodeAnalyzeResult> AnalyzeAsync(string code)
        {
            using (var cppcheck = await cppCheckCallerFactory.CreateCallerAsync(globalCancellationTokenSource.Token))
            {
                await cppcheck.WriteFileAsync(code);
                await cppcheck.FormatAsync();
                var fmtCode = await cppcheck.ReadFileAsync();
                var safeCode = Regex.Replace(fmtCode, "[ \t]*#[ \t]*include[ \t]*[\"<][\\w\\d \t\\\\/_\\.\\*]+[\">][ \t]*\n", "\n");
                await cppcheck.WriteFileAsync(safeCode);
                var res = await cppcheck.CppCheckAnalyzeAsync();
                await cppcheck.CleanUpAsync();
                var messages = new List<CodeAnalyzeResult.CodeAnalyzeMessage>();
                foreach (var message in res)
                {
                    if (TryGetDescription(message, out var desc))
                    {
                        int line = -1;
                        int column = -1;
                        if (message.Location != null)
                        {
                            line = message.Location.Line;
                            column = message.Location.Column;
                        }
                        messages.Add(new CodeAnalyzeResult.CodeAnalyzeMessage()
                        {
                            Level = message.Level,
                            Desc = desc,
                            Line = line,
                            Column = column
                        });
                    }
                }
                var ret = new CodeAnalyzeResult()
                {
                    SourceCode = fmtCode,
                    Messages = messages
                };
                logger.LogInformation('\n' + fmtCode);
                // logger.LogInformation(JsonSerializer.Serialize(ret));

                return ret;
            }
        }
    }
}
