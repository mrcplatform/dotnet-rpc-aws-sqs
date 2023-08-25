using System;

namespace RpcAwsSQS.Exceptions
{
    public class CreateQueueException : Exception
    {
        public CreateQueueException() { }

        public CreateQueueException(string message)
            : base(message) { }

        public CreateQueueException(string message, Exception inner)
            : base(message, inner) { }
    }
}
