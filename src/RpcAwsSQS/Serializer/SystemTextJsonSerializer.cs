using System;
using System.Text.Json;
using RpcAwsSQS.Serializer.Interfaces;

namespace RpcAwsSQS.Serializer
{
    public sealed class SystemTextJsonSerializer : IJsonSerializer
    {
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly JsonSerializerOptions _deserializeOptions;
        public SystemTextJsonSerializer()
        {
            _serializerOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            _deserializeOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public string Serialize<TRequest>(TRequest request)
        {
            if (request == null) throw new ArgumentNullException($"Request parameter cannot be null");

            return JsonSerializer.Serialize(request, _serializerOptions);
        }

        public TResponse Deserialize<TResponse>(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("Parameter cannot be null");

            return JsonSerializer.Deserialize<TResponse>(value, _deserializeOptions);
        }
    }
}
