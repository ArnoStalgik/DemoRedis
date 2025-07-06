using DemoRedis.Controllers;
using DemoRedis.Models;
using DemoRedis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using System.Text;

namespace DemoRedis.Tests.Controllers
{
    public class ProductControllerTests
    {
        private static byte[] StringToBytes(string value) => value == null ? null : Encoding.UTF8.GetBytes(value);

        [Fact]
        public async Task Get_ReturnsCache_WhenCacheExists()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<ProductController>>();
            var slowServiceMock = new Mock<SlowProductService>();
            var testId = "123";
            var cacheKey = $"product:{testId}";

            cacheMock
                .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync(StringToBytes("dummy")); // simulate cache hit

            var controller = new ProductController(cacheMock.Object, slowServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.Get(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            Assert.Equal("cache", dict["source"].GetString());
            Assert.Equal(testId, dict["id"].GetString());
        }

        [Fact]
        public async Task Get_ReturnsDb_WhenCacheMiss()
        {
            // Arrange
            var cacheMock = new Mock<IDistributedCache>();
            var loggerMock = new Mock<ILogger<ProductController>>();
            var slowServiceMock = new Mock<ISlowProductService>();
            var testId = "123";
            var cacheKey = $"product:{testId}";

            cacheMock
                .Setup(c => c.GetAsync(cacheKey, It.IsAny<CancellationToken>()))
                .ReturnsAsync((byte[]?)null); // simulate cache miss


            slowServiceMock
                .Setup(s => s.GetProductAsync(testId))
                .ReturnsAsync(new Produit { Id = testId, Nom = "mock", Maj = DateTime.Now });

            cacheMock
                .Setup(c => c.SetAsync(
                    cacheKey,
                    It.IsAny<byte[]>(),
                    It.IsAny<DistributedCacheEntryOptions>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var controller = new ProductController(cacheMock.Object, slowServiceMock.Object, loggerMock.Object);

            // Act
            var result = await controller.Get(testId);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var json = JsonSerializer.Serialize(okResult.Value);
            var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
            Assert.Equal("db", dict["source"].GetString());
            Assert.Equal(testId, dict["id"].GetString());
            cacheMock.Verify();
        }
    }
}