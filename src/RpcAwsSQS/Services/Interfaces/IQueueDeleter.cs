using Amazon.SQS.Model;
using System;

namespace RpcAwsSQS.Services.Interfaces
{
    public interface IQueueDeleter
    {
        void ScheduleQueueDeletion(DeleteQueueRequest deleteQueueRequest);
        void WaitForProcessing(TimeSpan timeout);
    }

    public interface IMessageDeleter
    {
        void ScheduleMessageDeletion(DeleteMessageRequest deleteMessageRequest);
        void WaitForProcessing(TimeSpan timeout);
    }
}
