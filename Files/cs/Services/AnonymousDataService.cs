using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.ServiceModel.Activation;
using Terrasoft.Core;
using Terrasoft.Web.Http.Abstractions;
using ExternalSystemsIntegration.Files.cs.Exchange.Data;
using ExternalSystemsIntegration.Files.cs.Exchange.DTO;

namespace ExternalSystemsIntegration.Files.cs.Services
{
    /// <summary> WebService [AnonymousDataService] </summary>
	[ServiceContract]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
	public class AnonymousDataService
    {
		#region Fields
		/// <summary> Ответ веб-сервиса </summary>
		private AnonymousDataServiceResponse response;

		/// <summary> Подключение на уровне приложения </summary>
		private readonly AppConnection appConnection;

		/// <summary> Системное или пользовательское подключение, используемое при выполнении запроса </summary>
		private readonly UserConnection userConnection;
		#endregion

		#region Constructors
		/// <summary> Конструктор с получением прав доступа системного подключения </summary>
		public AnonymousDataService()
		{
			appConnection = HttpContext.Current.Application["AppConnection"] as AppConnection;
			userConnection = appConnection.SystemUserConnection;
		}

		/// <summary> Конструктор с получением прав доступа пользовательского подключения </summary>
		public AnonymousDataService(UserConnection userConnection) 
		{ 
			this.userConnection = userConnection; 
		}
		#endregion

		#region Endpoints
		/// <summary> Endpoint: {{siteAddress}}/0/ServiceModel/AnonymousDataService/CreateCallback </summary>
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public AnonymousDataServiceResponse CreateCallback(CreateCallbackRequest request)
		{
			response = new AnonymousDataServiceResponse { Success = true };

			if (!string.IsNullOrEmpty(request.Phone) && !string.IsNullOrEmpty(request.Origin))
            {
				Guid serviceId = new Guid("03CAB828-CE2C-464B-A787-02B31529E6F0");
				Guid contactId = new ContactData().FindContactEntity(request.Phone, serviceId, userConnection);
				if (contactId != Guid.Empty)
				{
					try
					{
						Guid originId = DBData.SelectGuidByString("Id", "CaseOrigin", "UsrCode", request.Origin, userConnection);
						Guid caseId = new CaseData().CreateCaseEntity(contactId, originId, request.Phone, userConnection);

						if (caseId != Guid.Empty) { response.Id = caseId; }
						else { response.Success = false; }
					}
					catch (Exception ex)
					{
						Logger.WriteToLog("AnonymousDataService.CreateCallback.Exception", ex.Message, userConnection);
						response.Success = false;
						response.Error = ex.Message;
						response.Id = null;
						return response;
					}
				}
				else
				{
					Logger.WriteToLog("AnonymousDataService.CreateCallback.Else", $"Origin: {request.Origin}, MobilePhone: {request.Phone}", "Контакт не найден и не создан", userConnection);
					response.Success = false;
					response.Error = "Контакт не найден и не создан. Проверьте формат номера 380XXXXXXXXX";
					response.Id = null;
					return response;
				}

				return response;
			}
            else
            {
				response.Success = false;
				response.Error = null;
				response.Id = null;
				return response;
			}
		}

		/// <summary> Endpoint: {{siteAddress}}/0/ServiceModel/AnonymousDataService/CreateFeedback </summary>
		[OperationContract]
		[WebInvoke(Method = "POST", RequestFormat = WebMessageFormat.Json, BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public AnonymousDataServiceResponse CreateFeedback(CreateFeedbackRequest request)
        {
			response = new AnonymousDataServiceResponse { Success = true };

			if (!string.IsNullOrEmpty(request.Phone) && !string.IsNullOrEmpty(request.Origin))
            {
				Guid serviceId = new Guid("299F67F6-1123-4B56-B029-1DAD12408DF8");

				Guid contactId = string.IsNullOrWhiteSpace(request.Email)
					? new ContactData().FindContactEntity(request.Name, request.Phone, serviceId, userConnection)
					: new ContactData().FindContactEntity(request.Name, request.Phone, request.Email, serviceId, userConnection);

				if (contactId != Guid.Empty)
				{
					try
					{
						Guid originId = DBData.SelectGuidByString("Id", "CaseOrigin", "UsrCode", request.Origin, userConnection);
						Guid cityId = DBData.SelectGuidByString("Id", "City", "Name", request.City, userConnection);
						Guid caseId = new CaseData().CreateCaseEntity(contactId, originId, cityId, request.Name, request.Phone, request.Email, request.Card, request.Topic, request.Message, request.File, userConnection);

						if (caseId != Guid.Empty) { response.Id = caseId; }
						else { response.Success = false; }
					}
					catch (Exception ex)
					{
						Logger.WriteToLog("AnonymousDataService.CreateFeedback.Exception", ex.Message, userConnection);
						response.Success = false;
						response.Error = ex.Message;
						response.Id = null;
						return response;
					}
				}
				else
				{
					Logger.WriteToLog("AnonymousDataService.CreateFeedback.Else", $"Origin: {request.Origin}, MobilePhone: {request.Phone}, Email: {request.Email}", "Контакт не найден и не создан", userConnection);
					response.Success = false;
					response.Error = "Контакт не найден и не создан. Проверьте формат номера 380XXXXXXXXX или формат почтового ящика example@domain.com";
					response.Id = null;
					return response;
				}

				return response;
			}
			else
			{
				response.Success = false;
				response.Error = null;
				response.Id = null;
				return response;
			}
		}
		#endregion
	}
}