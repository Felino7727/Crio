using ExternalSystemsIntegration.Files.cs.Exchange.Data;
using ExternalSystemsIntegrationApi;
using System;
using System.Threading.Tasks;
using Terrasoft.Core;
using Terrasoft.Core.Factories;

namespace ExternalSystemsIntegration.Files.cs.External
{
	/// <summary> Внешний вызов для обработки данных по сущности [Заказ] </summary>
	[DefaultBinding(typeof(IOrderDataExternal))]
	public class OrderDataExternal : IOrderDataExternal
	{
		/// <summary> Импорт данных по заказу </summary>
		public void ImportOrderData(DateTime? dateLastExport, UserConnection userConnection, int usrResponseTimeout)
        {
			OrderData orderData = new OrderData();
			orderData.PrepareDataToGetOrderData(dateLastExport, userConnection, usrResponseTimeout);
		}

		/// <summary> Импорт данных по подарочному сертификату </summary>
		public void ImportGiftCertificateData(Guid orderId, UserConnection userConnection)
		{
			OrderData orderData = new OrderData();
			orderData.PrepareDataToGetGiftCertificateData(orderId, userConnection);
		}

		/// <summary> Импорт данных по продукту </summary>
		public void ImportProductData(Guid productId, UserConnection userConnection)
		{
			OrderData orderData = new OrderData();
			orderData.PrepareDataToGetProductData(productId, userConnection);
		}

		/// <summary> Повторная установка состояния заказа </summary>
		public void ReSetOrderStatus(string number, string usrCode, UserConnection userConnection)
		{
			OrderData orderData = new OrderData();
			Task resultOrderData = Task.Run(async () => { await orderData.SetOrderStatus(number, usrCode, userConnection); });
		}
	}
}
