using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;
using Microsoft.Extensions.Logging;

namespace Buaa.AIBot.Repository.Implement
{
    /// <summary>
    /// Implement of <see cref="IFavoriteRepository"/>.
    /// </summary>
    /// <remarks><seealso cref="IFavoriteRepository"/></remarks>
    public class FavoriteRepository : RepositoryBase, IFavoriteRepository
    {
        private ILogger<FavoriteRepository> logger;

        public FavoriteRepository(DatabaseContext context, ICachePool<int> cachePool, GlobalCancellationTokenSource globalCancellationTokenSource)
            : base(context, cachePool, globalCancellationTokenSource.Token)
        { }

        public FavoriteRepository(DatabaseContext context, ICachePool<int> cachePool, GlobalCancellationTokenSource globalCancellationTokenSource, ILogger<FavoriteRepository> logger)
            : base(context, cachePool, globalCancellationTokenSource.Token)
        {
            this.logger = logger;
        }

        private async Task CheckInsertAsync(FavoriteInfo favorite)
        {
            var user = await Context
                .Users
                .Where(u => u.UserId == favorite.CreatorId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new UserNotExistException(favorite.CreatorId);
            }
        }

        private async Task CheckUpdateAsync(FavoriteData target, FavoriteInfo favorite)
        {
            if ((await Context.Favorites.FindAsync(favorite.FavoriteId)) == null)
            {
                throw new FavoriteNotExistException(favorite.FavoriteId);
            }
        }

        public async Task<int> InsertFavoriteAsync(FavoriteInfo favorite)
        {
            if (favorite.Name.Length > Constants.FavoriteNameMaxLength)
            {
                throw new FavoriteNameTooLongException(favorite.Name.Length, Constants.FavoriteNameMaxLength);
            }
            await CheckInsertAsync(favorite);
            var target = new FavoriteData
            {
                UserId = favorite.CreatorId,
                Name = favorite.Name,
                Description = favorite.Description
            };
            Context.Favorites.Add(target);
            bool success = false;
            while (!success)
            {
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                await CheckInsertAsync(favorite);
            }
            int fid = target.FavoriteId;
            return fid;
        }

        public async Task<FavoriteInfo> SelectFavoriteByIdAsync(int favoriteId)
        {
            var favorite = await Context
                .Favorites
                .Select(f => new FavoriteInfo
                {
                    FavoriteId = f.FavoriteId,
                    CreatorId = f.UserId,
                    Name = f.Name,
                    Description = f.Description,
                    CreateTime = f.CreateTime
                })
                .Where(f => f.FavoriteId == favoriteId)
                .SingleOrDefaultAsync(CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return favorite;
        }

        public async Task DeleteFavoriteByIdAsync(int favoriteId)
        {
            var target = await Context.Favorites.FindAsync(favoriteId);
            if (target != null)
            {
                Context.Favorites.Remove(target);
                await SaveChangesAgainAndAgainAsync();
            }
        }
    
        public async Task UpdateFavoriteAsync(FavoriteInfo favorite)
        {
            var target = await Context.Favorites.FindAsync(favorite.FavoriteId);
            if (target == null)
            {
                throw new FavoriteNotExistException(favorite.FavoriteId);
            }
            bool success = true;
            if (favorite.Name != null)
            {
                success = false;
                if (favorite.Name.Length > Constants.FavoriteNameMaxLength)
                {
                    throw new FavoriteNameTooLongException(favorite.Name.Length, Constants.FavoriteNameMaxLength);
                }
                target.Name = favorite.Name;
            }
            if (favorite.Description != null)
            {
                success = false;
                target.Description = favorite.Description;
            }
            while (!success)
            {
                await CheckUpdateAsync(target, favorite);
                success = await TrySaveChangesAgainAndAgainAsync();   
            }
        }
    }
}