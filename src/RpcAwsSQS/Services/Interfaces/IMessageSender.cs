using System.Threading.Tasks;

namespace RpcAwsSQS.Services.Interfaces
{
    public interface IMessageSender
    {
        Task SendRPCMessageAsync<TRequest>(TRequest message, string queueUrl, string queueReplyUrl);
    }
}