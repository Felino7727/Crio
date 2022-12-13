using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
	/// <summary> Тело ответа веб-сервиса [OrderDataService]</summary>
	[DataContract]
	public class OrderDataServiceResponse
	{
		/// <summary> Индикатор успеха </summary>
		[DataMember(Order = 0)]
		[JsonProperty("Success")]
		public bool Success { get; set; }

		/// <summary> Сообщение о ошибке </summary>
		[DataMember(Order = 1)]
		[JsonProperty("Error")]
		public string Error { get; set; }
	}
}