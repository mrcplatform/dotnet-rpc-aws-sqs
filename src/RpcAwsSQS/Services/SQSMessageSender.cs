using Amazon.SQS.Model;
using Amazon.SQS;
using RpcAwsSQS.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;
using RpcAwsSQS.Serializer.Interfaces;
using RpcAwsSQS.Services.Interfaces;

namespace RpcAwsSQS.Services
{
    public class SQSMessageSender : IMessageSender
    {
        private readonly IJsonSerializer _serializer;
        private readonly IAmazonSQS _amazonSqs;
        private readonly IQueueDeleter _queueDeleter;

        public SQSMessageSender(IJsonSerializer serializer,
            IAmazonSQS amazonSqs,
            IQueueDeleter queueDeleter)
        {
            _serializer = serializer;
            _amazonSqs = amazonSqs;
            _queueDeleter = queueDeleter;
        }

        public async Task SendMessageAsync<TRequest>(TRequest message, string queueUrl)
        {
            var messageBody = _serializer.Serialize(message);

            var requestMessage = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody
            };

            var requestResponse = await _amazonSqs.SendMessageAsync(requestMessage);

            if (requestResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new SendMessageQueueException("failed to send message");
            }
        }

        public async Task SendRPCMessageAsync<TRequest>(TRequest message, string queueUrl, string queueReplyUrl)
        {
            var messageBody = _serializer.Serialize(message);

            var requestMessage = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = messageBody,
                MessageAttributes = new Dictionary<string, MessageAttributeValue>()
                {
                    {
                        "ReplyTo",
                        new MessageAttributeValue
                        {
                            DataType = "String",
                            StringValue = queueReplyUrl
                        }
                    }
                }
            };

            var requestResponse = await _amazonSqs.SendMessageAsync(requestMessage);

            if (requestResponse.HttpStatusCode != System.Net.HttpStatusCode.OK)
            {
                _queueDeleter.ScheduleQueueDeletion(new DeleteQueueRequest()
                {
                    QueueUrl = queueReplyUrl
                });

                throw new SendMessageQueueException("failed to send message");
            }
        }
    }
}