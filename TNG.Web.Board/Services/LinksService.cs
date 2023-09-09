using Square.Models;
using TNG.Web.Board.Data.RequestModels;

namespace TNG.Web.Board.Services
{
    public class LinksService
    {
        private readonly HttpClient http;

        public LinksService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            http = httpClientFactory.CreateClient();
            http.BaseAddress = new Uri(configuration["LinkSiteBaseUrl"]!);
            http.DefaultRequestHeaders.Add("apikey", configuration["LinkSiteApiKey"]!);
        }

        public async Task<string> CreateUrl(string route, CreateLinkRequest request)
        {
            var response = await http.PostAsJsonAsync(route, request);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }
            return string.Empty;
        }

        public async Task<bool> RemoveUrl(string route)
        {
            var response = await http.DeleteAsync(route);
            return response.IsSuccessStatusCode;
        }
    }
}
