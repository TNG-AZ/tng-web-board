using Ixnas.AltchaNet;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography;
using System.Text.Json;

namespace TNG.Web.Board.Services
{
    internal class AltchaCache : IAltchaChallengeStore
    {
        private readonly IMemoryCache _cache;
        public AltchaCache(IMemoryCache cache) 
        {
            _cache = cache;
        }
        public Task<bool> Exists(string challenge)
        {
            return Task.FromResult(_cache.TryGetValue(challenge, out _));
        }

        public async Task Store(string challenge, DateTimeOffset expiryUtc)
        {
            await _cache.GetOrCreateAsync(challenge,
                cacheEntry =>
                {
                    cacheEntry.SetSlidingExpiration(TimeSpan.FromDays(1));
                    return Task.FromResult(new object());
                });
        }
    }
    public class AltchaPageService
    {
        private readonly AltchaService service;
        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        public AltchaPageService(IAltchaChallengeStore cache) {

            var key = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            service = Altcha.CreateServiceBuilder()
                              .UseSha256(key)
                              .UseStore(cache)
                              .Build();
        }

        public string Generate()
        {
            var challenge = service.Generate();
            return JsonSerializer.Serialize(challenge, jsonOptions);
        }

        public async Task<bool> Validate(string altchaBase64)
            => (await service.Validate(altchaBase64)).IsValid;
    }
}
