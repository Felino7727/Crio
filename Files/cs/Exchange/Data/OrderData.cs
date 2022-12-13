using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Configuration;
using Newtonsoft.Json;
using ExternalSystemsIntegration.Files.cs.Exchange.Auth;
using ExternalSystemsIntegration.Files.cs.Exchange.DTO;
using System.Net;
using System.IO;
using Terrasoft.Core.Process;
using Terrasoft.Core.DB;
using Terrasoft.Common;
using System.Threading;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Data
{
    class OrderData
    {
        #region SetOrderStatus
        /// <summary> Установка состояния заказа </summary>
        /// <param name="number"> Номер заказа </param>
        /// <param name="usrCode"> Код состояния заказа </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task SetOrderStatus(string number, string usrCode, UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmMethodSetOrderStatus"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrTokenApiUmStarIs"));
            }
            catch (Exception ex) { Logger.WriteToOrderLog("SetOrderStatus.SysSettings.Exception", $"Number: {number}", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { SrcDocID = number, StateOM = usrCode }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    SetOrderStatusResponse response = JsonConvert.DeserializeObject<SetOrderStatusResponse>(responseStream);

                    if (response.Status != "ok") 
                    {
                        bool? isResend = DBData.SelectBooleanByString("UsrIsResend", "UsrErrorSendOrderStatus", "UsrCode", response.ErrorMessage, userConnection);

                        if (isResend == true)
                        {
                            Logger.WriteToOrderLog($"SetOrderStatus.Response.{response.Status}", $"Number: {number}", $"StateOM: {usrCode}, isResend: true, responseStream: { responseStream}", userConnection);

                            Dictionary<string, string> parameters = new Dictionary<string, string> { { "orderSrcDocID", number } };
                            BPData.StartBusinessProcess("UsrReExportOrderStatus", parameters, userConnection);
                        }
                        else if (isResend == null)
                        {
                            Logger.WriteToOrderLog($"SetOrderStatus.Response.{response.Status}", $"Number: {number}", $"StateOM: {usrCode}, isResend: null, responseStream: { responseStream}", userConnection);

                            string chatId = Convert.ToString(SysSettings.GetValue(userConnection, "UsrChatIdForInformingAboutExportDataError"));
                            SetExceptionChatB24(chatId, $"[b][Creatio][/b]\n[b]SrcDocID:[/b] {number}, [b]StateOM:[/b] {usrCode}\n[b]Exception:[/b] Значение {response.ErrorMessage} отсутствует в справочнике \"Ошибка отправки состояния заказа\"", userConnection);
                        }
                        else if (isResend == false)
                        {
                            Logger.WriteToOrderLog($"SetOrderStatus.Response.{response.Status}", $"Number: {number}", $"StateOM: {usrCode}, isResend: false, responseStream: { responseStream}", userConnection);
                        }
                    }
                }
                catch (HttpRequestException ex)
                { 
                    Logger.WriteToOrderLog("SetOrderStatus.EnsureSuccessStatusCode", $"Number: {number}", $"StateOM: {usrCode}, Exception: {ex.Message}", userConnection);

                    string chatId = Convert.ToString(SysSettings.GetValue(userConnection, "UsrChatIdForInformingAboutExportDataError"));
                    SetExceptionChatB24(chatId, $"[b][Creatio].[OrderData.SetOrderStatus.EnsureSuccessStatusCode][/b]\n[b]SrcDocID:[/b] {number}, [b]StateOM:[/b] {usrCode}\n[b]Exception:[/b] {ex.Message}", userConnection);

                    Dictionary<string, string> parameters = new Dictionary<string, string> { { "orderSrcDocID", number } };
                    BPData.StartBusinessProcess("UsrReExportOrderStatus", parameters, userConnection);
                }
                catch (Exception ex) 
                { 
                    Logger.WriteToOrderLog("SetOrderStatus.Exception", $"Number: {number}", $"StateOM: {usrCode}, Exception: {ex.Message}", userConnection);

                    string chatId = Convert.ToString(SysSettings.GetValue(userConnection, "UsrChatIdForInformingAboutExportDataError"));
                    SetExceptionChatB24(chatId, $"[b][Creatio].[OrderData.SetOrderStatus.Exception][/b]\n[b]SrcDocID:[/b] {number}, [b]StateOM:[/b] {usrCode}\n[b]Exception:[/b] {ex.Message}", userConnection);

                    Dictionary<string, string> parameters = new Dictionary<string, string> { { "orderSrcDocID", number } };
                    BPData.StartBusinessProcess("UsrReExportOrderStatus", parameters, userConnection);
                }
            }
            else { Logger.WriteToOrderLog("SetOrderStatus.SysSettings", $"Number: {number}", $"IsNullOrEmpty in SysSettings. UsrUriApiUmMethodSetOrderStatus: {uri}, UsrTokenApiUmStarIs: {token}", userConnection); }
        }

        /// <summary> Отправка сообщения в чат B24 </summary>
        /// <param name="dialogId"> Id чата </param>
        /// <param name="message"> Сообщение </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void SetExceptionChatB24(string dialogId, string message, UserConnection userConnection)
        {
            try
            {
                string url = "https://b24.eva.ua/rest/12811/t1wznm4p4rsv6twi/im.message.add?DIALOG_ID=" + dialogId + "&MESSAGE=" + message;
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "POST";
                HttpWebResponse response =  (HttpWebResponse)request.GetResponse();
                using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                {
                    string responseString = stream.ReadToEnd();
                    Logger.WriteToOrderLog("SetExceptionChatB24", $"dialogId: {dialogId}", $"Message: {message}, Response: {responseString}", userConnection);
                }
                response.Close();
                request = null;
            }
            catch (Exception ex) { Logger.WriteToOrderLog("SetExceptionChatB24.Exception", $"dialogId: {dialogId}", $"Message: {message}, Exception: {ex.Message}", userConnection); }
        }
        #endregion

        #region GetOrderData
        /// <summary> Подготовка данных для запроса данных по заказу </summary>
        /// <param name="dateLastExport"> Дата и время с которой импортировать данные </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        /// /// <param name="usrResponseTimeout"> Системная настройка (время до завершения)</param>
        /// <returns> string.Empty если успех, ex.Message если возникло исключение </returns>
        public string PrepareDataToGetOrderData(DateTime? dateLastExport, UserConnection userConnection, int usrResponseTimeout)
        {
            try
            {
                DateTime tokenLifetime = Convert.ToDateTime(SysSettings.GetValue(userConnection, "UsrTokenLifetimeApiUmStarIs"));
                if (tokenLifetime < DateTime.Now)
                {
                    try
                    {
                        CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
                        CancellationToken token = cancelTokenSource.Token;

                        ApiUmAuth authorization = new ApiUmAuth();

                        Task resultAuthorization = Task.Run(async () => { await authorization.Authorization(userConnection); });                        
                        resultAuthorization.Wait();

                        Task getOrderStatus = Task.Run(async () => { 
                            if (!token.IsCancellationRequested)
                                token.ThrowIfCancellationRequested(); 

                            await GetOrderData(dateLastExport, userConnection); 
                        }, token);

                        try
                        {                            
                            getOrderStatus.Start();
                            Thread.Sleep(usrResponseTimeout * 1000);
                            cancelTokenSource.Cancel();
                            getOrderStatus.Wait();

                            try
                            {
                                double timeOffsetStep = Convert.ToDouble(SysSettings.GetValue(userConnection, "UsrTimeOffsetStepApiUmMethodGetOrderData"));
                                dateLastExport = DateTime.Now.AddSeconds(timeOffsetStep);
                                SysSettings.SetDefValue(userConnection, "UsrDateLastExportApiUmMethodGetOrderData", dateLastExport);
                            }
                            catch (Exception ex)
                            {
                                Logger.WriteToOrderLog("PrepareDataToGetOrderData.SysSettings.SetDefValue.Exception", ex.Message, userConnection);
                                return ex.Message;
                            }
                        }
                        catch (AggregateException agg)
                        {
                            foreach (Exception e in agg.InnerExceptions)
                            {
                                if (e is TaskCanceledException)
                                {
                                    Logger.WriteToOrderLog("TaskCanceledException", "Долгая обработка", userConnection);
                                }
                                else
                                    Logger.WriteToOrderLog("TaskCanceledException", e.Message, userConnection); ;
                            }
                        }
                        finally
                        {
                            cancelTokenSource.Dispose();
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToOrderLog("PrepareDataToGetOrderData.Authorization + GetOrderData.Exception", ex.Message, userConnection);
                        return ex.Message;
                    }
                }
                else
                {
                    try
                    {
                        Task getOrderStatus = Task.Run(async () => { await GetOrderData(dateLastExport, userConnection); });
                        getOrderStatus.Wait();

                        try
                        {
                            double timeOffsetStep = Convert.ToDouble(SysSettings.GetValue(userConnection, "UsrTimeOffsetStepApiUmMethodGetOrderData"));
                            dateLastExport = DateTime.Now.AddSeconds(timeOffsetStep);
                            SysSettings.SetDefValue(userConnection, "UsrDateLastExportApiUmMethodGetOrderData", dateLastExport);
                        }
                        catch (Exception ex)
                        {
                            Logger.WriteToOrderLog("PrepareDataToGetOrderData.SysSettings.SetDefValue.Exception", ex.Message, userConnection);
                            return ex.Message;
                        }                        
                    }
                    catch (Exception ex)
                    {
                        Logger.WriteToOrderLog("PrepareDataToGetOrderData.GetOrderData.Exception", ex.Message, userConnection);
                        return ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToOrderLog("PrepareDataToGetOrderData.SysSettings.GetValue.Exception", ex.Message, userConnection);
                return ex.Message;
            }

            return string.Empty;
        }

        /// <summary> Запрос данных по заказу </summary>
        /// <param name="dateLastExport"> Дата и время с которой импортировать данные </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task GetOrderData(DateTime? dateLastExport, UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmMethodGetOrderData"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrTokenApiUmStarIs"));
                if (dateLastExport == default) { dateLastExport = Convert.ToDateTime(SysSettings.GetValue(userConnection, "UsrDateLastExportApiUmMethodGetOrderData")); }
            }
            catch (Exception ex) { Logger.WriteToOrderLog("GetOrderData.SysSettings.GetValue.Exception", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { DateLastExport = dateLastExport }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    GetOrderDataResponse response = JsonConvert.DeserializeObject<GetOrderDataResponse>(responseStream);
                    
                    if (response.Status == "ok")
                    {
                        Logger.WriteToOrderLog($"GetOrderData.Response.{response.Status}", responseStream, userConnection);
                        foreach (OrderDataResponse data in response.Data) { ProcessingOrderData(data, userConnection); }
                    }
                    else { Logger.WriteToOrderLog($"GetOrderData.Response.{response.Status}", $"Error: [{response.Error}], ErrorDescription: [{response.ErrorDescription}],  Message: [{response.Message}], CauseMessage: [{response.CauseMessage}]", userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("GetOrderData.EnsureSuccessStatusCode", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("GetOrderData.Exception", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("GetOrderData.SysSettings", $"IsNullOrEmpty in SysSettings. UsrUriApiUmMethodGetOrderData: {uri}, UsrTokenApiUmStarIs: {token}, UsrDateLastExportApiUmMethodGetOrderData: {dateLastExport}", userConnection); }
        }

        /// <summary> Обработка данных по заказу </summary>
        /// <param name="data"> Данные для обработки </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void ProcessingOrderData(OrderDataResponse data, UserConnection userConnection)
        {
            Entity entity = userConnection.EntitySchemaManager.GetInstanceByName("Order").CreateEntity(userConnection);
            if (entity.FetchFromDB("Number", data.SrcDocId))
            {
                try
                {
                    Guid statusEntityId = DBData.SelectGuidByString("Id", "OrderStatus", "UsrCode", data.StateOm, userConnection);

                    Guid currentStatusId = entity.GetTypedColumnValue<Guid>("StatusId");
                    bool finalStatus = DBData.SelectBooleanByGuid("FinalStatus", "OrderStatus", "Id", currentStatusId, userConnection);
                    if (finalStatus == false)
                    {
                        bool isGet = DBData.SelectBooleanByGuid("UsrIsGet", "OrderStatus", "Id", statusEntityId, userConnection);
                        if (isGet)
                        {
                            entity.SetColumnValue("UsrOTUtilsDocId", data.DocId ?? 0);
                            entity.SetColumnValue("StatusId", statusEntityId != Guid.Empty ? statusEntityId : (Guid?)null);
                            entity.SetColumnValue("UsrTTN", data.TtnNumber ?? string.Empty);
                            entity.SetColumnValue("UsrStock", data.UsrStock ?? string.Empty);
                            entity.SetColumnValue("UsrNumberActiveCard", data.LoyalCard ?? string.Empty);
                            entity.SetColumnValue("UsrSeatCount", data.CaseCount ?? 0);
                            entity.SetColumnValue("UsrDaysToShipment", data.Days ?? 0);

                            Guid causeOfDeliveryProblemId = DBData.SelectGuidByString("Id", "UsrCauseOfDeliveryProblem", "UsrCode", data.ErrorCode, userConnection);
                            entity.SetColumnValue("UsrCauseOfDeliveryProblemId", causeOfDeliveryProblemId != Guid.Empty ? causeOfDeliveryProblemId : (Guid?)null);

                            string currentCity = entity.GetTypedColumnValue<string>("UsrCity");
                            if (currentCity != data.ShippingCity) { entity.SetColumnValue("UsrCity", data.ShippingCity); }

                            string currentDeliveryDepartment = entity.GetTypedColumnValue<string>("UsrDeliveryDepartment");
                            if (currentDeliveryDepartment != data.ShippingAddress) { entity.SetColumnValue("UsrDeliveryDepartment", data.ShippingAddress); }

                            string currentDeliveryAddress = entity.GetTypedColumnValue<string>("DeliveryAddress");
                            if (currentDeliveryAddress != data.ShippingStreet)
                            {
                                entity.SetColumnValue("DeliveryAddress", data.ShippingStreet);
                                entity.SetColumnValue("UsrAddress", data.ShippingStreet);
                            }

                            DateTime unloadDate = entity.GetTypedColumnValue<DateTime>("UsrUnloadDate");
                            if (unloadDate == default && data.ImportDate != null) { entity.SetColumnValue("UsrUnloadDate", data.ImportDate); }
                            entity.SetColumnValue("UsrUploadDate", DateTime.Now);

                            entity.Save(false);
                            Guid entityId = entity.GetTypedColumnValue<Guid>("Id");

                            if (data.StateOm == "4")
                            {
                                Task getOrderDetailData = Task.Run(async () => { await GetOrderDetailData(entityId, data.SrcDocId, userConnection); });
                                getOrderDetailData.Wait();
                            }
                        }
                        else { Logger.WriteToOrderLog("ProcessingOrderData.NotUpdated", $"Number: {data.SrcDocId}", $"Данные по заказу не обновлены. Order.StatusId: {statusEntityId}, StateOm: {data.StateOm}", userConnection); }
                    }
                    else { Logger.WriteToOrderLog("ProcessingOrderData.Cancelled", $"Number: {data.SrcDocId}", $"Данные по заказу не обновлены, у заказа проставлено окончательное состояние. Order.StatusId: {currentStatusId}", userConnection); }

                }
                catch (Exception ex) { Logger.WriteToOrderLog("ProcessingOrderData.Exception", $"Number: {data.SrcDocId}", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("ProcessingOrderData.NotFound", $"Number: {data.SrcDocId}", "Заказ не найден", userConnection); }
        }

        /// <summary> Запрос детальных данных по заказу </summary>
        /// <param name="orderId"> Id заказа </param>
        /// <param name="orderNumber"> Номер заказа </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task GetOrderDetailData(Guid orderId, string orderNumber, UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmMethodGetOrderDetailData"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrTokenApiUmStarIs"));
            }
            catch (Exception ex) { Logger.WriteToOrderLog("GetOrderDetailData.SysSettings.GetValue.Exception", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { SrcDocId = orderNumber }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    GetOrderDetailDataResponse response = JsonConvert.DeserializeObject<GetOrderDetailDataResponse>(responseStream);

                    if (response.Status == "ok")
                    {
                        Logger.WriteToOrderLog($"GetOrderDetailData.Response.{response.Status}", $"Number: {orderNumber}", responseStream, userConnection);
                        foreach (OrderDetailDataResponse data in response.Data) { ProcessingOrderDetailData(orderId, data, userConnection); }
                    }
                    else { Logger.WriteToOrderLog($"GetOrderDetailData.Response.{response.Status}", $"Number: {orderNumber}", $"Error: [{response.Error}], ErrorDescription: [{response.ErrorDescription}]", userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("GetOrderDetailData.EnsureSuccessStatusCode", $"Number: {orderNumber}", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("GetOrderDetailData.Exception", $"Number: {orderNumber}", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("GetOrderDetailData.SysSettings", $"Number: {orderNumber}", $"IsNullOrEmpty in SysSettings. UsrUriApiUmMethodGetOrderDetailData: {uri}, UsrTokenApiUmStarIs: {token}", userConnection); }
        }

        /// <summary> Обработка детальных данных по заказу </summary>
        /// <param name="orderId"> Id заказа </param>
        /// <param name="data"> Данные для обработки </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void ProcessingOrderDetailData(Guid orderId, OrderDetailDataResponse data, UserConnection userConnection)
        {
            Entity entity = userConnection.EntitySchemaManager.GetInstanceByName("OrderProduct").CreateEntity(userConnection);
            Dictionary<string, object> conditionsOrderDetailData = new Dictionary<string, object>() { { "Order", orderId }, { "UsrOrderItemNumber", data.SrcPosID }, };
            if (entity.FetchFromDB(conditionsOrderDetailData))
            {
                try
                {
                    if (data != null)
                    {
                        entity.SetColumnValue("UsrInStock", data.QtyCollected);

                        if (data.QtyCollected == 0)
                        {
                            if (data.DateTimeDelivery == null) { entity.SetColumnValue("Notes", "Нет и не будет"); }
                            else { entity.SetColumnValue("Notes", data.DateTimeDelivery); }
                        }

                        entity.Save(false);
                        Guid entityId = entity.GetTypedColumnValue<Guid>("Id");
                    }
                }
                catch (Exception ex) { Logger.WriteToOrderLog("ProcessingOrderData.Exception", $"Number: {data.SrcDocId}", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("ProcessingOrderDetailData", $"Number: {data.SrcDocId}", $"Order.Id: {orderId}, OrderProduct.UsrOrderItemNumber: {data.SrcPosID}, OrderProduct.UsrSKU: {data.ProdID} не найден в dbo.OrderProduct", userConnection); }
        }

        #endregion

        #region GetGiftCertificateData
        /// <summary> Подготовка данных для запроса данных по подарочному сертификату </summary>
        /// <param name="orderId"> Id заказа </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void PrepareDataToGetGiftCertificateData(Guid orderId, UserConnection userConnection)
        {
            Guid usrPaymentMethod = new Guid("4f389db1-3416-49b1-8ed9-6253f5b0f255");
            List<string> giftCertificates = DBData.SelectListStringByGuid("UsrSessionIdCertificate", "UsrPaymentsOnOrder", "UsrOrderId", orderId, "UsrPaymentMethodId", usrPaymentMethod, userConnection);
            if (giftCertificates.Count > 0)
            {
                DateTime tokenLifetime = Convert.ToDateTime(SysSettings.GetValue(userConnection, "UsrTokenLifetimeApiUmStarIs"));
                if (tokenLifetime < DateTime.Now)
                {
                    try
                    {
                        ApiUmAuth authorization = new ApiUmAuth();
                        Task resultAuthorization = Task.Run(async () => { await authorization.Authorization(userConnection); });
                        resultAuthorization.Wait();

                        foreach (string giftCertificate in giftCertificates)
                        {
                            Task getOrderStatus = Task.Run(async () => { await GetGiftCertificateData(orderId, giftCertificate, userConnection); });
                            getOrderStatus.Wait();
                        }

                    }
                    catch (Exception ex) { Logger.WriteToOrderLog("PrepareDataToGetGiftCertificateData.Authorization + GetGiftCertificateData.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                }
                else
                {
                    try
                    {
                        foreach (string giftCertificate in giftCertificates)
                        {
                            Task getOrderStatus = Task.Run(async () => { await GetGiftCertificateData(orderId, giftCertificate, userConnection); });
                            getOrderStatus.Wait();
                        }
                    }
                    catch (Exception ex) { Logger.WriteToOrderLog("PrepareDataToGetGiftCertificateData.GetGiftCertificateData.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                }
            }   
        }

        /// <summary> Запрос данных по подарочному сертификату </summary>
        /// <param name="orderId"> Id заказа </param>
        /// <param name="giftCertificate"> Id сертификата </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task GetGiftCertificateData(Guid orderId, string giftCertificate, UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmMethodGetGiftCertificateData"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrTokenApiUmStarIs"));
            }
            catch (Exception ex) { Logger.WriteToOrderLog("GetGiftCertificateData.SysSettings.GetValue.Exception", $"Id: {orderId}", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { DCardID = giftCertificate }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    GetGiftCertificateDataResponse response = JsonConvert.DeserializeObject<GetGiftCertificateDataResponse>(responseStream);

                    if (response.Status != "ок")
                    {
                        Logger.WriteToOrderLog($"GetGiftCertificateData.Response.{response.Status}", $"Id: {orderId}", responseStream, userConnection);
                        foreach (GiftCertificateDataResponse data in response.Data) { ProcessingGiftCertificateData(orderId, data, userConnection); }
                    }
                    else { Logger.WriteToOrderLog($"GetGiftCertificateData.Response.{response.Status}", $"Id: {orderId}", $"Error: [{response.Error}], ErrorDescription: [{response.ErrorDescription}],  Message: [{response.Message}], CauseMessage: [{response.CauseMessage}]", userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("OrderData.GetGiftCertificateData.EnsureSuccessStatusCode", $"Id: {orderId}", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("GetGetGiftCertificateData.Exception", $"Id: {orderId}", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("GetGiftCertificateData.SysSettings", $"Id: {orderId}", $"IsNullOrEmpty in SysSettings. UsrUriApiUmMethodGetGiftCertificateData: {uri}, UsrTokenApiUmStarIs: {token}", userConnection); }
        }

        /// <summary> Обработка данных по подарочному сертификату </summary>
        /// <param name="orderId"> Id заказа </param>
        /// <param name="data"> Данные для обработки </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void ProcessingGiftCertificateData(Guid orderId, GiftCertificateDataResponse data, UserConnection userConnection)
        {
            try
            {
                Guid giftCertificateStatusEntityId = DBData.SelectGuidByString("Id", "UsrGiftCertificateStatus", "UsrCode", data.Status, userConnection);
                Guid accountEntityId = DBData.SelectGuidByString("Id", "Account", "Code", data.OurId, userConnection);
                Guid usrUsedInOrderId = DBData.SelectGuidByString("Id", "Order", "Number", data.SrcDocId, userConnection);

                Entity entity = userConnection.EntitySchemaManager.GetInstanceByName("UsrGiftCertificate").CreateEntity(userConnection);
                Dictionary<string, object> conditionsGiftCertificate = new Dictionary<string, object>() { { "UsrOrder", orderId }, { "UsrCertificate", data.DCardId }, };

                if (entity.FetchFromDB(conditionsGiftCertificate))
                {
                    entity.SetColumnValue("UsrStatusId", giftCertificateStatusEntityId != Guid.Empty ? giftCertificateStatusEntityId : (Guid?)null);
                    entity.SetColumnValue("UsrAccountId", accountEntityId != Guid.Empty ? accountEntityId : (Guid?)null);
                    entity.SetColumnValue("UsrAmount", data.Value ?? null);
                    entity.SetColumnValue("UsrExpirationDate", data.DateEnd ?? null);
                    entity.SetColumnValue("UsrPurchaseDate", data.DateRepayment ?? null);
                    entity.SetColumnValue("UsrUsedInOrderId", usrUsedInOrderId != Guid.Empty ? usrUsedInOrderId : (Guid?)null);
                    entity.Save(false);
                    Guid entityId = entity.GetTypedColumnValue<Guid>("Id");
                    Logger.WriteToOrderLog("ProcessingGiftCertificateData", $"Number: {data.SrcDocId}", $"Обновлены данные по подарочному сертификату: {data.DCardId}", userConnection);
                }
                else
                {
                    entity.SetColumnValue("UsrOrderId", orderId != Guid.Empty ? orderId : (Guid?)null);
                    entity.SetColumnValue("UsrStatusId", giftCertificateStatusEntityId != Guid.Empty ? giftCertificateStatusEntityId : (Guid?)null);
                    entity.SetColumnValue("UsrAccountId", accountEntityId != Guid.Empty ? accountEntityId : (Guid?)null);
                    entity.SetColumnValue("UsrCertificate", data.DCardId ?? string.Empty);
                    entity.SetColumnValue("UsrAmount", data.Value ?? null);
                    entity.SetColumnValue("UsrExpirationDate", data.DateEnd ?? null);
                    entity.SetColumnValue("UsrPurchaseDate", data.DateRepayment ?? null);
                    entity.SetColumnValue("UsrUsedInOrderId", usrUsedInOrderId != Guid.Empty ? usrUsedInOrderId : (Guid?)null);
                    entity.Save(false);
                    Guid entityId = entity.GetTypedColumnValue<Guid>("Id");
                    Logger.WriteToOrderLog("ProcessingGiftCertificateData", $"Number: {data.SrcDocId}", $"Добавлены данные по подарочному сертификату: {data.DCardId}", userConnection);
                }
            }
            catch (Exception ex) { Logger.WriteToOrderLog("ProcessingGiftCertificateData.Exception", $"Number: {data.SrcDocId}", ex.Message, userConnection); }
        }
        #endregion

        #region GetProduct
        /// <summary> Подготовка данных для запроса данных по продукту </summary>
        /// <param name="productId"> Id продукта </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void PrepareDataToGetProductData(Guid productId, UserConnection userConnection)
        {
            string sku = DBData.SelectStringByGuid("Code", "Product", "Id", productId, userConnection);
            DateTime tokenLifetime = Convert.ToDateTime(SysSettings.GetValue(userConnection, "UsrTokenLifetimeApiUmStarIs"));
            if (tokenLifetime < DateTime.Now)
            {
                try
                {
                    ApiUmAuth authorization = new ApiUmAuth();
                    Task resultAuthorization = Task.Run(async () => { await authorization.Authorization(userConnection); });
                    resultAuthorization.Wait();

                    Task getProductData = Task.Run(async () => { await GetProductData(productId, sku, userConnection); });
                    getProductData.Wait();

                }
                catch (Exception ex) { Logger.WriteToOrderLog("PrepareDataToGetProductData.Authorization + GetProductData.Exception", ex.Message, userConnection); }
            }
            else
            {
                try
                {
                    Task getOrderStatus = Task.Run(async () => { await GetProductData(productId, sku, userConnection); });
                    getOrderStatus.Wait();
                }
                catch (Exception ex) { Logger.WriteToOrderLog("PrepareDataToGetProductData.GetProductData.Exception", ex.Message, userConnection); }
            }
        }

        /// <summary> Запрос данных по продукту </summary>
        /// <param name="productId"> Id продукта </param>
        /// <param name="sku"> SKU продукта </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public async Task GetProductData(Guid productId, string sku, UserConnection userConnection)
        {
            string uri = string.Empty;
            string token = string.Empty;
            try
            {
                uri = Convert.ToString(SysSettings.GetValue(userConnection, "UsrUriApiUmMethodGetProductData"));
                token = Convert.ToString(SysSettings.GetValue(userConnection, "UsrTokenApiUmStarIs"));
            }
            catch (Exception ex) { Logger.WriteToOrderLog("OrderData.GetProductData.SysSettings.GetValue.Exception", $"Product.Id: {productId}", ex.Message, userConnection); }

            if (!string.IsNullOrEmpty(uri) && !string.IsNullOrEmpty(token))
            {
                try
                {
                    HttpClient client = new HttpClient();
                    client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
                    HttpContent content = new StringContent(JsonConvert.SerializeObject(new { TypeSelect = 1, ProdIDSTR = sku }));
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    HttpResponseMessage requestStream = await client.PostAsync(new Uri(uri), content);
                    requestStream.EnsureSuccessStatusCode();
                    string responseStream = await requestStream.Content.ReadAsStringAsync();
                    GetProductDataResponse response = JsonConvert.DeserializeObject<GetProductDataResponse>(responseStream);

                    if (response.Status != "ок")
                    {
                        Logger.WriteToOrderLog($"GetProductData.Response.{response.Status}", $"Product.Id: {productId}", responseStream, userConnection);
                        ProcessingProductData(productId, response, userConnection);
                    }
                    else { Logger.WriteToOrderLog($"GetProductData.Response.{response.Status}", $"Product.Id: {productId}", $"Error: [{response.Error}], ErrorDescription: [{response.ErrorDescription}],  Message: [{response.Message}], CauseMessage: [{response.CauseMessage}]", userConnection); }
                }
                catch (HttpRequestException ex) { Logger.WriteToOrderLog("GetProductData.EnsureSuccessStatusCode", $"Product.Id: {productId}", ex.Message, userConnection); }
                catch (Exception ex) { Logger.WriteToOrderLog("GetProductData.Exception", $"Product.Id: {productId}", ex.Message, userConnection); }
            }
            else { Logger.WriteToOrderLog("GetProductData.SysSettings", $"Product.Id: {productId}", $"IsNullOrEmpty in SysSettings. UsrUriApiUmMethodGetProductData: {uri}, UsrTokenApiUmStarIs: {token}", userConnection); }
        }

        /// <summary> Обработка данных по продукту </summary>
        /// <param name="productId"> Id продукта </param>
        /// <param name="response"> Данные для обработки </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public void ProcessingProductData(Guid productId, GetProductDataResponse response, UserConnection userConnection)
        {
            Guid tradeMarkEntityId = FindOrCreateTradeMark(productId, response.Data[0].BrandId, response.Data[0].BrandName, userConnection);

            foreach (ProductDataResponse data in response.Data)
            {
                try
                {
                    Guid accountEntityId = DBData.SelectGuidByString("Id", "Account", "Code", data.OurId, userConnection);
                    if (accountEntityId == Guid.Empty)
                    {
                        Logger.WriteToOrderLog("ProcessingProductData.Account", $"Product.Id: {productId}", $"Цена и остаток по продукту не добавлены, так как Account.Code: {data.OurId}, отсутствует в dbo.Account", userConnection);
                        continue;
                    }

                    Entity priceAndBalanceInStoreEntity = userConnection.EntitySchemaManager.GetInstanceByName("UsrPriceAndBalanceInStore").CreateEntity(userConnection);
                    Dictionary<string, object> conditionsPriceAndBalanceInStore = new Dictionary<string, object>() { { "UsrProduct", productId }, { "UsrAccount", accountEntityId }, };
                    if (priceAndBalanceInStoreEntity.FetchFromDB(conditionsPriceAndBalanceInStore))
                    {
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrPrice", data.PriceMc);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrQuantity", data.Qty);
                        priceAndBalanceInStoreEntity.Save(false);
                        Guid priceAndBalanceInStoreEntityId = priceAndBalanceInStoreEntity.GetTypedColumnValue<Guid>("Id");
                    }
                    else
                    {
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrProductId", productId);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrAccountId", accountEntityId);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrTradeMarkId", tradeMarkEntityId);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrPrice", data.PriceMc);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrQuantity", data.Qty);
                        
                        Guid regionEntityId = DBData.SelectGuidByString("Id", "Region", "UsrCode", data.RegionId, userConnection);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrRegionId", regionEntityId);
                        Guid cityEntityId = FindOrCreateCity(data.CityId, data.CityName, regionEntityId, userConnection);
                        priceAndBalanceInStoreEntity.SetColumnValue("UsrCityId", cityEntityId);

                        priceAndBalanceInStoreEntity.Save(false);
                        Guid priceAndBalanceInStoreEntityId = priceAndBalanceInStoreEntity.GetTypedColumnValue<Guid>("Id");
                    }
                }
                catch (Exception ex) { Logger.WriteToOrderLog("ProcessingProductData.Exception", $"Product.Id: {productId}", $"Account.Code: {data.OurId}, Exception: {ex.Message}", userConnection); }
            }
        }

        /// <summary> Поиск или создание торговой марки </summary>
        /// <param name="productId"> Id продукта </param>
        /// <param name="brandId"> Id торговой марки </param>
        /// <param name="brandName"> Название торговой марки </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public Guid FindOrCreateTradeMark(Guid productId, string brandId, string brandName, UserConnection userConnection)
        {
            Entity tradeMarkEntity = userConnection.EntitySchemaManager.GetInstanceByName("TradeMark").CreateEntity(userConnection);
            try
            {
                if (!tradeMarkEntity.FetchFromDB("UsrCode", brandId))
                {
                    tradeMarkEntity.SetDefColumnValues();
                    tradeMarkEntity.SetColumnValue("Name", brandName);
                    tradeMarkEntity.SetColumnValue("UsrCode", brandId);
                    tradeMarkEntity.Save(false);
                    Guid tradeMarkEntityId = tradeMarkEntity.GetTypedColumnValue<Guid>("Id");
                    Logger.WriteToOrderLog("FindOrCreateTradeMark", $"Product.Id: {productId}", $"В справочник добавлена торговая марка: {brandName}, TradeMark.Id: {tradeMarkEntityId}", userConnection);
                    return tradeMarkEntityId;
                }
                else
                {
                    Guid tradeMarkEntityId = tradeMarkEntity.GetTypedColumnValue<Guid>("Id");
                    return tradeMarkEntityId;
                }
            }
            catch (Exception ex)
            {
                Logger.WriteToOrderLog("FindOrCreateTradeMark.Exception", $"Product.Id: {productId}", $"UsrCode: {brandId}, Exception: {ex.Message}", userConnection);
                return Guid.Empty;
            }
        }

        /// <summary> Поиск или создание населенного пункта </summary>
        /// <param name="cityId"> Id населенного пункта </param>
        /// <param name="cityName"> Название населенного пункта </param>
        /// <param name="regionEntityId"> Id области </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public Guid FindOrCreateCity(string cityId, string cityName, Guid regionEntityId, UserConnection userConnection)
        {
            try
            {
                Dictionary<string, object> conditionsCityAndRegion = new Dictionary<string, object>() { { "Region", regionEntityId }, { "Name", cityName }, };
                Entity cityEntity = userConnection.EntitySchemaManager.GetInstanceByName("City").CreateEntity(userConnection);
                if (cityEntity.FetchFromDB("UsrCode", cityId))
                {
                    Guid cityEntityId = cityEntity.GetTypedColumnValue<Guid>("Id");
                    return cityEntityId;
                }
                else if (cityEntity.FetchFromDB(conditionsCityAndRegion))
                {
                    cityEntity.SetColumnValue("CountryId", new Guid("A470B005-E8BB-DF11-B00F-001D60E938C6"));
                    cityEntity.SetColumnValue("RegionId", regionEntityId);
                    cityEntity.SetColumnValue("UsrCode", cityId);
                    cityEntity.SetColumnValue("UsrActive", true);
                    cityEntity.SetColumnValue("UsrWeight", 1);
                    cityEntity.Save(false);
                    Guid cityEntityId = cityEntity.GetTypedColumnValue<Guid>("Id");
                    return cityEntityId;
                }
                else
                {
                    cityEntity.SetColumnValue("CountryId", new Guid("A470B005-E8BB-DF11-B00F-001D60E938C6"));
                    cityEntity.SetColumnValue("RegionId", regionEntityId);
                    cityEntity.SetColumnValue("Name", cityName);
                    cityEntity.SetColumnValue("UsrCode", cityId);
                    cityEntity.SetColumnValue("UsrActive", true);
                    cityEntity.SetColumnValue("UsrWeight", 1);
                    cityEntity.Save(false);
                    Guid cityEntityId = cityEntity.GetTypedColumnValue<Guid>("Id");
                    return cityEntityId;
                }
            }
            catch (Exception ex) 
            { 
                Logger.WriteToOrderLog("SearchCity.Exception", $" City.Name: {cityName}, City.UsrCode: {cityId}", ex.Message, userConnection); 
                return Guid.Empty; 
            }
        }
        #endregion
    }
}