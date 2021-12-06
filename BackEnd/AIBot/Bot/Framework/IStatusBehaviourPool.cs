using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Bot.Framework
{
    /// <summary>
    /// A pool to store all <see cref="IBotStatusBehaviour{IdType}"/> for this bot.
    /// </summary>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public interface IStatusBehaviourPool<IdType>
    {
        IBotStatusBehaviour<IdType> this[IdType id] { get; }
    }

    /// <summary>
    /// Implement of <see cref="IStatusBehaviourPool{IdType}"/>.
    /// </summary>
    /// <seealso cref="IStatusBehaviourPool{IdType}"/>
    /// <typeparam name="IdType">The type to mark a status, usually an enum.</typeparam>
    public class StatusBehaviourPool<IdType> : IStatusBehaviourPool<IdType>
    {
        public IBotStatusBehaviour<IdType> this[IdType id] { get => pool[id]; }

        private Dictionary<IdType, IBotStatusBehaviour<IdType>> pool;

        public StatusBehaviourPool(Dictionary<IdType, IBotStatusBehaviour<IdType>> pool)
        {
            this.pool = pool;
        }
}
}
