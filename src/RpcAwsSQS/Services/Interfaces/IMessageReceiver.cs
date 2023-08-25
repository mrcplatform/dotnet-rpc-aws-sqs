using RpcAwsSQS.DTO;
using System.Threading;
using System.Threading.Tasks;

namespace RpcAwsSQS.Services.Interfaces
{
    public interface IMessageReceiver
    {
        Task<TResponse> StartReceiverPollingAsync<TResponse>(string queueReplyUrl, int timeoutInSeconds);
        Task<ReceiveMessageResponse<TResponse>> GetMessageAsync<TResponse>(string queueUrl);
    }
}