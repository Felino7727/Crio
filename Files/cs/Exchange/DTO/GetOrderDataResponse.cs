using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Тело ответа для GetOrderDataResponse </summary>
    public class GetOrderDataResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public List<OrderDataResponse> Data { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("causeMessage")]
        public string CauseMessage { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}