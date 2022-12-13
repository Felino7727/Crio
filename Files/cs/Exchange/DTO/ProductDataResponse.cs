using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Массив Data в ответе для GetProductDataResponse </summary>
    public class ProductDataResponse
    {
        /// <summary> Код фирмы </summary>
        [JsonProperty("OurID")]
        public string OurId { get; set; }

        /// <summary> Id торговой марки </summary>
        [JsonProperty("BrandID")]
        public string BrandId { get; set; }

        /// <summary> Название торговой марки </summary>
        [JsonProperty("BrandName")]
        public string BrandName { get; set; }

        /// <summary> Остаток на магазине </summary>
        [JsonProperty("Qty")]
        public string Qty { get; set; }

        /// <summary> Цена </summary>
        [JsonProperty("PriceMC")]
        public decimal? PriceMc { get; set; }

        /// <summary> Id населенного пункта </summary>
        [JsonProperty("cityid")]
        public string CityId { get; set; }

        /// <summary> Название населенного пункта </summary>
        [JsonProperty("CityName")]
        public string CityName { get; set; }

        /// <summary> Id области </summary>
        [JsonProperty("RegionId")]
        public string RegionId { get; set; }
    }
}