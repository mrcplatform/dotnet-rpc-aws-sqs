using Amazon.SQS.Model;
using Amazon.SQS;
using Moq.AutoMock;
using System.Net;
using Moq;
using RpcAwsSQS.Serializer.Interfaces;
using RpcAwsSQS.Exceptions;
using RpcAwsSQS.Services;

namespace RpcAwsSQS.Tests
{
    public class SQSMessageSenderTests
    {
        
        [Fact]
        public async Task SendRPCMessageAsync_NoException_WhenSendMessageReturnsOk()
        {
            // Arrange
            var automoq = new AutoMocker();
            var mockSqsClient = automoq.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendMessageResponse() { HttpStatusCode = HttpStatusCode.OK });

            var mockSerialize = automoq.GetMock<IJsonSerializer>();
            mockSerialize
                .Setup(c => c.Serialize(It.IsAny<object>()))
                .Returns(It.IsAny<string>());

            var sendRpcMessage = automoq.CreateInstance<SQSMessageSender>();

            // Act
            await sendRpcMessage.SendRPCMessageAsync(new { Request = "teste"}, "queue", "queueReply");

            // Assert
            
        }

        [Fact]
        public async Task SendRPCMessageAsync_ShouldThrowException_WhenSendMessageAsyncReturnsNotOk()
        {
            // Arrange
            var automoq = new AutoMocker();
            var mockSqsClient = automoq.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SendMessageResponse() { HttpStatusCode = HttpStatusCode.BadRequest });

            var mockSerialize = automoq.GetMock<IJsonSerializer>();
            mockSerialize
                .Setup(c => c.Serialize(It.IsAny<object>()))
                .Returns(It.IsAny<string>());

            var sendRpcMessage = automoq.CreateInstance<SQSMessageSender>();

            // Act
            Func<Task> act = async () => await sendRpcMessage.SendRPCMessageAsync(new { Request = "teste" }, "queue", "queueReply");

            // Assert
            await Assert.ThrowsAsync<SendMessageQueueException>(act);
        }
    }
}
