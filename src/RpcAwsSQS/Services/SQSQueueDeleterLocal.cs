using Amazon.SQS.Model;
using Amazon.SQS;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using RpcAwsSQS.Services.Interfaces;

namespace RpcAwsSQS.Services
{
    public sealed class SQSQueueDeleterLocal : IQueueDeleter
    {
        private BlockingCollection<DeleteQueueRequest> _deleteQueueRequests;
        private readonly IAmazonSQS _sqsClient;
        private readonly ILogger<SQSQueueDeleterLocal> _logger;
        private ManualResetEvent _processedEvent = new ManualResetEvent(false);

        public SQSQueueDeleterLocal(IAmazonSQS sqsClient, ILogger<SQSQueueDeleterLocal> logger)
        {
            _sqsClient = sqsClient;
            _deleteQueueRequests = new BlockingCollection<DeleteQueueRequest>();
            _logger = logger;

            Task.Run(() => ProcessDeleteQueueRequests());
        }

        public void ScheduleQueueDeletion(DeleteQueueRequest deleteQueueRequest)
        {
            _logger.LogInformation($"[ScheduleQueueDeletion] {deleteQueueRequest.QueueUrl}");

            _deleteQueueRequests.Add(deleteQueueRequest);
        }

        private async Task ProcessDeleteQueueRequests()
        {
            _logger.LogInformation($"[ProcessDeleteQueueRequests] start");
            
            foreach (var deleteQueueRequest in _deleteQueueRequests.GetConsumingEnumerable())
            {
                try
                {
                    _logger.LogInformation($"[ProcessDeleteQueueRequests] delete {deleteQueueRequest.QueueUrl}");
                    
                    var result = await _sqsClient.DeleteQueueAsync(deleteQueueRequest);
                    
                    if(result.HttpStatusCode != System.Net.HttpStatusCode.OK)
                    {
                        _logger.LogInformation($"[ProcessDeleteQueueRequests] delete fail status {result.HttpStatusCode} requeue");

                        _deleteQueueRequests.Add(deleteQueueRequest);
                    }

                    _processedEvent.Set();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, ex.ToString());
                }
            }
        }

        public void WaitForProcessing(TimeSpan timeout)
        {
            _processedEvent.WaitOne(timeout);
        }
    }
}
