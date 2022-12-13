using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace ExternalSystemsIntegration.Files.cs.Exchange.DTO
{
	/// <summary> Тело запроса к endpoint: OrderDataService/ImportOrderData </summary>
	[DataContract]
	public class ImportOrderStatusRequest
	{
		/// <summary> Дата и время последнего экспорта </summary>
		[DataMember]
		[JsonProperty("DateLastExport")]
		public string DateLastExport { get; set; }
	}
}