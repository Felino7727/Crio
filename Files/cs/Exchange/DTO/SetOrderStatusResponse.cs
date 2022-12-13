using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Тело ответа для SetOrderStatus </summary>
    public class SetOrderStatusResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }
    }
}