using Newtonsoft.Json;
using System.Collections.Generic;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Тело ответа для GetProductData </summary>
    public class GetProductDataResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("data")]
        public List<ProductDataResponse> Data { get; set; }

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