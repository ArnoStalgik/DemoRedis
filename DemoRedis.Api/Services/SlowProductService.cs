using DemoRedis.Models;

namespace DemoRedis.Services
{
    public class SlowProductService : ISlowProductService
    {
        public async Task<Produit> GetProductAsync(string id)
        {
            await Task.Delay(2000); // Simule une DB lente
            return new Produit
            {
                Id = id,
                Nom = $"Produit {id}",
                Maj = DateTime.Now
            };
        }
    }

    public interface ISlowProductService
    {
        Task<Produit> GetProductAsync(string id);
    }
}
