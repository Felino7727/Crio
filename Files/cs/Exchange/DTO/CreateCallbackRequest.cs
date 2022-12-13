using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
	/// <summary> Тело запроса к endpoint: AnonymousDataService.svc/CreateCallback </summary>
	[DataContract]
	public class CreateCallbackRequest
	{
		/// <summary> Заявленный телефон* </summary>
		[DataMember]
		[JsonProperty("Phone")]
		public string Phone { get; set; }

		/// <summary> Источник обращения* </summary>
		[DataMember]
		[JsonProperty("Origin")]
		public string Origin { get; set; }
	}
}