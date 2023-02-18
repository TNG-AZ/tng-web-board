using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;

namespace TNG.Web.Board.Services
{
    public class GoogleServices
    {
        public readonly CalendarService Calendar;

        public GoogleServices(IConfiguration Configuration)
        {
            string[] Scopes = { CalendarService.Scope.Calendar };

            ServiceAccountCredential credential;

            using var stream = new FileStream(Configuration["Google_API_PRIVATE_KEYFILE"]!, FileMode.Open, FileAccess.Read);
            var confg = Google.Apis.Json.NewtonsoftJsonSerializer.Instance.Deserialize<JsonCredentialParameters>(stream);
            credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(confg.ClientEmail)
                {
                    Scopes = Scopes
                }.FromPrivateKey(confg.PrivateKey));

            Calendar = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TNG WWW Calendar Integration",
            });
        }
    }
}
