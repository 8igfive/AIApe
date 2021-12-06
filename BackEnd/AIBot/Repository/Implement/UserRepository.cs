using Buaa.AIBot.Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Buaa.AIBot.Repository.Exceptions;
using Buaa.AIBot.Utils;

namespace Buaa.AIBot.Repository.Implement
{
    /// <summary>
    /// Implement of <see cref="IUserRepository"/>
    /// </summary>
    /// <remarks><seealso cref="IUserRepository"/></remarks>
    public class UserRepository : RepositoryBase, IUserRepository
    {
        public UserRepository(DatabaseContext context, ICachePool<int> cachePool, GlobalCancellationTokenSource globalCancellationTokenSource)
            : base(context, cachePool, globalCancellationTokenSource.Token) { }

        public async Task<UserInfo> SelectUserByIdAsync(int userId)
        {

            var user = await Context
                .Users
                .Select(u => new UserInfo()
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Bcrypt = u.Bcrypt,
                    Name = u.Name,
                    Auth = u.Auth,
                    ProfilePhoto = u.ProfilePhoto
                })
                .SingleOrDefaultAsync(user => user.UserId == userId, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return user;
        }

        public async Task<UserInfo> SelectUserByEmailAsync(string email)
        {
            var user = await Context
                .Users
                .Select(u => new UserInfo()
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Bcrypt = u.Bcrypt,
                    Name = u.Name,
                    Auth = u.Auth,
                    ProfilePhoto = u.ProfilePhoto
                })
                .SingleOrDefaultAsync(user => user.Email == email, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return user;
        }

        public async Task<UserInfo> SelectUserByNameAsync(string name)
        {
            var user = await Context
                .Users
                .Select(u => new UserInfo()
                {
                    UserId = u.UserId,
                    Email = u.Email,
                    Bcrypt = u.Bcrypt,
                    Name = u.Name,
                    Auth = u.Auth,
                    ProfilePhoto = u.ProfilePhoto
                })
                .SingleOrDefaultAsync(user => user.Name == name, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            return user;
        }

        public async Task<string> SelectBcryptByEmailAsync(string email)
        {
            var user = await Context
                .Users
                .Select(user => new { user.Email, user.Bcrypt })
                .SingleOrDefaultAsync(user => user.Email == email, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                return null;
            }
            return user.Bcrypt;
        }

        public async Task<IEnumerable<AnswerIdInfo>> SelectAnswersIdByIdAsync(int userId)
        {
            var query = await Context
                .Answers
                .Where(a => a.UserId == userId)
                .Select(a => new AnswerIdInfo()
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId
                })
                .ToListAsync(CancellationToken)
                ;
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var user = await Context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return query;
        }

        public async Task<IEnumerable<AnswerIdInfo>> SelectAnswersIdByIdByModifyTimeAsync(int userId)
        {
            var query = await Context
                .Answers
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.ModifyTime)
                .Select(a => new AnswerIdInfo()
                {
                    AnswerId = a.AnswerId,
                    QuestionId = a.QuestionId
                })
                .ToListAsync(CancellationToken)
                ;
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var user = await Context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return query;
        }

        public async Task<IEnumerable<int>> SelectQuestionsIdByIdAsync(int userId)
        {
            var query = await Context
                .Questions
                .Where(q => q.UserId == userId)
                .Select(q => q.QuestionId)
                .ToListAsync(CancellationToken)
                ;
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var user = await Context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return query;
        }

        public async Task<IEnumerable<int>> SelectQuestionsIdByIdOrderByModifyTimeAsync(int userId)
        {
            var query = await Context
                .Questions
                .Where(q => q.UserId == userId)
                .OrderByDescending(a => a.ModifyTime)
                .Select(q => q.QuestionId)
                .ToListAsync(CancellationToken)
                ;
            CancellationToken.ThrowIfCancellationRequested();
            if (query.Count != 0)
            {
                return query;
            }
            var user = await Context.Users.FindAsync(userId);
            if (user == null)
            {
                return null;
            }
            return query;
        }

