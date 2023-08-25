namespace RpcAwsSQS.Serializer.Interfaces
{
    public interface IJsonSerializer
    {
        string Serialize<TRequest>(TRequest value);
        TResponse Deserialize<TResponse>(string value);
    }
}
