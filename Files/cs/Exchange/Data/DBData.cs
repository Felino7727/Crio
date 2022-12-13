using System;
using System.Collections.Generic;
using Terrasoft.Core;
using Terrasoft.Common;
using Terrasoft.Core.DB;

namespace ExternalSystemsIntegration.Files.cs.Exchange.Data
{
    /// <summary> Работа с базой данных </summary>
    public static class DBData
    {
        /// <summary> Чтение [SELECT TOP(1) Boolean, WHERE Guid] </summary>
        public static bool SelectBooleanByGuid(string returnColumn, string table, string column, Guid value, UserConnection userConnection)
        {
            if (value == Guid.Empty || value == null) { return false; }
            try
            {
                bool returnValue = (new Select(userConnection).Top(1)
                    .Column(returnColumn)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<bool>();
                return returnValue;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.SelectBooleanByGuid.Exception", $"returnColumn: {returnColumn}, table: {table}, column: {column}, value: {value}", ex.Message, userConnection);
                return false;
            }
        }

        /// <summary> Чтение [SELECT TOP(1) Boolean?, WHERE String] </summary>
        public static bool? SelectBooleanByString(string returnColumn, string table, string column, string value, UserConnection userConnection)
        {
            if (value == string.Empty || value == null) { return false; }
            try
            {
                bool? returnValue = (new Select(userConnection).Top(1)
                    .Column(returnColumn)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<dynamic>();
                return returnValue;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.SelectBooleanByString.Exception", $"returnColumn: {returnColumn}, table: {table}, column: {column}, value: {value}", ex.Message, userConnection);
                return false;
            }
        }

        /// <summary> Чтение [SELECT TOP(1) String, WHERE Guid] </summary>
        public static string SelectStringByGuid(string returnColumn, string table, string column, Guid value, UserConnection userConnection)
        {
            if (value == Guid.Empty || value == null) { return string.Empty; }
            try
            {
                string returnValue = (new Select(userConnection).Top(1)
                    .Column(returnColumn)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<string>();
                return returnValue;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.SelectStringByGuid.Exception", $"returnColumn: {returnColumn}, table: {table}, column: {column}, value: {value}", ex.Message, userConnection);
                return string.Empty;
            }
        }

        /// <summary> Чтение [SELECT TOP(1) Guid, WHERE String] </summary>
        public static Guid SelectGuidByString(string returnColumn, string table, string column, string value, UserConnection userConnection)
        {
            if (value == string.Empty || value == null) { return Guid.Empty; }
            try
            {
                Guid returnValue = (new Select(userConnection).Top(1)
                    .Column(returnColumn)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Select).ExecuteScalar<Guid>();
                return returnValue;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.SelectGuidByString.Exception", $"returnColumn: {returnColumn}, table: {table}, column: {column}, value: {value}", ex.Message, userConnection);
                return Guid.Empty;
            }
        }

        /// <summary> Чтение [SELECT List String, WHERE Guid AND Guid] </summary>
        public static List<string> SelectListStringByGuid(string returnColumn, string table, string column1, Guid value1, string column2, Guid value2, UserConnection userConnection)
        {
            try
            {
                List<string> list = new List<string>();
                Select select = new Select(userConnection)
                        .Column(returnColumn)
                        .From(table)
                        .Where(column1).IsEqual(Column.Parameter(value1))
                        .And(column2).IsEqual(Column.Parameter(value2)) as Select;
                select.ExecuteReader(dataReader => { list.Add(dataReader.GetColumnValue<string>(returnColumn)); });
                return list;
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.SelectListStringByGuid.Exception", $"returnColumn: {returnColumn}, table: {table}, column1: {column1}, value1: {value1}, column2: {column2}, value2: {value2}", ex.Message, userConnection);
                return null;
            }
        }

        /// <summary> Удаление [Delete All, WHERE Guid] </summary>
        public static void DeleteAllByGuid(string table, string column, Guid value, UserConnection userConnection)
        {
            try
            {
                Delete delete = new Delete(userConnection)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Delete;
                delete.Execute();
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByGuid", $"{column}: {value}", $"Данные удалены из [dbo.{table}]", userConnection);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByGuid.Exception", ex.Message, userConnection);
            }
        }

        /// <summary> Удаление [Delete All, WHERE String] </summary>
        public static void DeleteAllByString(string table, string column, string value, UserConnection userConnection)
        {
            try
            {
                Delete delete = new Delete(userConnection)
                    .From(table)
                    .Where(column).IsEqual(Column.Parameter(value)) as Delete;
                delete.Execute();
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByString", $"{column}: {value}", $"Данные удалены из [dbo.{table}]", userConnection);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByString.Exception", ex.Message, userConnection);
            }
        }

        /// <summary> Удаление [Delete All, WHERE Guid AND String] </summary>
        public static void DeleteAllByGuidAndString(string table, string column1, Guid value1, string column2, string value2, UserConnection userConnection)
        {
            try
            {
                Delete delete = new Delete(userConnection)
                    .From(table)
                    .Where(column1).IsEqual(Column.Parameter(value1))
                    .And(column2).IsEqual(Column.Parameter(value2)) as Delete;
                delete.Execute();
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByGuidAndString", $"{column1}: {value1}, {column2}: {value2}", $"Данные удалены из [dbo.{table}]", userConnection);
            }
            catch (Exception ex)
            {
                Logger.WriteToLog("Exchange.Data.DBData.DeleteAllByGuidAndString.Exception", ex.Message, userConnection);
            }
        }
    }
}