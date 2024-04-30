using Ixnas.AltchaNet;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Protocol;
using System.Security.Cryptography;
using System.Text.Json;

namespace TNG.Web.Board.Services
{
    public class AltchaPageService
    {
        private readonly AltchaService service;
        private readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        public AltchaPageService() {

            var key = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            service = Altcha.CreateServiceBuilder()
                              .UseSha256(key)
                              .UseInMemoryStore()
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