        private async Task CheckInsert(UserInfo user)
        {
            var dup = await Context
                .Users
                .Select(u => new { u.Name, u.Email })
                .FirstOrDefaultAsync(u => u.Name == user.Name || u.Email == user.Email, CancellationToken);
            CancellationToken.ThrowIfCancellationRequested();
            if (dup != null)
            {
                if (dup.Email == user.Email)
                {
                    throw new EmailHasExistException(user.Email);
                }
                throw new NameHasExistException(user.Name);
            }
        }

        public async Task<int> InsertUserAsync(UserInfo user)
        {
            if (user.Bcrypt == null)
            {
                throw new ArgumentNullException(nameof(user.Bcrypt));
            }
            if (user.Name == null)
            {
                throw new ArgumentNullException(nameof(user.Name));
            }
            if (user.Email == null)
            {
                throw new ArgumentNullException(nameof(user.Email));
            }
            if (user.ProfilePhoto == null)
            {
                throw new ArgumentNullException(nameof(user.ProfilePhoto));
            }
            if (user.Auth == AuthLevel.None)
            {
                throw new ArgumentNullException(nameof(user.Auth));
            }
            if (user.Email.Length > Constants.UserEmailMaxLength)
            {
                throw new UserEmailToLongException(user.Email.Length, Constants.UserEmailMaxLength);
            }
            if (user.Name.Length > Constants.UserNameMaxLength)
            {
                throw new UserNameToLongException(user.Name.Length, Constants.UserNameMaxLength);
            }
            if (user.Bcrypt.Length != Constants.UserBcryptLength)
            {
                throw new UserBycryptLengthException(user.Bcrypt.Length, Constants.UserBcryptLength);
            }
            var target = new UserData()
            {
                Email = user.Email,
                Bcrypt = user.Bcrypt,
                Name = user.Name,
                Auth = user.Auth,
                ProfilePhoto = (int)user.ProfilePhoto
            };
            await CheckInsert(user);
            bool success = false;
            Context.Users.Add(target);
            while (!success)
            {
                success = await TrySaveChangesAgainAndAgainAsync();
                if (success)
                {
                    break;
                }
                await CheckInsert(user);
            }
            return target.UserId;
        }

        public async Task UpdateUserAsync(UserInfo user)
        {
            var target = await Context.Users.FindAsync(user.UserId);
            if (target == null)
            {
                throw new UserNotExistException(user.UserId);
            }
            bool success = true;
            if (user.Bcrypt != null)
            {
                success = false;
                target.Bcrypt = user.Bcrypt;
                if (user.Bcrypt.Length != Constants.UserBcryptLength)
                {
                    throw new UserBycryptLengthException(user.Bcrypt.Length, Constants.UserBcryptLength);
                }
            }
            if (user.Name != null)
            {
                success = false;
                target.Name = user.Name;
                if (user.Name.Length > Constants.UserNameMaxLength)
                {
                    throw new UserNameToLongException(user.Name.Length, Constants.UserNameMaxLength);
                }
            }
            if (user.Auth != AuthLevel.None)
            {
                success = false;
                target.Auth = user.Auth;
            }
            if (user.ProfilePhoto != null)
            {
                success = false;
                target.ProfilePhoto = (int)user.ProfilePhoto;
            }
            while (!success)
            {
                if (user.Name != null)
                {
                    var old = await Context.Users
                        .Where(u => u.Name == user.Name)
                        .FirstOrDefaultAsync(CancellationToken);
                    CancellationToken.ThrowIfCancellationRequested();
                    if (old != null && old.UserId != user.UserId)
                    {
                        throw new NameHasExistException(user.Name);
                    }
                } 
                if ((await Context.Users.Where(u => u.UserId == user.UserId).FirstOrDefaultAsync(CancellationToken)) == null)
                {
                    throw new UserNotExistException(user.UserId);
                }
                success = await TrySaveChangesAgainAndAgainAsync();
            }
        }

        public async Task DeleteUserByIdAsync(int userId)
        {
            var target = await Context.Users.FindAsync(userId);
            if (target != null)
            {
                Context.Users.Remove(target);
                await SaveChangesAgainAndAgainAsync();
            }
        }
    }
}
