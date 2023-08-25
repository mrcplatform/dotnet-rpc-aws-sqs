using System.Threading.Tasks;

namespace RpcAwsSQS.Services.Interfaces
{
    public interface IQueueCreater
    {
        Task<string> CreateTemporaryResponseQueue(int timeOutInSeconds = 5);
    }
}
