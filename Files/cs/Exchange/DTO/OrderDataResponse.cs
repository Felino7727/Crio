using System;
using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Массив Data в ответе для GetOrderDataResponse </summary>
    public class OrderDataResponse
    {
        /// <summary> Номер заказа </summary>
        [JsonProperty("SrcDocID")]
        public string SrcDocId { get; set; }

        /// <summary> Номер заказа в OTUtils </summary>
        [JsonProperty("DocID")]
        public int? DocId { get; set; }

        /// <summary> Код состояния заказа </summary>
        [JsonProperty("StateOM")]
        public string StateOm { get; set; }

        /// <summary> Номер ТТН </summary>
        [JsonProperty("TTNNumber")]
        public string TtnNumber { get; set; }

        /// <summary> Код фирмы </summary>
        [JsonProperty("OurID")]
        public string OurId { get; set; }

        /// <summary> Номер активной карты </summary>
        [JsonProperty("LoyalCard")]
        public string LoyalCard { get; set; }

        /// <summary> Количество мест </summary>
        [JsonProperty("CaseCount")]
        public int? CaseCount { get; set; }

        /// <summary> Склад </summary>
        [JsonProperty("UsrStock")]
        public string UsrStock { get; set; }

        /// <summary> Код ошибки </summary>
        [JsonProperty("ErrorCode")]
        public string ErrorCode { get; set; }

        /// <summary> Дней до отгрузки </summary>
        [JsonProperty("Days")]
        public int? Days { get; set; }

        /// <summary> Дата выгрузки </summary>
        [JsonProperty("ImportDate")]
        public DateTime? ImportDate { get; set; }

        /// <summary> Город </summary>
        [JsonProperty("ShippingCity")]
        public string ShippingCity { get; set; }

        /// <summary> Отделение доставки </summary>
        [JsonProperty("ShippingAddress")]
        public string ShippingAddress { get; set; }

        /// <summary> Адрес доставки / Улица </summary>
        [JsonProperty("ShippingStreet")]
        public string ShippingStreet { get; set; }
    }
}