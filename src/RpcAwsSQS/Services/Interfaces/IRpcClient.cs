using System.Threading.Tasks;

namespace RpcAwsSQS.Services.Interfaces
{
    public interface IRpcClient
    {
        Task<TResponse> SendAndResponseAsync<TResponse, TRequest>(TRequest request, string queueName, int timeoutInSeconds = 5);
        Task SendAsync<TRequest>(TRequest request, string queueUrl);
    }
}
