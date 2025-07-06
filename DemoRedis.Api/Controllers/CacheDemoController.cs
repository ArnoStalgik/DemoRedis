using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace DemoRedis.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CacheDemoController(IDistributedCache cache) : ControllerBase
    {
        [HttpGet("{key}")]
        public async Task<IActionResult> Get(string key)
        {
            var value = await cache.GetStringAsync(key);
            if (value == null)
                return NotFound("Valeur absente du cache !");

            return Ok(new { key, value });
        }

        [HttpPost("{key}")]
        public async Task<IActionResult> Set(string key, [FromBody] string value)
        {
            await cache.SetStringAsync(key, value, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return Ok(new { key, value, cached = true });
        }

        [HttpDelete("{key}")]
        public async Task<IActionResult> Delete(string key)
        {
            await cache.RemoveAsync(key);

            return Ok(new { key, deleted = true });
        }
    }
}
