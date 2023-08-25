using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Amazon.Runtime;
using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RpcAwsSQS.Serializer;
using RpcAwsSQS.Serializer.Interfaces;
using RpcAwsSQS.Services;
using RpcAwsSQS.Services.Interfaces;

namespace RpcAwsSQS.Extensions
{
    public static class DependencyExtensions
    {
        public static IServiceCollection AddRpcClient(this IServiceCollection services, IConfiguration configuration)
        {
            AWSOptions awsOptions = configuration.GetAWSOptions();
            
            services.AddDefaultAWSOptions(awsOptions);
            
            services.AddAWSService<IAmazonSQS>();

            services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

            services.AddScoped<IMessageReceiver, SQSMessageReceiver>();

            services.AddScoped<IMessageSender, SQSMessageSender>();

            services.AddScoped<IQueueCreater, SQSQueueCreater>();

            services.AddScoped<IQueueDeleter, SQSQueueDeleterLocal>();

            services.AddScoped<IRpcClient, SQSRpcClient>();

            return services;
        }

        public static IServiceCollection AddRpcClient(this IServiceCollection services, string awsKey, string awsSecretKey, string awsRegion)
        {
            AWSOptions awsOptions = new AWSOptions()
            {
                Credentials = new BasicAWSCredentials(awsKey, awsSecretKey),
                Region = RegionEndpoint.GetBySystemName(awsRegion)
            };


            services.AddDefaultAWSOptions(awsOptions);

            services.AddAWSService<IAmazonSQS>();

            services.AddSingleton<IJsonSerializer, SystemTextJsonSerializer>();

            services.AddScoped<IMessageReceiver, SQSMessageReceiver>();

            services.AddScoped<IMessageSender, SQSMessageSender>();

            services.AddScoped<IQueueCreater, SQSQueueCreater>();

            services.AddScoped<IQueueDeleter, SQSQueueDeleterLocal>();

            services.AddScoped<IRpcClient, SQSRpcClient>();

            return services;
        }
    }
}
