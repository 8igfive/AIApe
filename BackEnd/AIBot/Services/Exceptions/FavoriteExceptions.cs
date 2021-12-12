using System;

namespace Buaa.AIBot.Services.Exceptions
{
    public class FavoriteException : Exception
    {
        public FavoriteException() : base() { }

        public FavoriteException(string msg) 
            : base(msg) { }

        public FavoriteException(string msg, Exception innerException) : base(msg, innerException) { }
    }

    public class FavoriteNameTooLongException : FavoriteException
    {
        public FavoriteNameTooLongException(int actual, int limit)
            : base($"Favorite name must be shorter than {limit}, but {actual} was given.") { }
    }

    public class FavoriteNotExistException : FavoriteException
    {
        public FavoriteNotExistException(int fid) 
            : base($"No favorite has fid={fid}.") { }

        public FavoriteNotExistException(int fid, Exception innerException)
            : base($"No favorite has fid={fid}.", innerException) { }
    }
}