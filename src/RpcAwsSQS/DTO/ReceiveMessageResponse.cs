namespace RpcAwsSQS.DTO
{
    public sealed class ReceiveMessageResponse<TResponse>
    {
        public string ReceiptHandle { get; set; }
        public TResponse Data { get; set; }

        internal static ReceiveMessageResponse<TResponse> Create(string receiptHandle, TResponse response)
        {
            return new ReceiveMessageResponse<TResponse>()
            {
                ReceiptHandle = receiptHandle,
                Data = response
            };
        }
    }
}
