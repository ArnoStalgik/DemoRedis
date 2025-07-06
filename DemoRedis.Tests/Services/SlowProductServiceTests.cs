using DemoRedis.Services;

namespace DemoRedis.Tests.Services
{
    public class SlowProductServiceTests
    {
        [Fact]
        public async Task GetProductAsync_ReturnsProduit_WithExpectedId()
        {
            var service = new SlowProductService();
            var id = "77";
            var result = await service.GetProductAsync(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.StartsWith("Produit", result.Nom);
            Assert.True(result.Maj > DateTime.Now.AddMinutes(-1)); // La date est "fraîche"
        }
    }
}