using System;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Data
{
    class ContactData
    {
		/// <summary> Поиск контакта </summary>
		/// <param name="phone"> Телефон </param>
		/// <param name="serviceId"> Id сервиса инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid FindContactEntity(string phone, Guid serviceId, UserConnection userConnection)
		{
			string mobilePhone = CleanPhone(phone, userConnection);
			bool isValidPhone = IsValidPhone(mobilePhone, userConnection);
			if (isValidPhone == false) { return Guid.Empty; }

			Guid contactId = DBData.SelectGuidByString("Id", "Contact", "MobilePhone", mobilePhone, userConnection);
			if (contactId == Guid.Empty)
			{
                contactId = FindCommunicationEntity(phone, userConnection);
                if (contactId == Guid.Empty)
                {
					contactId = CreateContactEntity(mobilePhone, serviceId, userConnection);
					return contactId;
				}
                else { return contactId; }
			}
			else
			{
				Logger.WriteToLog("ContactData.FindContactEntity", $"Найден по MobilePhone: {mobilePhone}", userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, userConnection);
				return contactId;
			}
		}

		/// <summary> Поиск контакта </summary>
		/// <param name="phone"> Телефон </param>
		/// <param name="serviceId"> Id сервиса инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid FindContactEntity(string name, string phone, Guid serviceId, UserConnection userConnection)
		{
			string mobilePhone = CleanPhone(phone, userConnection);
			bool isValidPhone = IsValidPhone(mobilePhone, userConnection);
			if (isValidPhone == false) { return Guid.Empty; }

			Guid contactId = DBData.SelectGuidByString("Id", "Contact", "MobilePhone", mobilePhone, userConnection);
			if (contactId == Guid.Empty)
			{
				contactId = FindCommunicationEntity(phone, userConnection);
				if (contactId == Guid.Empty)
				{
					contactId = CreateContactEntity(name, mobilePhone, serviceId, userConnection);
					return contactId;
				}
				else { return contactId; }
			}
			else
			{
				Logger.WriteToLog("ContactData.FindContactEntity", $"Найден по MobilePhone: {mobilePhone}", userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, userConnection);
				return contactId;
			}
		}

		/// <summary> Поиск контакта </summary>
		/// <param name="phone"> Телефон </param>
		/// <param name="email"> Почтовый ящик </param>
		/// <param name="serviceId"> Id сервиса инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid FindContactEntity(string name, string phone, string email, Guid serviceId, UserConnection userConnection)
		{
			string mobilePhone = CleanPhone(phone, userConnection);
			bool isValidPhone = IsValidPhone(mobilePhone, userConnection);
			if (isValidPhone == false) { return Guid.Empty; }

			bool іsValidEmail = IsValidEmail(email, userConnection);
			if (іsValidEmail == false) { return Guid.Empty; }

			Guid contactId = DBData.SelectGuidByString("Id", "Contact", "MobilePhone", mobilePhone, userConnection);
			if (contactId == Guid.Empty)
			{
				contactId = DBData.SelectGuidByString("Id", "Contact", "Email", email, userConnection);
                if (contactId == Guid.Empty)
                {
					contactId = FindCommunicationEntity(phone, email, userConnection);
					if (contactId == Guid.Empty)
					{
						contactId = CreateContactEntity(name, mobilePhone, email, serviceId, userConnection);
						return contactId;
					}
					else { return contactId; }
				}
                else
                {
					Logger.WriteToLog("ContactData.FindContactEntity", $"Найден по Email: {email}", userConnection);
					CreateCommunicationEntity(contactId, mobilePhone, email, userConnection);
					return contactId;
				}
			}
			else
			{
				Logger.WriteToLog("ContactData.FindContactEntity", $"Найден по MobilePhone: {mobilePhone}", userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, email, userConnection);
				return contactId;
			}
		}

		/// <summary> Поиск средств связи </summary>
		/// <param name="mobilePhone"> Телефон </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid FindCommunicationEntity(string mobilePhone, UserConnection userConnection)
        {
			try
			{
				if (!string.IsNullOrEmpty(mobilePhone))
				{
					Entity entityContactCommunication = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
					if (entityContactCommunication.FetchFromDB("Number", mobilePhone))
					{
						Guid contactId = entityContactCommunication.GetTypedColumnValue<Guid>("ContactId");
						Logger.WriteToLog("ContactData.FindCommunicationEntity", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные найдены в [dbo.ContactCommunication]", userConnection);
						return contactId;
					}
                    else { return Guid.Empty; }
				}
				return Guid.Empty;
			}
			catch (Exception ex) 
			{ 
				Logger.WriteToLog("ContactData.FindCommunicationEntity.Exception", $"Мобильный телефон: {mobilePhone}" , ex.Message, userConnection);
				DBData.DeleteAllByString("ContactCommunication", "Number", mobilePhone, userConnection);
				return Guid.Empty;
			}
        }

		/// <summary> Поиск средств связи </summary>
		/// <param name="mobilePhone"> Телефон </param>
		/// <param name="email"> Почтовый ящик </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid FindCommunicationEntity(string mobilePhone, string email, UserConnection userConnection)
		{
			try
			{
				if (!string.IsNullOrEmpty(mobilePhone) && !string.IsNullOrEmpty(email))
				{
					Entity entityContactCommunication = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
					if (entityContactCommunication.FetchFromDB("Number", mobilePhone))
					{
						Guid contactId = entityContactCommunication.GetTypedColumnValue<Guid>("ContactId");
						Logger.WriteToLog("ContactData.FindCommunicationEntity", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные найдены в [dbo.ContactCommunication]", userConnection);
						return contactId;
					}

					if (entityContactCommunication.FetchFromDB("Number", email))
					{
						Guid contactId = entityContactCommunication.GetTypedColumnValue<Guid>("ContactId");
						Logger.WriteToLog("ContactData.FindCommunicationEntity", $"Contact.Id: {contactId}", $"Почтовый ящик: {email}, данные найдены в [dbo.ContactCommunication]", userConnection);
						return contactId;
					}
				}

				return Guid.Empty;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.FindCommunicationEntity.Exception", $"Мобильный телефон: {mobilePhone}, Почтовый ящик: {email}", ex.Message, userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Создание средства связи </summary>
		/// <param name="contactId"> Id контакта </param>
		/// <param name="mobilePhone"> Мобильный телефон </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public void CreateCommunicationEntity(Guid contactId, string mobilePhone, UserConnection userConnection)
		{
			try
			{
				if (!string.IsNullOrEmpty(mobilePhone))
				{
					Dictionary<string, object> conditionsMobilePhone = new Dictionary<string, object>() { { "Contact", contactId }, { "Number", mobilePhone }, };
					Entity entityContactCommunication = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
					if (!entityContactCommunication.FetchFromDB(conditionsMobilePhone))
					{
						entityContactCommunication.SetDefColumnValues();
						entityContactCommunication.SetColumnValue("CommunicationTypeId", new Guid("D4A2DC80-30CA-DF11-9B2A-001D60E938C6"));
						entityContactCommunication.SetColumnValue("ContactId", contactId);
						entityContactCommunication.SetColumnValue("Number", mobilePhone);
						entityContactCommunication.Save(false);
						Logger.WriteToLog("ContactData.CreateCommunicationEntity", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные записаны в [dbo.ContactCommunication]", userConnection);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.CreateCommunicationEntity.Exception", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные не записаны в [dbo.ContactCommunication]. {ex.Message}", userConnection);

				DBData.DeleteAllByGuidAndString("ContactCommunication", "ContactId", contactId, "Number", mobilePhone, userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, userConnection);
			}
		}

		/// <summary> Создание средства связи </summary>
		/// <param name="contactId"> Id контакта </param>
		/// <param name="mobilePhone"> Мобильный телефон </param>
		/// <param name="email"> Почтовый ящик </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public void CreateCommunicationEntity(Guid contactId, string mobilePhone, string email, UserConnection userConnection)
		{
			try
			{
				if (!string.IsNullOrEmpty(mobilePhone))
				{
					Dictionary<string, object> conditionsMobilePhone = new Dictionary<string, object>() { { "Contact", contactId }, { "Number", mobilePhone }, };
					Entity entityContactCommunication = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
					if (!entityContactCommunication.FetchFromDB(conditionsMobilePhone))
					{
						entityContactCommunication.SetDefColumnValues();
						entityContactCommunication.SetColumnValue("CommunicationTypeId", new Guid("D4A2DC80-30CA-DF11-9B2A-001D60E938C6"));
						entityContactCommunication.SetColumnValue("ContactId", contactId);
						entityContactCommunication.SetColumnValue("Number", mobilePhone);
						entityContactCommunication.Save(false);
						Logger.WriteToLog("ContactData.CreateCommunicationEntity", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные записаны в [dbo.ContactCommunication]", userConnection);
					}
				}
			}
			catch (Exception ex) 
			{ 
				Logger.WriteToLog("ContactData.CreateCommunicationEntity.Exception", $"Contact.Id: {contactId}", $"Мобильный телефон: {mobilePhone}, данные не записаны в [dbo.ContactCommunication]. {ex.Message}", userConnection);

				DBData.DeleteAllByGuidAndString("ContactCommunication", "ContactId", contactId, "Number", mobilePhone, userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, email, userConnection);
			}

			try
			{
				if (!string.IsNullOrEmpty(email))
				{
					Dictionary<string, object> conditionsMobilePhone = new Dictionary<string, object>() { { "Contact", contactId }, { "Number", email }, };
					Entity entityContactCommunication = userConnection.EntitySchemaManager.GetInstanceByName("ContactCommunication").CreateEntity(userConnection);
					if (!entityContactCommunication.FetchFromDB(conditionsMobilePhone))
					{
						entityContactCommunication.SetDefColumnValues();
						entityContactCommunication.SetColumnValue("CommunicationTypeId", new Guid("EE1C85C3-CFCB-DF11-9B2A-001D60E938C6"));
						entityContactCommunication.SetColumnValue("ContactId", contactId);
						entityContactCommunication.SetColumnValue("Number", email);
						entityContactCommunication.Save(false);
						Logger.WriteToLog("ContactData.CreateCommunicationEntity", $"Contact.Id: {contactId}", $"Почтовый ящик: {email}, данные записаны в [dbo.ContactCommunication]", userConnection);
					}
				}
			}
			catch (Exception ex) 
			{ 
				Logger.WriteToLog("ContactData.CreateCommunicationEntity.Exception", $"Contact.Id: {contactId}", $"Почтовый ящик: {email}, данные не записаны в [dbo.ContactCommunication]. {ex.Message}", userConnection);

				DBData.DeleteAllByGuidAndString("ContactCommunication", "ContactId", contactId, "Number", email, userConnection);
				CreateCommunicationEntity(contactId, mobilePhone, email, userConnection);
			}
		}

		/// <summary> Создание контакта </summary>
		/// <param name="mobilePhone"> Мобильный телефон </param>
		/// <param name="serviceId"> Сервис инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateContactEntity(string mobilePhone, Guid serviceId, UserConnection userConnection)
		{
			try
			{
				Entity contactEntity = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);
				contactEntity.SetDefColumnValues();
				contactEntity.SetColumnValue("TypeId", new Guid("00783EF6-F36B-1410-A883-16D83CAB0980"));
				contactEntity.SetColumnValue("MobilePhone", mobilePhone);
				contactEntity.SetColumnValue("Surname", "Клиент");
				contactEntity.SetColumnValue("GivenName", mobilePhone);
				contactEntity.SetColumnValue("LanguageId", new Guid("E35E61EF-CC32-4A97-B98C-52473E35CE60"));
				contactEntity.SetColumnValue("UsrIsCreatedFromService", true);
				contactEntity.SetColumnValue("UsrCreatedByServiceId", serviceId);
				contactEntity.Save(false);

				Guid contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				Logger.WriteToLog("ContactData.CreateContactEntity", $"Contact.Id: {contactId}", "Создан новый контакт", userConnection);
				return contactId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.CreateContactEntity.Exception", $"Contact.MobilePhone: {mobilePhone}", $"Новый контакт не создан. {ex.Message}", userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Создание контакта </summary>
		/// <param name="mobilePhone"> Мобильный телефон </param>
		/// <param name="serviceId"> Сервис инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateContactEntity(string name, string mobilePhone, Guid serviceId, UserConnection userConnection)
		{
			try
			{
				Entity contactEntity = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);
				contactEntity.SetDefColumnValues();
				contactEntity.SetColumnValue("TypeId", new Guid("00783EF6-F36B-1410-A883-16D83CAB0980"));
				contactEntity.SetColumnValue("MobilePhone", mobilePhone);
				contactEntity.SetColumnValue("GivenName", name);
				contactEntity.SetColumnValue("LanguageId", new Guid("E35E61EF-CC32-4A97-B98C-52473E35CE60"));
				contactEntity.SetColumnValue("UsrIsCreatedFromService", true);
				contactEntity.SetColumnValue("UsrCreatedByServiceId", serviceId);
				contactEntity.Save(false);

				Guid contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				Logger.WriteToLog("ContactData.CreateContactEntity", $"Contact.Id: {contactId}", "Создан новый контакт", userConnection);
				return contactId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.CreateContactEntity.Exception", $"Contact.MobilePhone: {mobilePhone}", $"Новый контакт не создан. {ex.Message}", userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Создание контакта </summary>
		/// <param name="mobilePhone"> Мобильный телефон </param>
		/// <param name="email"> Почтовый ящик </param>
		/// <param name="serviceId"> Сервис инициатор </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public Guid CreateContactEntity(string name, string mobilePhone, string email, Guid serviceId, UserConnection userConnection)
		{
			try
			{
				Entity contactEntity = userConnection.EntitySchemaManager.GetInstanceByName("Contact").CreateEntity(userConnection);
				contactEntity.SetDefColumnValues();
				contactEntity.SetColumnValue("TypeId", new Guid("00783EF6-F36B-1410-A883-16D83CAB0980"));
				contactEntity.SetColumnValue("MobilePhone", mobilePhone);
				contactEntity.SetColumnValue("Email", email);
				contactEntity.SetColumnValue("GivenName", name);
				contactEntity.SetColumnValue("LanguageId", new Guid("E35E61EF-CC32-4A97-B98C-52473E35CE60"));
				contactEntity.SetColumnValue("UsrIsCreatedFromService", true);
				contactEntity.SetColumnValue("UsrCreatedByServiceId", serviceId);
				contactEntity.Save(false);

				Guid contactId = contactEntity.GetTypedColumnValue<Guid>("Id");
				Logger.WriteToLog("ContactData.CreateContactEntity", $"Contact.Id: {contactId}", "Создан новый контакт", userConnection);
				return contactId;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.CreateContactEntity.Exception", $"Contact.MobilePhone: {mobilePhone}, Contact.Email: {email}", $"Новый контакт не создан. {ex.Message}", userConnection);
				return Guid.Empty;
			}
		}

		/// <summary> Очистка номера телефона </summary>
		/// <param name="phone"> Телефон </param>
		public string CleanPhone(string phone, UserConnection userConnection)
		{
			if (string.IsNullOrWhiteSpace(phone)) { return string.Empty; }

            try
            {
				Regex digitsOnly = new Regex(@"[^\d]");
				string digitsOnlyPhone = digitsOnly.Replace(phone, "");
				return digitsOnlyPhone;
			}
            catch (Exception ex)
            {
				Logger.WriteToLog("ContactData.ClearingPhone.Exception", ex.Message, userConnection);
				return string.Empty;
			}
		}

		/// <summary> Валидация номера телефона </summary>
		/// <param name="phone"> Телефон </param>
		public bool IsValidPhone(string phone, UserConnection userConnection)
		{
			if (string.IsNullOrWhiteSpace(phone)) { return false; }
			if (phone.Length != 12) { return false; }

			try
            {
				bool іsValidPhone = Regex.IsMatch(phone, @"^[3][8][0]\d{9}");
				return іsValidPhone;
			}
            catch (Exception ex)
            {
				Logger.WriteToLog("ContactData.IsValidPhone.Exception", ex.Message, userConnection);
				return false;
			}
		}

		/// <summary> Валидация почтового ящика </summary>
		/// <param name="email"> Почтовый ящик </param>
		/// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
		public bool IsValidEmail(string email, UserConnection userConnection)
        {
			if (string.IsNullOrWhiteSpace(email)) { return false; }

			try
			{
				email = Regex.Replace(email, @"(@)(.+)$", DomainMapper, RegexOptions.None, TimeSpan.FromMilliseconds(200));

				string DomainMapper(Match match)
				{
					var idn = new IdnMapping();
					string domainName = idn.GetAscii(match.Groups[2].Value);
					return match.Groups[1].Value + domainName;
				}

				bool іsValidEmail = Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
				return іsValidEmail;
			}
			catch (Exception ex)
			{
				Logger.WriteToLog("ContactData.IsValidEmail.Exception", ex.Message, userConnection);
				return false;
			}
		}
	}
}