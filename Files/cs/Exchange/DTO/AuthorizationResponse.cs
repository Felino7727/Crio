using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Тело ответа с токеном доступа </summary>
    public class AuthorizationResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }
}