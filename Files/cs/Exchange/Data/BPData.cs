using System;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Core.Process;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Data
{
    public static class BPData
    {
        /// <summary> Запуск бизнес-процесса </summary>
        /// <param name="processSchemaName"> Название бизнес-процесса </param>
        /// <param name="parameters"> Параметры </param>
        /// <param name="userConnection"> Пользовательское подключение, используемое при выполнении запроса </param>
        public static void StartBusinessProcess(string processSchemaName, Dictionary<string, string> parameters, UserConnection userConnection)
        {
            try { ProcessDescriptor processExecutor = userConnection.ProcessEngine.ProcessExecutor.Execute(processSchemaName, parameters); }
            catch (Exception ex) { Logger.WriteToLog("BPData.StartBusinessProcess.Exception", $"processSchemaName: {processSchemaName}", ex.Message, userConnection); }
        }
    }
}
