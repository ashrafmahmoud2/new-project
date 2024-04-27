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
                clsSQLDate.GenerateDataLayer( columnInfo, dbName);
        }

    }
}
