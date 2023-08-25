using Amazon.SQS.Model;
using Amazon.SQS;
using Moq.AutoMock;
using RpcAwsSQS.Services;
using Moq;
using System.Net;
using RpcAwsSQS.Exceptions;

namespace RpcAwsSQS.Tests
{
    public class SQSQueueCreaterTests
    {
        [Fact]
        public async Task CreateTemporaryResponseQueue_Should_Setup_CreateQueueRequest_Attributes_And_Tags_Correctly()
        {
            // Arrange
            var automoq = new AutoMocker();
            var mockSqsClient = automoq.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(s => s.CreateQueueAsync(It.Is<CreateQueueRequest>(r => CheckAttributesAndTags(r)), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateQueueResponse() { HttpStatusCode = HttpStatusCode.OK });

            var queueCreater = automoq.CreateInstance<SQSQueueCreater>();

            // Act
            await queueCreater.CreateTemporaryResponseQueue();

            // Assert
            mockSqsClient.Verify(s => s.CreateQueueAsync(It.IsAny<CreateQueueRequest>(), It.IsAny<CancellationToken>()), Times.Once());
        }

        [Fact]
        public async Task CreateTemporaryResponseQueue_Should_Throw_CreateQueueException_For_Not_Ok_Response()
        {
            // Arrange
            var automoq = new AutoMocker();
            var mockSqsClient = automoq.GetMock<IAmazonSQS>();
            mockSqsClient
                .Setup(s => s.CreateQueueAsync(It.IsAny<CreateQueueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new CreateQueueResponse() { HttpStatusCode = HttpStatusCode.BadRequest });

            var queueCreater = automoq.CreateInstance<SQSQueueCreater>();

            // Act
            Func<Task> act = async () => await queueCreater.CreateTemporaryResponseQueue();

            // Assert
            await Assert.ThrowsAsync<CreateQueueException>(act);
        }

        private bool CheckAttributesAndTags(CreateQueueRequest request)
        {
            bool attributesAreCorrect = request.Attributes["ReceiveMessageWaitTimeSeconds"] == "0"
                                     && request.Attributes["VisibilityTimeout"] == "0";

            bool tagsAreCorrect = request.Tags[Constants.KEY_TAG_CREATE_AT] != null
                               && request.Tags[Constants.KEY_TAG_EXPIRE_AT] != null
                               && request.Tags[Constants.KEY_TAG_TYPE] == Constants.VALUE_TAG_TYPE;

            return attributesAreCorrect && tagsAreCorrect;
        }
    }
}