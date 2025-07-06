using DemoRedis.Models;
using DemoRedis.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace DemoRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProductController(IDistributedCache cache, ISlowProductService slowService, ILogger<ProductController> logger) : ControllerBase
    {
        [HttpGet("json/{id}")]
        public async Task<IActionResult> GetJson(string id)
        {
            var (source, produit, duration) = await GetProductWithCacheAsync(id);
            return Ok(new { source, id, produit, durationMs = duration });
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            var cacheKey = $"product:{id}";

            try
            {
                var value = await cache.GetStringAsync(cacheKey);
                if (value != null)
                {
                    logger.LogInformation("CACHE OK pour {CacheKey}", cacheKey);
                    return Ok(new { source = "cache", id, value = string.Empty });
                }
                else
                {
                    logger.LogWarning("CACHE KO pour {CacheKey}", cacheKey);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Redis pas dispo, fallback sur DB");
            }

            // Fallback “base de données” lente
            _ = await slowService.GetProductAsync(id);

            // On tente de remettre en cache
            try
            {
                await cache.SetStringAsync(cacheKey, string.Empty, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Impossible de setter la valeur dans Redis (Redis down ?)");
            }

            return Ok(new { source = "db", id, value = string.Empty });
        }

        [HttpGet("benchmark/{id}")]
        public async Task<IActionResult> Benchmark(string id, [FromQuery] int count = 5)
        {
            // Purge le cache avant de commencer
            await cache.RemoveAsync($"product:json:{id}");
            var times = new List<long>();
            var sources = new List<string>();

            for (int i = 0; i < count; i++)
            {
                var (source, _, duration) = await GetProductWithCacheAsync(id);
                times.Add(duration);
                sources.Add(source);
            }

            return Ok(new
            {
                message = $"Benchmark de {count} appels pour le produit {id}",
                temps_milliseconds = times,
                sources,
                explication = "Le 1er appel est lent (cache KO), les suivants sont instantanés (cache OK)."
            });
        }

        private async Task<(string Source, Produit Produit, long DurationMs)> GetProductWithCacheAsync(string id)
        {
            var cacheKey = $"product:json:{id}";
            var stopwatch = new System.Diagnostics.Stopwatch();
            Produit produit;
            string source;
            stopwatch.Start();

            try
            {
                var cached = await cache.GetStringAsync(cacheKey);
                if (cached != null)
                {
                    produit = JsonSerializer.Deserialize<Produit>(cached);
                    logger.LogInformation("CACHE HIT (JSON) pour {CacheKey}", cacheKey);
                    source = "cache";
                    stopwatch.Stop();
                    return (source, produit, stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    logger.LogWarning("CACHE MISS (JSON) pour {CacheKey}", cacheKey);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Redis not available, fallback on DB");
            }

            // Accès "DB"
            produit = await slowService.GetProductAsync(id);
            source = "db";

            // Mise en cache
            try
            {
                var serialized = JsonSerializer.Serialize(produit);
                await cache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Cannot set value in Redis (Redis down?)");
            }
            stopwatch.Stop();
            return (source, produit, stopwatch.ElapsedMilliseconds);
        }

    }
}
