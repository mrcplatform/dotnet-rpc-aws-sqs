using Amazon.SQS.Model;
using Amazon.SQS;
using Moq;
using Moq.AutoMock;
using System.Net;
using AutoFixture;
using System.Text.Json;
using RpcAwsSQS.Services;
using RpcAwsSQS.Serializer.Interfaces;
using RpcAwsSQS.Services.Interfaces;
using RpcAwsSQS.Serializer;
using Amazon.Runtime;
using RpcAwsSQS.DTO;
using RpcAwsSQS.Exceptions;

namespace RpcAwsSQS.Tests
{
    public class SQSMessageReceiverTests
    {
        [Fact]
        public async Task StartReceiverPollingAsync_NoException_WhenSendMessageReturnsOk()
        {
            // Arrange
            var queueDeleteMock = new Mock<IQueueDeleter>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var messageReceiver = new SQSMessageReceiver(queueDeleteMock.Object, new SystemTextJsonSerializer(), sqsClientMock.Object);
            var receiveData = new ReceiveMessageResponse();
            var dataResponse = new ResponseData();
            receiveData.HttpStatusCode = HttpStatusCode.OK;
            receiveData.Messages = new List<Message>()
            {
                new Message()
                {
                    Body = JsonSerializer.Serialize(dataResponse),
                    ReceiptHandle = Guid.NewGuid().ToString()
                }
            };

            sqsClientMock
                .Setup(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(receiveData);

            // Act
            var result = await messageReceiver.StartReceiverPollingAsync<ResponseData>("queueReply", 5);

            // Assert
            Assert.Equal(dataResponse.Text, result.Text);
            queueDeleteMock.Verify(v => v.ScheduleQueueDeletion(It.IsAny<DeleteQueueRequest>()), Times.Once);
        }

        [Fact]
        public async Task StartReceiverPollingAsync_WhenReceiveMessageTakesTooLong_ShouldThrowTimeoutException()
        {
            // Arrange
            int timeoutInSeconds = 2;
            var queueDeleteMock = new Mock<IQueueDeleter>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var messageReceiver = new SQSMessageReceiver(queueDeleteMock.Object, new SystemTextJsonSerializer(), sqsClientMock.Object);
            
            var receiveData = new ReceiveMessageResponse();
            receiveData.HttpStatusCode = HttpStatusCode.OK;
            receiveData.Messages = new List<Message>();

            sqsClientMock
                .Setup(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(receiveData);

            // Act
            Func<Task> act = async () => await messageReceiver.StartReceiverPollingAsync<ResponseData>("queueReply", timeoutInSeconds);

            // Assert
            await Assert.ThrowsAsync<TimeoutException>(act);
            queueDeleteMock.Verify(v => v.ScheduleQueueDeletion(It.IsAny<DeleteQueueRequest>()), Times.Once);
        }

        [Fact]
        public async Task StartReceiverPollingAsync_WhenReceiveMessageReceiptHandleNull_ShouldThrowGetMessageQueueException()
        {
            // Arrange
            int timeoutInSeconds = 2;
            var queueDeleteMock = new Mock<IQueueDeleter>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var messageReceiver = new SQSMessageReceiver(queueDeleteMock.Object, new SystemTextJsonSerializer(), sqsClientMock.Object);
            var dataResponse = new ResponseData();
            var receiveData = new ReceiveMessageResponse();
            receiveData.HttpStatusCode = HttpStatusCode.OK;
            receiveData.Messages = new List<Message>()
            {
                new Message()
                {
                    ReceiptHandle = null,
                    Body = JsonSerializer.Serialize(dataResponse)
                }
            };

            sqsClientMock
                .Setup(s => s.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(receiveData);

            // Act
            Func<Task> act = async () => await messageReceiver.StartReceiverPollingAsync<ResponseData>("queueReply", timeoutInSeconds);

            // Assert
            await Assert.ThrowsAsync<GetMessageQueueException>(act);
            queueDeleteMock.Verify(v => v.ScheduleQueueDeletion(It.IsAny<DeleteQueueRequest>()), Times.Never);
        }
    }

    partial class ResponseData
    {
        public string Text { get; set; }
    }
}

