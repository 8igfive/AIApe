using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Bot.WorkingModule
{
    public interface IGccHandlerFactory
    {
        Task<GccHandlerFactory.IGccHandler> CreateHandlerAsync();
    }

    public class GccHandlerFactory : IGccHandlerFactory
    {
        public string WorkDir { get; }
        public ILogger<GccHandlerFactory> Logger { get; }
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly ConcurrentDictionary<int, byte> used = new ConcurrentDictionary<int, byte>();
        private static int MinName { get; } = 1;
        private static int MaxName { get; } = int.MaxValue / 2;
        private int lastName = MinName;
        private bool needInit = true;

        public GccHandlerFactory(string workDir, ILogger<GccHandlerFactory> logger)
        {
            WorkDir = workDir;
            this.Logger = logger;
            logger.LogInformation("GccHandlerFactory use workdir={GccWorkDir}", Path.GetFullPath(workDir));
        }

        public async Task<IGccHandler> CreateHandlerAsync()
        {
            await semaphore.WaitAsync();
            if (needInit)
            {
                if (Directory.Exists(WorkDir))
                {
                    Logger.LogWarning("GccHandlerFactory cleanup workdir={GccWorkDir}", Path.GetFullPath(WorkDir));
                    Directory.Delete(WorkDir, recursive: true);
                }
                else
                {
                    Logger.LogInformation("GccHandlerFactory makedir {GccWorkDir}", Path.GetFullPath(WorkDir));
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
                GccHandler ret = new GccHandler(this, lastName);
                used.TryAdd(lastName, 0);
                return ret;
            }
            finally
            {
                semaphore.Release();
            }
        }

        private void Release(int name)
        {
            used.TryRemove(name, out _);
        }

        public interface IGccHandler : IDisposable
        {
            void CleanUp();
            Task<string> CompileAsync();
            Task CreatSourceFileAsync(string content);
        }

        private class GccHandler : IGccHandler
        {
            public string BasePath => Path.Combine(factory.WorkDir, Name.ToString());
            public string SourcePath => BasePath + ".c";
            public string ExecutablePath => BasePath + ".exe";
            public int Name { get; }
            private GccHandlerFactory factory;

            internal GccHandler(GccHandlerFactory factory, int name)
            {
                this.Name = name;
                this.factory = factory;
            }

            public void Dispose()
            {
                factory.Release(Name);
            }

            /// <summary>
            /// Create a source file using given content.
            /// </summary>
            /// <param name="content">C source. </param>
            /// <returns></returns>
            public async Task CreatSourceFileAsync(string content)
            {
                await File.WriteAllTextAsync(SourcePath, content);
            }

            /// <summary>
            /// Compile the source file. Make sure call <see cref="CreatSourceFileAsync(string)"/> before.
            /// </summary>
            /// <returns>The output info of gcc. </returns>
            public async Task<string> CompileAsync()
            {
                var info = new ProcessStartInfo("gcc");
                info.UseShellExecute = false;
                info.RedirectStandardOutput = true;
                info.RedirectStandardError = true;
                info.Arguments = $"-o {ExecutablePath} {SourcePath}";
                var process = Process.Start(info);
                var error = await process.StandardError.ReadToEndAsync();
                var output = await process.StandardOutput.ReadToEndAsync();
                if (error.Length == 0)
                {
                    if (output.Length == 0)
                    {
                        return string.Empty;
                    }
                    return output;
                }
                if (output.Length == 0)
                {
                    return error;
                }
                return error + '\n' + output;
            }

            /// <summary>
            /// Remove files at <see cref="SourcePath"/> and <see cref="ExecutablePath"/> if exists.
            /// </summary>
            public void CleanUp()
            {
                if (File.Exists(SourcePath))
                {
                    File.Delete(SourcePath);
                }
                if (File.Exists(ExecutablePath))
                {
                    File.Delete(ExecutablePath);
                }
            }
        }
    }
}
