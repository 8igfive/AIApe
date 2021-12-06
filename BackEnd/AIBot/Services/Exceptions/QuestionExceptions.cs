using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Buaa.AIBot.Services.Exceptions
{
    public class QuestionException : Exception
    {
        public QuestionException() : base() { }

        public QuestionException(string msg) 
            : base(msg) { }

        public QuestionException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public class UserNotExistException : QuestionException
    {
        public UserNotExistException(int uid)
            : base($"No user has uid={uid}") { }
        public UserNotExistException(int uid, Exception innerException)
            : base($"No user has uid={uid}", innerException) { }
    }

    public class QuestionNotExistException : QuestionException
    {
        public QuestionNotExistException(int qid) 
            : base($"No question has qid={qid}.") { }

        public QuestionNotExistException(int qid, Exception innerException)
            : base($"No question has qid={qid}.", innerException) { }
    }

    public class AnswerNotExistException : QuestionException
    {
        public AnswerNotExistException(int aid)
            : base($"No answer has aid={aid}.") { }
        public AnswerNotExistException(int aid, Exception innerException)
            : base($"No answer has aid={aid}.", innerException) { }
    }

    public class TagNotExistException : QuestionException
    {
        public int TagId { get; }
        public TagNotExistException(int tid)
            : base($"No tag has tid={tid}.")
        {
            TagId = tid;
        }
        public TagNotExistException(int tid, Exception innerException)
            : base($"No tag has tid={tid}.", innerException)
        {
            TagId = tid;
        }
    }

    public class QuestionTitleTooLongException : QuestionException
    {
        public QuestionTitleTooLongException(int actual, int limit)
            : base($"Question title must be shorter than {limit}, but {actual} was given.") { }
    }

    public class UserHasAnswerTheQuestionException : QuestionException
    {
        public UserHasAnswerTheQuestionException(int uid, int qid)
            : base($"User with uid={uid} has already answered question with qid={qid}.") { }
        public UserHasAnswerTheQuestionException(int uid, int qid, Exception innerException)
            : base($"User with uid={uid} has already answered question with qid={qid}.", innerException) { }
    }

    public class UnknownTagCategoryException : QuestionException
    {
        public UnknownTagCategoryException(string category)
            : base($"unknown tag category: {category}.") { }
    }

    public class TagNameTooLongException : QuestionException
    {
        public TagNameTooLongException(int actual, int limit)
            : base($"Tag name must be shorter than {limit}, but {actual} was given.") { }
    }

    public class TagNameExistException : QuestionException
    {
        public TagNameExistException(string name)
            : base($"Tag name \"{name}\" has exist.") { }
        public TagNameExistException(string name, Exception innerException)
            : base($"Tag name \"{name}\" has exist.", innerException) { }
    }
}
