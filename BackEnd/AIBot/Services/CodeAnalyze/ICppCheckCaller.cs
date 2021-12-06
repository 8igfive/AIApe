using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text;
using System.Xml;

namespace Buaa.AIBot.Services.CodeAnalyze
{

    public interface ICppCheckCaller : IDisposable
    {
        Task WriteFileAsync(string code);
        Task FormatAsync();
        Task<List<CppCheckResult>> CppCheckAnalyzeAsync();
        Task<string> ReadFileAsync();
        Task CleanUpAsync();
    }

    public interface ICppCheckCallerFactory
    {
        Task<ICppCheckCaller> CreateCallerAsync(CancellationToken cancellationToken);
    }

    public class CppCheckCallerFactory : ICppCheckCallerFactory
    {
        public string WorkDir { get; }
        public ILogger<ICppCheckCallerFactory> Logger { get; }
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<int, byte> used = new ConcurrentDictionary<int, byte>();
        private static int MinName { get; } = 1;
        private static int MaxName { get; } = int.MaxValue / 2;
        private int lastName = MinName;
        private bool needInit = true;

        public CppCheckCallerFactory(string workDir, ILogger<ICppCheckCallerFactory> logger)
        {
            WorkDir = Path.GetFullPath(workDir);
            Logger = logger;
            logger.LogInformation("CppCheckCallerFactory use workdir={GccWorkDir}", WorkDir);
        }

        private void Release(int name)
        {
            used.TryRemove(name, out _);
        }

        public async Task<ICppCheckCaller> CreateCallerAsync(CancellationToken cancellationToken)
        {
            await semaphore.WaitAsync();
            if (needInit)
            {
                if (Directory.Exists(WorkDir))
                {
                    Logger.LogWarning("CppCheckCallerFactory cleanup workdir={GccWorkDir}", Path.GetFullPath(WorkDir));
                    Directory.Delete(WorkDir, recursive: true);
                }
                else
                {
                    Logger.LogInformation("CppCheckCallerFactory makedir {GccWorkDir}", Path.GetFullPath(WorkDir));
                }
                Directory.CreateDirectory(WorkDir);
                needInit = false;
            }
            try
            {
                do
                {
                    lastName++;
                } while (used.ContainsKey(lastName));
                ICppCheckCaller ret = new CppCheckCaller(this, lastName, cancellationToken);
                used.TryAdd(lastName, 0);
                return ret;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private class CppCheckCaller : ICppCheckCaller
        {
            public string BasePath => Path.Combine(factory.WorkDir, Name.ToString());
            public string SourcePath => BasePath + ".c";
            public string SourcePathOrig => BasePath + ".c.orig";
            public int Name { get; }
            private CppCheckCallerFactory factory;
            private CancellationToken ct;

            public CppCheckCaller(CppCheckCallerFactory factory, int name, CancellationToken ct)
            {
                this.factory = factory;
                Name = name;
                this.ct = ct;
            }

            public Task WriteFileAsync(string code)
            {
                return File.WriteAllTextAsync(SourcePath, code + '\n', System.Text.Encoding.UTF8, ct);
            }

            public async Task FormatAsync()
            {
                var argumentBuilder = new StringBuilder();
                argumentBuilder
                    .Append(SourcePath).Append(' ')     // target file.
                    .Append("--style=allman ")          // basic style.
                    .Append("--indent=spaces=4 ")       // using spaces4 instead of tab.
                    .Append("--pad-oper ")              // add spaces around operators.
                    .Append("--pad-comma ")             // add space after comma.
                    .Append("--pad-header ")            // add space after header-keywords such as if, while, for.
                    .Append("--unpad-paren ")           // remove space arount paren.
                    .Append("--align-pointer=name ")    // make 'int* ptr' to 'int *ptr'.
                    .Append("--break-one-line-headers ")// expand one-line statements like 'if (a == b) { return a; }' into multi-lines.
                    .Append("--add-braces ")            // add brances for no-braces statements after headers.
                    ;

                var info = new ProcessStartInfo("astyle");
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.Arguments = argumentBuilder.ToString();
                var process = Process.Start(info);
                while (!process.HasExited)
                {
                    ct.ThrowIfCancellationRequested();
                    await Task.Delay(TimeSpan.FromMilliseconds(30));
                }
            }

            private async Task<string> GetXmlResultAsync()
            {
                var arguments = new StringBuilder();
                arguments.Append("--xml ");
                arguments.Append("--enable=");
                arguments.Append(string.Join(",",
                    Enum.GetValues<InfoLevel>()
                    .Where(l => l != InfoLevel.error)
                    .Select(l => l.ToString())));
                arguments.Append(' ');
                arguments.Append(SourcePath);
                var processStartInfo = new ProcessStartInfo("cppcheck")
                {
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = false,
                    Arguments = arguments.ToString()
                };
                var process = Process.Start(processStartInfo);
                var errorBuilder = new StringBuilder();
                {
                    int bufferSize = 1024;
                    char[] buffer = new char[bufferSize];
                    int length = bufferSize;
                    while (true)
                    {
                        length = await process.StandardError.ReadBlockAsync(new Memory<char>(buffer), ct);
                        ct.ThrowIfCancellationRequested();
                        if (length == 0)
                        {
                            break;
                        }
                        errorBuilder.Append(new Memory<char>(buffer, 0, length));
                    }
                }
                return errorBuilder.ToString();
            }

            public async Task<List<CppCheckResult>> CppCheckAnalyzeAsync()
            {
                var ret = new List<CppCheckResult>();
                var xmlContent = await GetXmlResultAsync();
                var doc = new XmlDocument();
                doc.LoadXml(xmlContent);
                var root = doc.DocumentElement;
                var errors = root.SelectSingleNode("errors");

                foreach (XmlNode item in errors)
                {
                    var location = item.SelectSingleNode("location");
                    Location locationInfo = null;
                    if (location != null)
                    {
                        locationInfo = new Location()
                        {
                            Line = int.Parse(location.Attributes["line"].Value),
                            Column = int.Parse(location.Attributes["column"].Value),
                            FileName = location.Attributes["file"].Value
                        };
                    }
                    var sym = item.SelectSingleNode("symbol");
                    var info = new CppCheckResult()
                    {
                        Category = item.Attributes["id"].Value,
                        Level = Enum.Parse<InfoLevel>(item.Attributes["severity"].Value),
                        Message = item.Attributes["verbose"].Value,
                        Symbol = sym?.InnerText,
                        Location = locationInfo
                    };
                    ret.Add(info);
                }

                // factory.Logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(ret));

                return ret;
            }

            public async Task<string> ReadFileAsync()
            {
                return await File.ReadAllTextAsync(SourcePath, ct);
            }

            public void Dispose()
            {
                factory.Release(Name);
            }

            public Task CleanUpAsync()
            {
                if (File.Exists(SourcePath))
                { 
                    File.Delete(SourcePath);
                }
                if (File.Exists(SourcePathOrig))
                {
                    File.Delete(SourcePathOrig);
                }
                return Task.CompletedTask;
            }
        }
    }
}
