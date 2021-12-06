using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Repository.Exceptions
{
    public class RepositoryException : Exception
    {
        public RepositoryException() : base() { }
        public RepositoryException(string msg) : base(msg) { }
        public RepositoryException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public class EmailHasExistException : RepositoryException
    {
        public EmailHasExistException(string email) 
            : base($"User with email={email} has already existed.") { }
    }

    public class NameHasExistException : RepositoryException
    {
        public NameHasExistException(string name) 
            : base($"User with name={name} has already existed.") { }
    }

    public class TagNameHasExistException : RepositoryException
    {
        public TagNameHasExistException(string name) 
            : base($"Tag with name={name} has already existed.") { }
    }

    public class UserNotExistException : RepositoryException
    {
        public UserNotExistException(int userId) 
            : base($"User with uid={userId} is not exist.") { }
    }

    public class TagNotExistException : RepositoryException
    {
        public int TagId { get; }

        public TagNotExistException(int tagId) 
            : base($"Tag with tid={tagId} is not exist.")
        {
            TagId = tagId;
        }
    }

    public class QuestionNotExistException : RepositoryException
    {
        public QuestionNotExistException(int questionId) 
            : base($"Question with qid={questionId} is not exist.") { }
    }

    public class UserHasAnswerTheQuestionException : RepositoryException
    {
        public UserHasAnswerTheQuestionException(int userId, int questionId)
            : base($"User with uid={userId} has answer the question with qid={questionId}") { }
    }

    public class AnswerNotExistException : RepositoryException
    {
        public AnswerNotExistException(int answerId) 
            : base($"Answer with aid={answerId} is not exist.") { }
    }

    public class UserNameToLongException : RepositoryException
    {
        public UserNameToLongException(int actual, int max)
            : base($"User name has max length={max}, but {actual} get.") { }
    }

    public class UserEmailToLongException : RepositoryException
    {
        public UserEmailToLongException(int actual, int max)
            : base($"User email has max length={max}, but {actual} get.") { }
    }

    public class UserBycryptLengthException : RepositoryException
    {
        public UserBycryptLengthException(int actual, int require)
            : base($"User bcrypt expect length={require}, but {actual} get.") { }
    }

    public class QuestionTitleTooLongException : RepositoryException
    {
        public QuestionTitleTooLongException(int actual, int max)
            : base($"Question title has max length={max}, but {actual} get.") { }
    }

    public class TagNameTooLongException : RepositoryException
    {
        public TagNameTooLongException(int actual, int max)
            : base($"Tag name has max length={max}, but {actual} get.") { }
    }

    public class UserHasLikedTargetException : RepositoryException
    {
        public UserHasLikedTargetException(int uid, int target)
            : base($"User with uid={uid} has already liked target with id={target}.")
        { }
    }

    public class UserNotLikedTargetException : RepositoryException
    {
        public UserNotLikedTargetException(int uid, int target)
            : base($"User with uid={uid} has not liked target with id={target} yet.")
        { }
    }
}
