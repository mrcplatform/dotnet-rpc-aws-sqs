using Amazon.SQS.Model;
using System.Threading;
using System;
using System.Threading.Tasks;
using RpcAwsSQS.Services.Interfaces;
using Amazon.SQS;
using RpcAwsSQS.Serializer.Interfaces;
using System.Linq;
using RpcAwsSQS.Exceptions;
using RpcAwsSQS.DTO;

namespace RpcAwsSQS.Services
{
    public class SQSMessageReceiver : IMessageReceiver
    {
        private readonly IQueueDeleter _sqsDeleter;
        private readonly IAmazonSQS _sqsClient;
        private readonly IJsonSerializer _serializer;

        public SQSMessageReceiver(
            IQueueDeleter queueDeleter,
            IJsonSerializer serializer,
            IAmazonSQS sqsClient)
        {
            _sqsDeleter = queueDeleter;
            _serializer = serializer;
            _sqsClient = sqsClient;
        }

        public async Task<TResponse> StartReceiverPollingAsync<TResponse>(string queueReplyUrl, int timeoutInSeconds)
        {
            ReceiveMessageResponse<TResponse> response = default;

            Task<DeleteQueueRequest> consumerPooling = Task.Run(async () =>
            {
                using (CancellationTokenSource cts = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutInSeconds)))
                {
                    while (true)
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            throw new TimeoutException($"timeOutInSeconds {timeoutInSeconds}");
                        }

                        response = await GetMessageAsync<TResponse>(queueReplyUrl);

                        if (response != null)
                        {
                            return new DeleteQueueRequest
                            {
                                QueueUrl = queueReplyUrl
                            };
                        }
                    }
                }
            });

            var result = await Task.WhenAll(consumerPooling);

            if (TaskFail(result, response))
            {
                throw new GetMessageQueueException("Failed to receive message");
            }

            _sqsDeleter.ScheduleQueueDeletion(result[0]);
          
            return response.Data;
        }

        private bool TaskFail<TResponse>(DeleteQueueRequest[] result, ReceiveMessageResponse<TResponse> response)
        {
            return result[0] is null || response.ReceiptHandle is null;
        }

        public async Task<ReceiveMessageResponse<TResponse>> GetMessageAsync<TResponse>(string queueUrl)
        {
            ReceiveMessageResponse<TResponse> message = default;

            var requestResponse = new ReceiveMessageRequest()
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 1
            };

            var response = await _sqsClient.ReceiveMessageAsync(requestResponse);

            if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new GetMessageQueueException($"Error in receiving message: HTTP {response.HttpStatusCode}");
            }

            if (!response.Messages.Any())
            {
                return message;
            }
            
            message = ReceiveMessageResponse<TResponse>.Create(response.Messages[0].ReceiptHandle, _serializer.Deserialize<TResponse>(response.Messages[0].Body));
            
            return message;
        }
    }
}