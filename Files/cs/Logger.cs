using System;
using Terrasoft.Core;
using Terrasoft.Core.DB;

namespace ExternalSystemsIntegration.Files.cs
{
    public static class Logger
    {
        /// <summary> «апись данных в таблицу логировани€ </summary>
        public static bool WriteToLog(string service, string body, UserConnection userConnection)
        {
            try
            {
                Insert insert = new Insert(userConnection).Into("UsrExternalSystemsIntegrationLog")
                .Set("UsrExecution", Column.Parameter(DateTime.Now))
                .Set("UsrService", Column.Parameter(service))
                .Set("UsrBody", Column.Parameter(body));

                insert.Execute();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> «апись данных в таблицу логировани€ </summary>
        public static bool WriteToLog(string service, string entity, string body, UserConnection userConnection)
        {
            try
            {
                Insert insert = new Insert(userConnection).Into("UsrExternalSystemsIntegrationLog")
                .Set("UsrExecution", Column.Parameter(DateTime.Now))
                .Set("UsrService", Column.Parameter(service))
                .Set("UsrEntity", Column.Parameter(entity))
                .Set("UsrBody", Column.Parameter(body));

                insert.Execute();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> «апись данных в таблицу логировани€ заказа </summary>
        public static bool WriteToOrderLog(string service, string body, UserConnection userConnection)
        {
            try
            {
                Insert insert = new Insert(userConnection).Into("UsrExternalSystemsIntegrationOrderLog")
                .Set("UsrExecution", Column.Parameter(DateTime.Now))
                .Set("UsrService", Column.Parameter(service))
                .Set("UsrBody", Column.Parameter(body));

                insert.Execute();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary> «апись данных в таблицу логировани€ заказа </summary>
        public static bool WriteToOrderLog(string service, string entity, string body, UserConnection userConnection)
        {
            try
            {
                Insert insert = new Insert(userConnection).Into("UsrExternalSystemsIntegrationOrderLog")
                .Set("UsrExecution", Column.Parameter(DateTime.Now))
                .Set("UsrService", Column.Parameter(service))
                .Set("UsrEntity", Column.Parameter(entity))
                .Set("UsrBody", Column.Parameter(body));

                insert.Execute();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}