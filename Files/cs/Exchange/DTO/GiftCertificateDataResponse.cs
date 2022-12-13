using System;
using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Массив Data в ответе для GiftCertificateDataResponse </summary>
    public class GiftCertificateDataResponse
    {
        /// <summary> Номер подарочного сертификата </summary>
        [JsonProperty("dCardId")]
        public string DCardId { get; set; }

        /// <summary> Код состояния подарочного сертификата </summary>
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary> Номинал подарочного сертификата </summary>
        [JsonProperty("value")]
        public decimal? Value { get; set; }

        /// <summary> Срок годности подарочного сертификата </summary>
        [JsonProperty("dateEnd")]
        public DateTime? DateEnd { get; set; }

        /// <summary> Дата и время отоваривания подарочного сертификата </summary>
        [JsonProperty("dateRepayment")]
        public DateTime? DateRepayment { get; set; }

        /// <summary> Код фирмы </summary>
        [JsonProperty("ourId")]
        public string OurId { get; set; }

        /// <summary> Номер заказа </summary>
        [JsonProperty("srcDocId")]
        public string SrcDocId { get; set; }
    }
}