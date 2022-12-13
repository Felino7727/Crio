using System;
using System.Linq;
using Terrasoft.Core;
using System.Threading.Tasks;
using Terrasoft.Core.Entities;
using Terrasoft.Core.Configuration;
using Terrasoft.Core.Entities.Events;
using ExternalSystemsIntegration.Files.cs.Exchange.Auth;
using ExternalSystemsIntegration.Files.cs.Exchange.Data;

namespace ExternalSystemsIntegration.Files.cs.EventListeners
{
    /// <summary> Слушатель событий сущности [Заказ] </summary>
    [EntityEventListener(SchemaName = "Order")]
    class OrderEventListener : BaseEntityEventListener
    {
        /// <summary> Обработчик события после обновления записи </summary>
        /// <param name="sender"> Ссылка на экземпляр объекта, который генерирует событие </param>
        /// <param name="e"> Аргументы события </param>
        public override void OnUpdated(object sender, EntityAfterEventArgs e)
        {
            base.OnUpdated(sender, e);

            Entity entity = (Entity)sender;
            UserConnection userConnection = entity.UserConnection;

            Guid orderId = e.PrimaryColumnValue;
            bool isStatusIdModified = e.ModifiedColumnValues.Any(column => column.Name == "StatusId");
            if (isStatusIdModified)
            {
                bool listenerEnabled = false;
                try { listenerEnabled = Convert.ToBoolean(SysSettings.GetValue(userConnection, "UsrSwitchOrderEventListenerOnUpdated")); }
                catch (Exception ex) { Logger.WriteToOrderLog("OrderEventListener.OnUpdated.listenerEnabled.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                if (listenerEnabled)
                {
                    Guid statusId = entity.GetTypedColumnValue<Guid>("StatusId");
                    bool isSet = false;
                    try { isSet = DBData.SelectBooleanByGuid("UsrIsSet", "OrderStatus", "Id", statusId, userConnection); }
                    catch (Exception ex) { Logger.WriteToOrderLog("OrderEventListener.OnUpdated.isSet.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                    if (isSet)
                    {
                        string number = entity.GetTypedColumnValue<string>("Number");
                        string usrCode = string.Empty;
                        try { usrCode = DBData.SelectStringByGuid("UsrCode", "OrderStatus", "Id", statusId, userConnection); }
                        catch (Exception ex) { Logger.WriteToOrderLog("OrderEventListener.OnUpdated.usrCode.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                        if (!string.IsNullOrEmpty(usrCode))
                        {
                            try
                            {
                                DateTime tokenLifetime = Convert.ToDateTime(SysSettings.GetValue(userConnection, "UsrTokenLifetimeApiUmStarIs"));
                                if (tokenLifetime < DateTime.Now)
                                {
                                    ApiUmAuth authorization = new ApiUmAuth();
                                    Task resultAuthorization = Task.Run(async () => { await authorization.Authorization(userConnection); });
                                    resultAuthorization.Wait();

                                    OrderData orderData = new OrderData();
                                    Task resultOrderData = Task.Run(async () => { await orderData.SetOrderStatus(number, usrCode, userConnection); });
                                    resultOrderData.Wait();
                                }
                                else
                                {
                                    OrderData orderData = new OrderData();
                                    Task resultOrderData = Task.Run(async () => { await orderData.SetOrderStatus(number, usrCode, userConnection); });
                                    resultOrderData.Wait();
                                }
                            }
                            catch (Exception ex) { Logger.WriteToOrderLog("OrderEventListener.OnUpdated.Exception", $"Id: {orderId}", ex.Message, userConnection); }
                        }
                    }
                }
            }
        }
    }
}
