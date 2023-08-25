using RpcAwsSQS.Services.Interfaces;
using System.Threading.Tasks;

namespace RpcAwsSQS.Services
{
    public sealed class SQSRpcClient : IRpcClient
    {
        private readonly IQueueCreater _sqsCreater;
        private readonly IMessageSender _messageSender;
        private readonly IMessageReceiver _messageReceiver;

        public SQSRpcClient(
            IQueueCreater sqsCreater,
            IMessageSender messageSender,
            IMessageReceiver messageReceiver)
        {
            _sqsCreater = sqsCreater;
            _messageSender = messageSender;
            _messageReceiver = messageReceiver;
        }

        public async Task<TResponse> SendAndResponseAsync<TResponse, TRequest>(TRequest request, string queueUrl, int timeoutInSeconds = 5)
        {
            var queueReplyUrl = await _sqsCreater.CreateTemporaryResponseQueue(timeoutInSeconds);
            
            await _messageSender.SendRPCMessageAsync(request, queueUrl, queueReplyUrl);

            var response = await _messageReceiver.StartReceiverPollingAsync<TResponse>(queueReplyUrl, timeoutInSeconds);

            return response;
        }
    }
}
