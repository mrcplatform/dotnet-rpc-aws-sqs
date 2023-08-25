using AutoFixture;
using RpcAwsSQS.Serializer;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace RpcAwsSQS.Tests
{
    public class SystemTextJsonSerializerTests
    {

        [Fact]
        public void Serialize_Should_Return_Valid_Json_String()
        {
            // Arrange
            var autoFixture = new Fixture();
            var requestData = autoFixture.Create<TesteDataObject>();
            var serializer = new SystemTextJsonSerializer();

            // Act
            var responseString = serializer.Serialize(requestData);

            var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var responseData = JsonSerializer
                                        .Deserialize<TesteDataObject>(responseString, jsonOptions);

            // Assert
            Assert.NotNull(responseString);

            foreach (PropertyInfo property in requestData.GetType().GetProperties())
            {
                Assert.Equal(property.GetValue(requestData), property.GetValue(responseData));
            }
        }

        [Fact]
        public void Serialize_Should_Throw_An_Exception_For_Null_Parameter()
        {
            // Arrange
            var serializer = new SystemTextJsonSerializer();
            TesteDataObject request = null;

            // Act
            Action act = () =>  serializer.Serialize(request);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void Deserialize_Should_Return_Object_For_Valid_Json_String()
        {
            // Arrange
            var autoFixture = new Fixture();
            var dataTeste = autoFixture.Create<TesteDataObject>();
            var jsonString = new JsonObject()
            {
                ["Id"] = dataTeste.Id,
                ["name"] = dataTeste.Name,
                ["COUNT"] = dataTeste.Count,
                ["vaLue"] = dataTeste.Value,
                ["valid"] = dataTeste.Valid
            }.ToJsonString();
            var serializer = new SystemTextJsonSerializer();

            // Act
            var responseData = serializer.Deserialize<TesteDataObject>(jsonString);

            // Assert
            Assert.NotNull(responseData);

            foreach (PropertyInfo property in dataTeste.GetType().GetProperties())
            {
                Assert.Equal(property.GetValue(dataTeste), property.GetValue(responseData));
            }
        }

        [Fact]
        public void Deserialize_Should_Return_Object_Default_Values_For_Invalid_Json_String()
        {
            // Arrange
            var autoFixture = new Fixture();
            var anotherDataType = autoFixture.Create<AnotherDataObject>();
            var jsonString = new JsonObject()
            {
                ["Code"] = anotherDataType.Code,
                ["Timestamp"] = anotherDataType.Timestamp,
                ["Parameter"] = anotherDataType.Parameter
            }.ToJsonString();
            var serializer = new SystemTextJsonSerializer();

            // Act
            var responseData = serializer.Deserialize<TesteDataObject>(jsonString);

            // Assert
            Assert.Equal(Guid.Empty, responseData.Id);
            Assert.False(responseData.Valid);
            Assert.Equal(0, responseData.Count);
            Assert.Equal(0, responseData.Value);
            Assert.Null(responseData.Name);
        }

        [Fact]
        public void Deserialize_Should_Throw_An_Exception_For_Null_Parameter()
        {
            // Arrange
            var serializer = new SystemTextJsonSerializer();
            
            // Act
            Action act = () => serializer.Deserialize<TesteDataObject>(null);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        [Fact]
        public void Deserialize_Should_Throw_An_Exception_For_Empty_Parameter()
        {
            // Arrange
            var serializer = new SystemTextJsonSerializer();

            // Act
            Action act = () => serializer.Deserialize<TesteDataObject>(string.Empty);

            // Assert
            Assert.Throws<ArgumentNullException>(act);
        }

        partial class TesteDataObject
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public int Count { get; set; }
            public decimal Value { get; set; }
            public bool Valid { get; set; }
        }

        partial class AnotherDataObject
        {
            public int Code { get; set; }
            public string Parameter { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }
}
