using System;
using Terrasoft.Core;

namespace ExternalSystemsIntegrationApi
{
    /// <summary> Внешний вызов для обработки данных по сущности [Заказ] </summary>
    public interface IOrderDataExternal
    {
        /// <summary> Импорт данных по заказу </summary>
        void ImportOrderData(DateTime? dateLastExport, UserConnection userConnection, int UsrResponseTimeout);

        /// <summary> Импорт данных по подарочному сертификату </summary>
        void ImportGiftCertificateData(Guid orderId, UserConnection userConnection);

        /// <summary> Импорт данных по продукту </summary>
        void ImportProductData(Guid productId, UserConnection userConnection);

        /// <summary> Повторная установка состояния заказа </summary>
		void ReSetOrderStatus(string number, string usrCode, UserConnection userConnection);
    }
}
