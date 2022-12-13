using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Terrasoft.Core;
using Terrasoft.Core.Entities;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Data
{
    class CaseData
    {
		/// <summary> Создание обращения </summary>
		/// <param name="contactId"> Id контакта </param>
		/// <param name="originId"> Id источника </param>
		/// <param name="phone"> Заявленный телефон </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateCaseEntity(Guid contactId, Guid originId, string phone, UserConnection userConnection)
        {
			try
			{
				Entity caseEntity = userConnection.EntitySchemaManager.GetInstanceByName("Case").CreateEntity(userConnection);
				caseEntity.SetDefColumnValues();

				caseEntity.SetColumnValue("ContactId", contactId);
				caseEntity.SetColumnValue("OriginId", originId != Guid.Empty ? originId : new Guid("C4A2A015-A24D-4995-81A7-C0B9AC6ECF05"));
				caseEntity.SetColumnValue("Subject", "Заказ обратного звонка");
				caseEntity.SetColumnValue("RegisteredOn", DateTime.Now);
				caseEntity.SetColumnValue("UsrAppliedPhone", phone);
				caseEntity.Save(false);
				Guid caseId = caseEntity.GetTypedColumnValue<Guid>("Id");
				return caseId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("CaseData.CreateCaseEntity.Exception", ex.Message, userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Создание обращения </summary>
		/// <param name="contactId"> Id контакта </param>
		/// <param name="originId"> Id источника </param>
		/// <param name="name"> Заявленное имя </param>
		/// <param name="phone"> Заявленный телефон </param>
		/// <param name="email"> Заявленный мейл </param>
		/// <param name="card"> Заявленный номер карты </param>
		/// <param name="topic"> Тема </param>
		/// <param name="message"> Описание </param>
		/// <param name="files"> Ссылки на файлы </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateCaseEntity(Guid contactId, Guid originId, Guid cityId, string name, string phone, string email, string card, string topic, string message, List<string> files, UserConnection userConnection)
		{
			try
			{
				Entity caseEntity = userConnection.EntitySchemaManager.GetInstanceByName("Case").CreateEntity(userConnection);
				caseEntity.SetDefColumnValues();

				caseEntity.SetColumnValue("ContactId", contactId);
				caseEntity.SetColumnValue("OriginId", originId != Guid.Empty ? originId : (Guid?)null);
				caseEntity.SetColumnValue("UsrCityId", cityId != Guid.Empty ? cityId : (Guid?)null);

				caseEntity.SetColumnValue("Subject", topic);
				caseEntity.SetColumnValue("Symptoms", message);
				caseEntity.SetColumnValue("UsrDescription", GetPlainText(message));
				caseEntity.SetColumnValue("RegisteredOn", DateTime.Now);

				caseEntity.SetColumnValue("UsrAppliedName", name);
				caseEntity.SetColumnValue("UsrAppliedPhone", phone);
                if (!string.IsNullOrWhiteSpace(email)) { caseEntity.SetColumnValue("UsrAppliedEmail", email); }
				if (!string.IsNullOrWhiteSpace(card)) { caseEntity.SetColumnValue("UsrAppliedCardNumber", card); }
				
				caseEntity.Save(false);
				Guid caseId = caseEntity.GetTypedColumnValue<Guid>("Id");

                if (files != null)
                {
					foreach (var file in files)
					{
						CreateFile(new Guid("539BC2F8-0EE0-DF11-971B-001D60E938C6"), caseId, file, userConnection);
					}
				}
				
				return caseId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("CaseData.CreateCaseEntity.Exception", ex.Message, userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Создание ссылки на файл </summary>
		/// <param name="typeId"> Id типа записи </param>
		/// <param name="caseId"> Id обращения </param>
		/// <param name="file"> Ссылка на файл </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateFile(Guid typeId, Guid caseId, string file, UserConnection userConnection)
		{
			try
			{
				if (String.IsNullOrEmpty(file)) { return Guid.Empty; }

				Entity caseFileEntity = userConnection.EntitySchemaManager.GetInstanceByName("CaseFile").CreateEntity(userConnection);
				caseFileEntity.SetDefColumnValues();
				caseFileEntity.SetColumnValue("Name", file);
				caseFileEntity.SetColumnValue("TypeId", typeId);
				caseFileEntity.SetColumnValue("CaseId", caseId);
				caseFileEntity.Save();
				Guid caseFileId = caseFileEntity.GetTypedColumnValue<Guid>("Id");
				return caseFileId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("CaseData.CreateFile.Exception", ex.Message, userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Получить простой текст </summary>
		/// <param name="value"> Значение </param>
		public string GetPlainText(string value)
		{
			var stylesScripts = new Regex("(\\<script(.+?)\\</script\\>)|(\\<style(.+?)\\</style\\>)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
			value = stylesScripts.Replace(value, "");
			value = Regex.Replace(value, "<(.|\n)*?>", String.Empty);
			value = value.Replace(" ", "");
			value = value.Replace("\t", "");
			value = value.Replace("\n", "");
			value = value.Replace("\r\n", "");
			value = value.Replace("\r", "");
			while (value.IndexOf("  ") > 0)
			{
				value = value.Replace("  ", "");
			}
			return value;
		}
	}
}