using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository
{
    public interface IFavoriteRepository : IRepositoryBase
    {
        /// <summary>
        /// Insert a new favorite. FavoriteId, CreateTime will be generated automatically.
        /// </summary>
        /// <remarks>
        /// CreaterId is required.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">CreaterId is null</exception>
        /// <exception cref="FavoriteNameTooLongException">Name Length greater than limits.</exception>
        /// <exception cref="UserNotExistException">There is no user with uid=<paramref name="question"/>.CreaterId. </exception>
        /// <param name="favorite">the new question to store</param>
        /// <returns>fid</returns>
        Task<int> InsertFavoriteAsync(FavoriteInfo favorite);

        /// <summary>
        /// Select a favorite by id.
        /// </summary>
        /// <param name="favoriteId">qid</param>
        /// <returns>a FavoriteInfo object if exist, or null</returns>
        Task<FavoriteInfo> SelectFavoriteByIdAsync(int favoriteId);

        /// <summary>
        /// Make sure no favorite whose Id is <paramref name="favoriteId"/>. (no operation if it has already not exist).
        /// </summary>
        /// <param name="favoriteId">fid</param>
        /// <returns></returns>
        Task DeleteFavoriteByIdAsync(int favoriteId);

        /// <summary>
        /// Update the favorite with fid=<paramref name="favorite"/>.FavoriteId.
        /// </summary>
        /// <remarks>
        /// Use <paramref name="favorite"/>.FavoriteId to appoint the favorite to be update
        /// Every <paramref name="favorite"/>'s not-null Property will replace the old value.
        /// CreateTime, CreaterId will never change.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="FavoriteNotExistException">There is no favorite with given qid.</exception>
        /// <exception cref="FavoriteNameTooLongException">Name Length greater than limits.</exception>
        /// <param name="favorite">the new info for the favorite</param>
        /// <returns></returns>
        Task UpdateFavoriteAsync(FavoriteInfo favorite);
    }
}