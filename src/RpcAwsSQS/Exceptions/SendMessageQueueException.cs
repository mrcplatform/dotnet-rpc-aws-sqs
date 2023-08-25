using System;

namespace RpcAwsSQS.Exceptions
{
    public class SendMessageQueueException : Exception
    {
        public SendMessageQueueException() { }

        public SendMessageQueueException(string message) 
            : base(message) { }

        public SendMessageQueueException(string message, Exception inner)
           : base(message, inner) { }
    }
}
