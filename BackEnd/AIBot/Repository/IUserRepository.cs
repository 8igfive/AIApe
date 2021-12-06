using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buaa.AIBot.Repository.Models;
using Buaa.AIBot.Repository.Exceptions;

namespace Buaa.AIBot.Repository
{
    public interface IUserRepository
    {
        /// <summary>
        /// Select a User by uid.
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns>a UserInfo object if exist, or null</returns>
        Task<UserInfo> SelectUserByIdAsync(int userId);

        /// <summary>
        /// Select a User by email.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>a UserInfo object if exist, or null</returns>
        Task<UserInfo> SelectUserByEmailAsync(string email);

        /// <summary>
        /// Select a User by name.
        /// </summary>
        /// <param name="name">name</param>
        /// <returns>a UserInfo object if exist, or null</returns>
        Task<UserInfo> SelectUserByNameAsync(string name);

        /// <summary>
        /// Select the bcrypt result of a User by email.
        /// </summary>
        /// <param name="email">email</param>
        /// <returns>a bvrypt object if exist, or null</returns>
        Task<string> SelectBcryptByEmailAsync(string email);

        /// <summary>
        /// Select all qid by uid;
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns>list of qid, may empty. null if not exist</returns>
        Task<IEnumerable<int>> SelectQuestionsIdByIdAsync(int userId);

        /// <summary>
        /// Select all qid by uid, sorted by modifyTime, the first is the latest.
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns>list of qid, may empty. null if not exist</returns>
        Task<IEnumerable<int>> SelectQuestionsIdByIdOrderByModifyTimeAsync(int userId);

        /// <summary>
        /// Select all qid by uid;
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns>list of aid, may empty. null if not exist</returns>
        Task<IEnumerable<AnswerIdInfo>> SelectAnswersIdByIdAsync(int userId);

        /// <summary>
        /// Select all qid by uid, sorted by modifyTime, the first is the latest.
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns>list of aid, may empty. null if not exist</returns>
        Task<IEnumerable<AnswerIdInfo>> SelectAnswersIdByIdByModifyTimeAsync(int userId);

        /// <summary>
        /// Insert a new user. UserId will be generated automatically.
        /// </summary>
        /// <remarks>
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Email, Bcrypt, ProfilePhoto, or Name is null. or Auth is None.</exception>
        /// <exception cref="UserEmailToLongException">Email lonnger than <see cref="Constants.UserEmailMaxLength"/>.</exception>
        /// <exception cref="UserNameToLongException">Name longger than <see cref="Constants.UserNameMaxLength"/>.</exception>
        /// <exception cref="UserBycryptLengthException">Bcrypt.length not equals to <see cref="Constants.UserBcryptLength"/>.</exception>
        /// <exception cref="EmailHasExistException">There is already a user using the same email with <paramref name="user"/>.Email</exception>
        /// <exception cref="NameHasExistException">There is already a user using the same name with <paramref name="user"/>.Name</exception>
        /// <param name="user">the new user to store</param>
        /// <returns>uid</returns>
        Task<int> InsertUserAsync(UserInfo user);

        /// <summary>
        /// Update the user with <paramref name="user"/>.UserId. 
        /// </summary>
        /// <remarks>
        /// Use <paramref name="user"/>.UserId to appoint the user to be update
        /// Every <paramref name="user"/>'s not-null (for Auth, AuthLevel.None) Property will replace the old value.
        /// Email will never be change although <paramref name="user"/>.Email is not null.
        /// No operation if any exception occurs.
        /// </remarks>
        /// <exception cref="UserNameToLongException">Name longger than <see cref="Constants.UserNameMaxLength"/>.</exception>
        /// <exception cref="UserBycryptLengthException">Bcrypt.length not equals to <see cref="Constants.UserBcryptLength"/>.</exception>
        /// <exception cref="UserNotExistException">There is no user with uesr.UserId == <paramref name="user"/>.UserId</exception>
        /// <exception cref="NameHasExistException">There is already a user using the same name with <paramref name="user"/>.Name</exception>
        /// <param name="user">the new info for the user</param>
        /// <returns></returns>
        Task UpdateUserAsync(UserInfo user);

        /// <summary>
        /// Make sure no user whose Id is <paramref name="userId"/>. (no operation if it has already not exist).
        /// </summary>
        /// <param name="userId">uid</param>
        /// <returns></returns>
        Task DeleteUserByIdAsync(int userId);
    }
}
