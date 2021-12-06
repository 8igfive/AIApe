using Buaa.AIBot.Bot.WorkingModule;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework
{
    /// <summary>
    /// The Sender used by <see cref="IBotStatusBehaviour{IdType}"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="IBotStatusBehaviour{IdType}"/> use this to send messages to the user.
    /// </remarks>
    public interface IBotSender
    {
        IBotSender AddMessage(string message, bool newLine = true);
        IBotSender AddUrl(string url, bool newLine = true);
        IBotSender AddQuestion(int qid, bool newLine = true);
        IBotSender NewScope();
        IBotSender AddPrompt(string prompt);
    }

    /// <summary>
    /// The Receiver used by <see cref="IBotStatusBehaviour{IdType}"/>.
    /// </summary>
    /// <remarks>
    /// <see cref="IBotStatusBehaviour{IdType}"/> use this to get messages from user.
    /// </remarks>
    public interface IBotReceiver
    {
        string UserMessage { get; }
    }

    public interface IBotEnterContext
    {
        IBotSender Sender { get; }
        IWorkingModule Worker { get; }
    }

    public class BotEnterContext : IBotEnterContext
    {
        public IBotSender Sender { get; set; }
        public WorkingModule.IWorkingModule Worker { get; set; }
    }

    public interface IBotExitContext
    {
        IBotReceiver Receiver { get; }
        IBotSender Sender { get; }
        IWorkingModule Worker { get; }
    }

    public class BotExitContext : IBotExitContext
    {
        public IBotSender Sender { get; set; }
        public IBotReceiver Receiver { get; set; }
        public WorkingModule.IWorkingModule Worker { get; set; }
    }

    /// <summary>
    /// The Behaviour of each status. It shoud not have any fields.
    /// </summary>
    /// <remarks>
    /// If you need to store any status information, using <see cref="IBotStatusContainer{IdType}"/>.
    /// </remarks>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public interface IBotStatusBehaviour<IdType>
    {
        /// <summary>
        /// The Id of this status.
        /// </summary>
        IdType Id { get; }

        /// <summary>
        /// When enter this status, this method id called.
        /// </summary>
        /// <remarks>
        /// In this Method, Send a message to the user.
        /// And you can get the response of the user in <see cref="ExitAsync(IBotStatusContainer{IdType}, IBotSender, IBotReceiver)"/>.
        /// </remarks>
        /// <param name="status"></param>
        /// <param name="sender"></param>
        /// <returns></returns>
        Task EnterAsync(IBotStatusContainer status, IBotEnterContext context);

        /// <summary>
        /// When receiving the response from user, this method is called.
        /// </summary>
        /// <remarks>
        /// In this Method, Get the message from user and decide next status.
        /// You can also send some message to the user.
        /// </remarks>
        /// <param name="status"></param>
        /// <param name="sender"></param>
        /// <param name="receiver"></param>
        /// <returns>Next status id.</returns>
        Task<IdType> ExitAsync(IBotStatusContainer status, IBotExitContext context);
    }
}
