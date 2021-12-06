using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Buaa.AIBot.TimedTask
{
    public class TimedTaskManager
    {
        private CancellationTokenSource globalCancellationTokenSource;
        private LoggerFactory loggerFactory;
        private List<TimedTask> TimedTasks = new List<TimedTask>();
        private ILogger<TimedTaskManager> logger;

        public class LoggerFactory
        {
            private IServiceProvider serviceProvider;

            public LoggerFactory(IServiceProvider serviceProvider)
            {
                this.serviceProvider = serviceProvider;
            }

            public ILogger<T> GetLogger<T>()
            {
                return serviceProvider.GetRequiredService<ILogger<T>>();
            }
        }

        private class TimedTask
        {
            public TimedTask(Func<CancellationToken, LoggerFactory, Task> task, TimeSpan intervalTime)
            {
                Function = task;
                IntervalTime = intervalTime;
            }

            public Func<CancellationToken, LoggerFactory, Task> Function { get; }
            public TimeSpan IntervalTime { get; }
            public DateTime LastStartingTime { get; set; }
            public CancellationTokenSource CancellationTokenSource { get; set; }
            public Task Task { get; set; }
            public DateTime NextStartingTime => LastStartingTime + IntervalTime;
            public bool IsReady() => NextStartingTime <= DateTime.Now;
            public string TaskName => Function.Method.Name;

            public void Start(CancellationToken ct, LoggerFactory loggerFactory)
            {
                if (CancellationTokenSource != null)
                {
                    CancellationTokenSource.Cancel();
                }
                CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(ct);
                LastStartingTime = DateTime.Now;
                Task = Function.Invoke(CancellationTokenSource.Token, loggerFactory);
            }
        }

        public TimedTaskManager(IHost host)
        {
            loggerFactory = new LoggerFactory(host.Services);
        }

        private async Task Init()
        {
            foreach (var timedTask in TimedTasks)
            {
                if (globalCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                await Task.Delay(TimeSpan.FromMinutes(2));
                if (globalCancellationTokenSource.IsCancellationRequested)
                {
                    return;
                }
                logger.LogInformation($"{nameof(TimedTaskManager)} starting task first times: {timedTask.Function.Method.Name}");
                timedTask.Start(globalCancellationTokenSource.Token, loggerFactory);
            }
        }

        private async void StartMainLoop()
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                logger ??= loggerFactory.GetLogger<TimedTaskManager>();
                await Init();
                while (!globalCancellationTokenSource.IsCancellationRequested)
                {
                    TimeSpan delayTime = TimeSpan.MaxValue;
                    foreach (var timedTask in TimedTasks)
                    {
                        if (globalCancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        await Task.Yield();
                        if (globalCancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                        if (timedTask.Task != null && timedTask.Task.IsCompleted)
                        {
                            var e =  timedTask.Task.Exception;
                            if (e != null)
                            {
                                logger.LogError("running {taskName} with exception: {exception}", timedTask.TaskName, e.InnerException);
                            }
                            timedTask.Task = null;
                        }
                        if (timedTask.IsReady())
                        {
                            logger.LogInformation($"{nameof(TimedTaskManager)} starting task: {timedTask.Function.Method.Name}");
                            timedTask.Start(globalCancellationTokenSource.Token, loggerFactory);
                        }
                        var waitingTime = timedTask.NextStartingTime - DateTime.Now;
                        if (waitingTime < delayTime)
                        {
                            delayTime = waitingTime;
                        }
                        if (globalCancellationTokenSource.IsCancellationRequested)
                        {
                            return;
                        }
                    }
                    await Task.Delay(delayTime, globalCancellationTokenSource.Token);
                }
            }
            catch (OperationCanceledException) { }
        }

        public void RegisterTask(Func<CancellationToken, LoggerFactory, Task> task, TimeSpan intervalTime)
        {
            if (globalCancellationTokenSource != null)
            {
                throw new InvalidOperationException($"Tring to register a timedTask to a running {nameof(TimedTaskManager)}");
            }
            TimedTasks.Add(new TimedTask(task, intervalTime));
        }

        public void Start()
        {
            if (globalCancellationTokenSource != null)
            {
                throw new InvalidOperationException($"Tring to start a running {nameof(TimedTaskManager)}");
            }
            globalCancellationTokenSource = new CancellationTokenSource();
            StartMainLoop();
        }

        public void Stop()
        {
            if (globalCancellationTokenSource == null)
            {
                throw new InvalidOperationException($"Tring to stop a not running {nameof(TimedTaskManager)}");
            }
            globalCancellationTokenSource.Cancel();
        }
    }
}
