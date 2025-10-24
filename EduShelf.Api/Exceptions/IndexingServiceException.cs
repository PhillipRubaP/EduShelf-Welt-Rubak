using System;

namespace EduShelf.Api.Exceptions
{
    public class IndexingServiceException : Exception
    {
        public IndexingServiceException(string message) : base(message)
        {
        }

        public IndexingServiceException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}