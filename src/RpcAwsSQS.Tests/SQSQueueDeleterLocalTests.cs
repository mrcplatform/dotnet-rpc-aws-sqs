using Amazon.SQS;
using Amazon.SQS.Model;
using AutoFixture;
using Moq;
using Moq.AutoMock;
using RpcAwsSQS.Services;

namespace RpcAwsSQS.Tests
{
    public class SQSQueueDeleterLocalTests
    {
        [Fact]
        public void ScheduleQueueDeletion_Should_Add_DeleteQueueRequest_In_Queue_And_Should_Execute_DeleteQueueAsyn_With_Success_Only_Once()
        {
            // Arrange
            var mocker = new AutoMocker();
            var fixture = new Fixture();
            var mockSqsClient = mocker.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(sqs => sqs.DeleteQueueAsync(It.IsAny<DeleteQueueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteQueueResponse() { HttpStatusCode = System.Net.HttpStatusCode.OK });

            var sqsDeleter = mocker.CreateInstance<SQSQueueDeleterLocal>();
            var deleteRequest = fixture.Create<DeleteQueueRequest>();

            // Act
            sqsDeleter.ScheduleQueueDeletion(deleteRequest);
            sqsDeleter.WaitForProcessing(TimeSpan.FromSeconds(1));

            // Assert
            mockSqsClient.Verify(sqs => sqs.DeleteQueueAsync(It.IsAny<DeleteQueueRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public void ScheduleQueueDeletion_Should_Add_DeleteQueueRequest_In_Queue_And_Should_Execute_DeleteQueueAsyn_With_Fail_Two_Times_At_Least()
        {
            // Arrange
            var mocker = new AutoMocker();
            var fixture = new Fixture();
            var mockSqsClient = mocker.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(sqs => sqs.DeleteQueueAsync(It.IsAny<DeleteQueueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new DeleteQueueResponse() { HttpStatusCode = System.Net.HttpStatusCode.BadRequest });

            var sqsDeleter = mocker.CreateInstance<SQSQueueDeleterLocal>();
            var deleteRequest = fixture.Create<DeleteQueueRequest>();

            // Act
            sqsDeleter.ScheduleQueueDeletion(deleteRequest);
            sqsDeleter.WaitForProcessing(TimeSpan.FromMilliseconds(200));
            
            // Assert
            mockSqsClient.Verify(sqs => sqs.DeleteQueueAsync(It.IsAny<DeleteQueueRequest>(), It.IsAny<CancellationToken>()), Times.AtLeast(2));
        }
    }
}