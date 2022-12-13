using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
	/// <summary> Тело ответа веб-сервиса [AnonymousDataService] </summary>
	[DataContract]
	public class AnonymousDataServiceResponse
	{
		/// <summary> Индикатор успеха </summary>
		[DataMember(Order = 0)]
		[JsonProperty("Success")]
		public bool Success { get; set; }

		/// <summary> Сообщение об ошибке </summary>
		[DataMember(Order = 1)]
		[JsonProperty("Error")]
		public string Error { get; set; }

		/// <summary> Id созданной записи </summary>
		[DataMember(Order = 2)]
		[JsonProperty("Id")]
		public Guid? Id { get; set; }
	}
}