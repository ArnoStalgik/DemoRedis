using DemoRedis.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text;
using System.Text.Json;

namespace DemoRedis.Tests.Controllers
{
    public class CacheControllerTests
    {
        private static byte[] StringToBytes(string value) => value == null ? null : Encoding.UTF8.GetBytes(value);

        [Fact]
        public async Task Get_ReturnsValue_WhenKeyExists()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var key = "clé_test";
            var value = "valeur_test";
            cacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync(StringToBytes(value));
            var controller = new CacheDemoController(cacheMock.Object);

            // Act
            var result = await controller.Get(key);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            Assert.Equal(key, dict["key"].GetString());
            Assert.Equal(value, dict["value"].GetString());
        }

        [Fact]
        public async Task Get_ReturnsNotFound_WhenKeyMissing()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var key = "cle_absente";
            cacheMock
                .Setup(c => c.GetAsync(key, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null);
            var controller = new CacheDemoController(cacheMock.Object);

            // Act
            var result = await controller.Get(key);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Valeur absente du cache !", notFound.Value);
        }

        [Fact]
        public async Task Set_CachesValue()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var key = "maCle";
            var value = "maValeur";

            cacheMock.Setup(c => c.SetAsync(
                key,
                StringToBytes(value),
                It.IsAny<DistributedCacheEntryOptions>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var controller = new CacheDemoController(cacheMock.Object);

            // Act
            var result = await controller.Set(key, value);

            // Assert
            cacheMock.Verify();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            Assert.Equal(key, dict["key"].GetString());
            Assert.Equal(value, dict["value"].GetString());
            Assert.True(dict["cached"].GetBoolean());
        }

        [Fact]
        public async Task Delete_RemovesKey()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var key = "a_supprimer";
            cacheMock.Setup(c => c.RemoveAsync(key, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask).Verifiable();

            var controller = new CacheDemoController(cacheMock.Object);

            // Act
            var result = await controller.Delete(key);

            // Assert
            cacheMock.Verify();
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            Assert.Equal(key, dict["key"].GetString());
            Assert.True(dict["deleted"].GetBoolean());
        }
    }
}