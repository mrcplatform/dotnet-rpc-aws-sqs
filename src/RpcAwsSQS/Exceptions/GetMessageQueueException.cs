using System;

namespace RpcAwsSQS.Exceptions
{
    public class GetMessageQueueException : Exception
    {
        public GetMessageQueueException() { }

        public GetMessageQueueException(string message)
            : base(message) { }

        public GetMessageQueueException(string message, Exception inner)
           : base(message, inner) { }
    }
}
