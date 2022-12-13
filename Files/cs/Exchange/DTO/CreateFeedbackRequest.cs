using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
	/// <summary> Тело запроса к endpoint: AnonymousDataService.svc/CreateFeedback </summary>
	[DataContract]
	public class CreateFeedbackRequest
	{
		/// <summary> Источник обращения* </summary>
		[DataMember]
		[JsonProperty("Origin")]
		public string Origin { get; set; }

		/// <summary> Тема обращения* </summary>
		[DataMember]
		[JsonProperty("Topic")]
		public string Topic { get; set; }

		/// <summary> Текст обращения* </summary>
		[DataMember]
		[JsonProperty("Message")]
		public string Message { get; set; }

		/// <summary> Заявленное имя* </summary>
		[DataMember]
		[JsonProperty("Name")]
		public string Name { get; set; }

		/// <summary> Заявленный телефон* </summary>
		[DataMember]
		[JsonProperty("Phone")]
		public string Phone { get; set; }

		/// <summary> Заявленный мейл* </summary>
		[DataMember]
		[JsonProperty("Email")]
		public string Email { get; set; }

		/// <summary> Заявленный номер карты </summary>
		[DataMember]
		[JsonProperty("Card")]
		public string Card { get; set; }

		/// <summary> Название населенного пункта </summary>
		[DataMember]
		[JsonProperty("City")]
		public string City { get; set; }

		/// <summary> Прикрепленные файлы </summary>
		[DataMember]
		[JsonProperty("File")]
		public List<string> File { get; set; } = new List<string>();
	}
}