using System;
using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
    /// <summary> Массив Data в ответе для GetOrderDetailDataResponse </summary>
    public class OrderDetailDataResponse
    {
        /// <summary> Номер заказа </summary>
        [JsonProperty("SrcDocID")]
        public string SrcDocId { get; set; }

        /// <summary> Номер позиции </summary>
        [JsonProperty("SrcPosID")]
        public int? SrcPosID { get; set; }

        /// <summary> Название продукта </summary>
        [JsonProperty("ProdName")]
        public string ProdName { get; set; }

        /// <summary> SKU </summary>
        [JsonProperty("ProdID")]
        public string ProdID { get; set; }

        /// <summary> Количество </summary>
        [JsonProperty("Qty")]
        public decimal? Qty { get; set; }

        /// <summary> Количество собранного товара </summary>
        [JsonProperty("QtyCollected")]
        public decimal? QtyCollected { get; set; }

        /// <summary> Состояние заказа </summary>
        [JsonProperty("StateOM")]
        public int? StateOM { get; set; }

        /// <summary> Дата поставки товара </summary>
        [JsonProperty("DateTimeDelivery")]
        public string DateTimeDelivery { get; set; }
    }
}
