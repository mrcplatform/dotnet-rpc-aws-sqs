![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/mrcplatform/dotnet-rpc-aws-sqs/main.yml?logo=github&label=homolog&cacheSeconds=60)
[![codecov Homolog](https://codecov.io/gh/mrcplatform/dotnet-rpc-aws-sqs/branch/homolog/graph/badge.svg?token=KHJ98TFEO9)](https://codecov.io/gh/mrcplatform/dotnet-rpc-aws-sqs)

![GitHub Workflow Status (with event)](https://img.shields.io/github/actions/workflow/status/mrcplatform/dotnet-rpc-aws-sqs/main.yml?logo=github&label=main&cacheSeconds=60)
[![codecov Main](https://codecov.io/gh/mrcplatform/dotnet-rpc-aws-sqs/branch/main/graph/badge.svg?token=KHJ98TFEO9)](https://codecov.io/gh/mrcplatform/dotnet-rpc-aws-sqs)

## DOTNET RPC AWS SQS
Implementation of the RPC pattern with SQS queue

#### How to use
Resolve Dependency Injection with extension method **AddRpcClient**
```csharp
Services.AddRpcClient(awsKey: "yourKey", awsSecretKey:"yourSecret", awsRegion: "yourAwsRegion");
```

You can also use the [WSSDK.Extensions.NETCore.Setup](https://docs.aws.amazon.com/sdk-for-net/v3/developer-guide/net-dg-config-netcore.html) structure to resolve credentials
```csharp
Services.AddRpcClient(configuration: Configuration)
```

Use the client **IRpcClient** to Send And Response or just Send messages

```csharp
 public class ServiceExemple
 {
    private readonly IRpcClient _rpcClient;
    private readonly int _timeOutInSeconds = 120;

    public ServiceExemple(IRpcClient rpcClient)
    {
        _rpcClient = rpcClient;
    }

    public Task SendAndResponseExemple()
    {
        var response = await _rpcClient.SendAndResponseAsync<ResponseDto, RequestDto>(message, queueUrl, _timeOutInSeconds);
    }

    public void SendExemple()
    {
        await _rpcClient.SendAsync(message, queueUrl);
    }
 }
```

> For now, the queue deletion routine is coupled to the client running in the background, the idea is to parameterize it to be able to create a decoupled queue deleter or use the local queue deleter

### Exceptions throws

- **CreateQueueException** - thrown when it is not possible to create the temporary response queue

- **GetMessageQueueException** - thrown when unable to get message from queue

- **SendMessageQueueException** - thrown when it is not possible to publish message to the queue

- **TimeoutException** - thrown when the timeout received by parameter is reached

### Temporary Response Queue

- Queue Name  
`__responseQueue-Guid`

- Queue Tags  
`__createAt: 'dd/MM/yyyy HH:mm:ss'(UTC)`   
`__expireAt: 'dd/MM/yyyy HH:mm:ss' (createAt + timeOutInSeconds * 3)`  
`__type: 'responseTempQueue'`  
