using Amazon.SQS.Model;
using Amazon.SQS;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using RpcAwsSQS.Services.Interfaces;
using System.Net;
using RpcAwsSQS.Exceptions;
using Microsoft.Extensions.Logging;

namespace RpcAwsSQS.Services
{
    public sealed class SQSQueueCreater : IQueueCreater
    {
        private readonly IAmazonSQS _amazonSqs;
        private readonly ILogger<SQSQueueCreater> _logger;

        public SQSQueueCreater(IAmazonSQS amazonSqs, ILogger<SQSQueueCreater> logger)
        {
            _amazonSqs = amazonSqs;
            _logger = logger;
        }

        public async Task<string> CreateTemporaryResponseQueue(int timeOutInSeconds = 5)
        {
            int queueExpireTime = timeOutInSeconds * 3;
            
            DateTime now = DateTime.UtcNow;

            var queueName = $"{Constants.RESPONSE_QUEUE_PREFIX}-{Guid.NewGuid()}";

            _logger.LogInformation($"[QueueCreater][CreateTemporaryResponseQueue] Create Queue Request {queueName}");

            var createQueueRequest = new CreateQueueRequest(queueName);

            createQueueRequest.Attributes = new Dictionary<string, string>
                    {
                        {"ReceiveMessageWaitTimeSeconds", "0"},
                        {"VisibilityTimeout", "0"}
                    };

            createQueueRequest.Tags.Add(Constants.KEY_TAG_CREATE_AT, now.ToString());
            createQueueRequest.Tags.Add(Constants.KEY_TAG_EXPIRE_AT, now.AddSeconds(queueExpireTime).ToString());
            createQueueRequest.Tags.Add(Constants.KEY_TAG_TYPE, Constants.VALUE_TAG_TYPE);

            var createQueueResponse = await _amazonSqs.CreateQueueAsync(createQueueRequest);

            if(createQueueResponse.HttpStatusCode != HttpStatusCode.OK) 
            {
                throw new CreateQueueException($"[CreateResponseTempQueue] {queueName} fail with status {createQueueResponse.HttpStatusCode}");
            }

            _logger.LogInformation($"[QueueCreater][CreateTemporaryResponseQueue] Queue {queueName} created");

            return createQueueResponse.QueueUrl;
        }
    }
}
