using System;

namespace Buaa.AIBot.Repository.Models
{
    public class SearchResult
    {
        public string Title { get; set; }

        public string Url { get; set; }
    }

    public class OnStartEventArgs
    {
        public string Question { get; set; }
    }
    public class OnCompletedEventArgs
    {
        public string Question { get; set; }

        public string PageSource { get; set; }
        public long Milliseconds { get; set; }
    }

    public class OnErrorEventArgs
    {
        public string Question { get; set; }
        public Exception Error { get; set; }
    }
}