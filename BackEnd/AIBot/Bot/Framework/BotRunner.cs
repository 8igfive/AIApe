using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buaa.AIBot.Bot.WorkingModule;

namespace Buaa.AIBot.Bot.Framework
{
    /// <summary>
    /// POCO of Bot's input.
    /// </summary>
    public class InputInfo
    {
        /// <summary>
        /// Message from user.
        /// </summary>
        public string Message { get; set; } 
    }

    /// <summary>
    /// POCO of Bot's output.
    /// </summary>
    public class OutputInfo
    {
        /// <summary>
        /// Message send to user.
        /// </summary>
        public IEnumerable<string> Messages { get; set; }

        /// <summary>
        /// Recommand the user to answer by choosing one of these.
        /// </summary>
        public IEnumerable<string> Prompt { get; set; }
    }

    /// <summary>
    /// Runner of a bot.
    /// </summary>
    public interface IBotRunner
    {
        /// <summary>
        /// Start a new Session and get the welcome message.
        /// </summary>
        /// <param name="userId">The identifier of the session.</param>
        /// <returns>Welcome message</returns>
        Task<OutputInfo> Start(int userId);

        /// <summary>
        /// Continue a new Session.
        /// </summary>
        /// <param name="userId">The identifier of the session.</param>
        /// <param name="input">The messages from user.</param>
        /// <returns>New messages to user.</returns>
        Task<OutputInfo> Run(int userId, InputInfo input);
    }

    /// <summary>
    /// Imformations for Building a <see cref="BotRunner{IdType}"/>.
    /// </summary>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public class BotRunnerOptions<IdType>
    {
        public IStatusContainerPool<IdType> StatusPool { get; set; }
        public IStatusBehaviourPool<IdType> BehaviourPool { get; set; }
        public BotStatus<IdType> InitStatus { get; set; }
    }

    /// <summary>
    /// Implement of <see cref="IBotRunner"/>.
    /// </summary>
    /// <remarks><seealso cref="IBotRunner"/></remarks>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public class BotRunner<IdType> : IBotRunner
    {
        private BotRunnerOptions<IdType> options;
        private IWorkingModule workingModule;

        private IStatusContainerPool<IdType> StatusPool => options.StatusPool;
        private IStatusBehaviourPool<IdType> BehaviourPool => options.BehaviourPool;

        /// <summary>
        /// Construct an Implement of <see cref="IBotRunner"/>.
        /// </summary>
        /// <param name="options"></param>
        public BotRunner(BotRunnerOptions<IdType> options, IWorkingModule workingModule)
        {
            if (options.StatusPool == null)
            {
                throw new ArgumentNullException(nameof(options.StatusPool));
            }
            if (options.BehaviourPool == null)
            {
                throw new ArgumentNullException(nameof(options.BehaviourPool));
            }
            if (options.InitStatus == null)
            {
                throw new ArgumentNullException(nameof(options.InitStatus));
            }
            this.options = options;
            this.workingModule = workingModule;
        }

        private BotStatus<IdType> InitStatus(int userId)
        {
            var origin = options.InitStatus;
            var status = new BotStatus<IdType>()
            {
                UserId = userId,
                Status = origin.Status
            };
            foreach (var pair in origin.Items)
            {
                status.Items.Add(pair.Key, pair.Value);
            }
            return status;
        }

        public async Task<OutputInfo> Start(int userId)
        {
            var status = InitStatus(userId);
            var sender = new Sender();
            var nextStatusBehaviour = BehaviourPool[status.Status];
            await nextStatusBehaviour.EnterAsync(status, new BotEnterContext()
            {
                Sender = sender,
                Worker = workingModule
            });
            await StatusPool.SaveStatusAsync(userId, status);
            return sender.DumpToOutputInfo();
        }

        public async Task<OutputInfo> Run(int userId, InputInfo input)
        {
            var receiver = new InputReceiverAdapter()
            {
                Input = input
            };
            var sender = new Sender();
            var status = await StatusPool.GetStatusAsync(userId);
            if (status == null)
            {
                sender.AddMessage("对话超时，将重新开始...\n");
                status = InitStatus(userId);
            }
            else
            {
                var currentStatusBehaviour = BehaviourPool[status.Status];
                status.Status = await currentStatusBehaviour.ExitAsync(status, new BotExitContext()
                {
                    Sender = sender,
                    Receiver = receiver,
                    Worker = workingModule
                });
            }
            var nextStatusBehaviour = BehaviourPool[status.Status];
            await nextStatusBehaviour.EnterAsync(status, new BotEnterContext() 
            { 
                Sender = sender, 
                Worker = workingModule 
            });
            await StatusPool.SaveStatusAsync(userId, status);
            return sender.DumpToOutputInfo();
        }

        private class InputReceiverAdapter : IBotReceiver
        {
            public string UserMessage => Input.Message;
            public InputInfo Input { get; set; }
        }

        private class Sender : IBotSender
        {
            public List<string> Messages { get; } = new List<string>();
            public List<string> Prompts { get; } = new List<string>();
            private StringBuilder MessageBuilder { get; } = new StringBuilder();

            public string Protect(string origin)
            {
                return origin.Replace("\\", "\\\\").Replace("[", "\\[").Replace("]", "\\]");
            }

            public IBotSender AddMessage(string message, bool newLine = true)
            {
                MessageBuilder.Append(Protect(message));
                if (newLine)
                {
                    MessageBuilder.Append('\n');
                }
                return this;
            }

            public IBotSender NewScope()
            {
                FreshMessageBuilder();
                return this;
            }

            public void FreshMessageBuilder()
            {
                if (MessageBuilder.Length > 0)
                {
                    Messages.Add(MessageBuilder.ToString());
                    MessageBuilder.Clear();
                }
            }

            public IBotSender AddPrompt(string prompt)
            {
                Prompts.Add(prompt);
                return this;
            }

            public IBotSender AddQuestion(int qid, bool newLine = true)
            {
                MessageBuilder.Append($"[question {qid}]");
                if (newLine)
                {
                    MessageBuilder.Append('\n');
                }
                return this;
            }

            public IBotSender AddUrl(string url, bool newLine = true)
            {
                MessageBuilder.Append($"[url {Protect(url)}]");
                if (newLine)
                {
                    MessageBuilder.Append('\n');
                }
                return this;
            }

            public OutputInfo DumpToOutputInfo()
            {
                FreshMessageBuilder();
                return new OutputInfo()
                {
                    Messages = Messages,
                    Prompt = Prompts
                };
            }
        }
    }
}
