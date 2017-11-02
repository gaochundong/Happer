using System;

namespace Happer.Pipelining
{
    public class RequestPipelinesException : Exception
    {
        public RequestPipelinesException(Exception innerException)
            : base("Request pipelines error occurred.", innerException)
        {
        }
    }
}
