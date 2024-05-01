using CodeGenDataLayer;
using GenerateDataAccessLayerLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CodeGenBusinessLayer
{
    public static class clsSQL
    {       
        public static DataTable GetAllDatabaseNames()
        {
            return clsSQLDate.GetAllDatabaseNames();

        }

        public static DataTable GetTablesNameByDB(String DataBase)
        {
            return clsSQLDate.GetTablesNameByDB(DataBase);

        }
        
        public static DataTable GetTableInfo(String TableName,string DBName)
        {
            return clsSQLDate.GetTableInfo(TableName, DBName);

        }

        public static string GenerateDataAccessSettings(string DBName)
        {
            return clsSQLDate.GenerateDataAccessSettings(DBName);
        }

        public static string GenerateDataLayer(string dbName, List<List<clsColumnInfoForDataAccess>> columnInfo)
        {
            return 
                clsGenerateDataLayer_Data.GenerateDataLayer( columnInfo, dbName);
        }

        public static string GenerateBusinessLayer(string dbName, List<List<clsColumnInfoForDataAccess>> columnInfo)
        {
            return
                clsGenerateBusinessLayer_Data.GenerateBusinessLayer(columnInfo, dbName);
        }
        
        public static string GenerateStoredProcedure(string dbName, List<List<clsColumnInfoForDataAccess>> columnInfo)
        {
            return
                clsGenerateStoredProcedureData.GenerateGenerateStoredProcedure(columnInfo, dbName);
        }
       
        public static string GenerateErrorLogger(string DBName)
        {
            return clsSQLDate.GenerateErrorLogger(DBName);
        }

        public static string GenerateLogHandler(string DBName)
        {
            return clsSQLDate.GenerateLogHandler(DBName);
        }

        public static string GenerateDelegateHelperMethods(string DBName)
        {
            return clsGenerateDelegateHelperMethod_Data.GenerateDelegateHelperMethods(DBName);
        }

        public static bool GenerateDataAccessInFilePath(string DBName, string DataAccessPath)
        {
            return clsGenerateDataLayer_Data.GenerateDataAccessInFilePath(DBName,DataAccessPath);
        }

        public static List<string> GetTablesNameByDBByList(string DBName)
        {
           return clsSQLDate.GetTablesNameByDBByList(DBName);
        }

        public static bool GenerateBusinessClasInFilePath(string DBName, string DataAccessPath)
        {
            return clsGenerateBusinessLayer_Data.GenerateBusinessClasInFilePath(DBName, DataAccessPath);
        }
        
        public static bool GenerateAppconfigInFilePath( string DataAccessPath, string DBName)
        {
            return clsSQLDate.GenerateAppconfigInFilePath(DataAccessPath,DBName );
        }
        
        public static bool GenerateStoredProceduresToAllTables( string DBName)
        {
            return clsGenerateStoredProcedureData.GenerateStoredProceduresToAllTables(DBName);
        }
        
        public static bool GenerateStoredProceduresToSelectedTable(string DBName,string SelectedTable )
        {
            return clsGenerateStoredProcedureData.GenerateStoredProceduresToSelectedTable(DBName, SelectedTable);
        }


    }
}
