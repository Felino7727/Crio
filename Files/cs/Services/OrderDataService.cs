using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using Terrasoft.Core;
using Terrasoft.Web.Common;
using Terrasoft.Web.Http.Abstractions;
using ExternalSystemsIntegration.Files.cs.Exchange.Data;
using ExternalSystemsIntegration.Files.cs.Exchange.DTO;

namespace ExternalSystemsIntegration.Files.cs.Services
{
	/// <summary> WebService [OrderDataService] </summary>
	[ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class OrderDataService : BaseService
    {
		/// <summary> Ответ веб-сервиса </summary>
		private OrderDataServiceResponse response;

		/// <summary> Пользовательское подключение, используемое при выполнении запроса </summary>
		private readonly UserConnection userConnection = HttpContext.Current.Session["UserConnection"] as UserConnection;

		/// <summary> Endpoint: {{siteAddress}}/0/rest/OrderDataService/ImportOrderData </summary>
		[OperationContract]
        [WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
        public OrderDataServiceResponse ImportOrderData(ImportOrderStatusRequest request)
        {
			response = new OrderDataServiceResponse { Success = true };
			DateTime? dateLastExport;
			int usrResponseTimeout =  int.Parse(Terrasoft.Core.Configuration.SysSettings.GetValue(userConnection, "UsrResponseTimeout").ToString());

            try
            {
				bool isDate = DateTime.TryParse(request.DateLastExport, out DateTime date);
				dateLastExport = isDate == true ? date : (DateTime?)null;

				if (dateLastExport == null)
                {
					Logger.WriteToOrderLog("OrderDataService.ImportOrderData.DateLastExport", $"Некорректные данные: {request.DateLastExport}", userConnection);
					response.Success = false;
					response.Error = $"Некорректные данные: {request.DateLastExport}";
					return response;
				}
			}
            catch (Exception ex)
            {
				Logger.WriteToOrderLog("OrderDataService.ImportOrderData.DateLastExport", ex.Message, userConnection);
				response.Success = false;
				response.Error = ex.Message;
				return response;
			}

            try
            {
				OrderData orderData = new OrderData();
				string prepareDataToGetOrderStatus = orderData.PrepareDataToGetOrderData(dateLastExport, userConnection, usrResponseTimeout);

				if (prepareDataToGetOrderStatus != string.Empty)
				{
					response.Success = false;
					response.Error = prepareDataToGetOrderStatus;
					return response;
				}
			}
            catch (Exception ex)
            {
				Logger.WriteToOrderLog("OrderDataService.ImportOrderData.PrepareDataToGetOrderData", ex.Message, userConnection);
				response.Success = false;
				response.Error = ex.Message;
				return response;
			}
			
			return response;
		}
	}
}