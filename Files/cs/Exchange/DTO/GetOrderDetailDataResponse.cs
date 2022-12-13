using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Тело ответа для GetOrderDetailDataResponse </summary>
    public class GetOrderDetailDataResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public List<OrderDetailDataResponse> Data { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}
